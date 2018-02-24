# The Performance Characteristics of Async Methods

In the last two blog posts we've covered [the internals of async methods](https://blogs.msdn.microsoft.com/seteplia/2017/11/30/dissecting-the-async-methods-in-c/) in C# and then we looked at [the extensibility points](https://blogs.msdn.microsoft.com/seteplia/2018/01/11/extending-the-async-methods-in-c/) the C# compiler provides to adjust the behavior of async methods. Today we're going to explore the performance characteristics of async methods.

As you should already know from the [first post of the series](https://blogs.msdn.microsoft.com/seteplia/2017/11/30/dissecting-the-async-methods-in-c/), the compiler does a lot of transformations to make asynchronous programming experience very similar to a synchronous one. But to do that the compiler creates a state machine instance, pass it around to an async method builder, that calls task awaiter etc. Obviously, all of that logic has its own cost, but how much do we pay?

Back in pre-TPL days, asynchronous operations usually were fairly coarse-grained so the overhead of an asynchronous operation was likely negligible. But today even relatively simple application could have hundreds if not thousands asynchronous operations per second. The TPL was designed with this workload in mind but it's not magic, it has some overhead.

To measure an overhead of async methods will use a slightly modified example that we used in the first blog post.

```csharp
public class StockPrices
{
    private const int Count = 100;
    private List<(string name, decimal price)> _stockPricesCache;

    // Async version
    public async Task<decimal> GetStockPriceForAsync(string companyId)
    {
        await InitializeMapIfNeededAsync();
        return DoGetPriceFromCache(companyId);
    }

    // Sync version that calls async init
    public decimal GetStockPriceFor(string companyId)
    {
        InitializeMapIfNeededAsync().GetAwaiter().GetResult();
        return DoGetPriceFromCache(companyId);
    }

    // Purely sync version
    public decimal GetPriceFromCacheFor(string companyId)
    {
        InitializeMapIfNeeded();
        return DoGetPriceFromCache(companyId);
    }

    private decimal DoGetPriceFromCache(string name)
    {
        foreach (var kvp in _stockPricesCache)
        {
            if (kvp.name == name)
            {
                return kvp.price;
            }
        }

        throw new InvalidOperationException($"Can't find price for '{name}'.");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void InitializeMapIfNeeded()
    {
        // Similar initialization logic.
    }

    private async Task InitializeMapIfNeededAsync()
    {
        if (_stockPricesCache != null)
        {
            return;
        }

        await Task.Delay(42);

        // Getting the stock prices from the external source.
        // Generate 1000 items to make cache hit somewhat expensive
        _stockPricesCache = Enumerable.Range(1, Count)
            .Select(n => (name: n.ToString(), price: (decimal)n))
            .ToList();
        _stockPricesCache.Add((name: "MSFT", price: 42));
    }
}
```

`StockPrices` class populates the cache with the stock prices from an external source and provides an API to query it. The main difference from the first post's example is the switch from the dictionary to a list of prices. To measure the overhead of different forms of async methods compared to synchronous ones the operation itself should do at least some work and linear search of stock prices models this aspect.

`GetPricesFromCache` is intentionally built using a plain loop to avoid any allocations.

## Synchronous vs. Task-based asynchronous versions

In the first benchmark, we comparing async method that calls async initialization method (`GetStockPriceForAsync`), a synchronous method that calls asynchronous initialization method (`GetStockPriceFor`) and synchronous method that calls synchronous initialization method.

```csharp
private readonly StockPrices _stockPrices = new StockPrices();

public SyncVsAsyncBenchmark()
{
    // Warming up the cache
    _stockPrices.GetStockPriceForAsync("MSFT").GetAwaiter().GetResult();
}

[Benchmark]
public decimal GetPricesDirectlyFromCache()
{
    return _stockPrices.GetPriceFromCacheFor("MSFT");
}

[Benchmark(Baseline = true)]
public decimal GetStockPriceFor()
{
    return _stockPrices.GetStockPriceFor("MSFT");
}

[Benchmark]
public decimal GetStockPriceForAsync()
{
    return _stockPrices.GetStockPriceForAsync("MSFT").GetAwaiter().GetResult();
}
```

The results are:

```
                     Method |     Mean | Scaled |  Gen 0 | Allocated |
--------------------------- |---------:|-------:|-------:|----------:|
 GetPricesDirectlyFromCache | 2.177 us |   0.96 |      - |       0 B |
           GetStockPriceFor | 2.268 us |   1.00 |      - |       0 B |
      GetStockPriceForAsync | 2.523 us |   1.11 | 0.0267 |      88 B |
```

This data is already very interesting:

* The async method is rather fast. `GetPricesForAsync` completes synchronously in this benchmark and it's about 15% (*) slower than the purely synchronous method.
* The synchronous `GetPricesFor` method that calls asynchronous `InitializeMapIfNeededAsync` method has even lower overhead, but the most surprising thing that it does **not allocate at all** (Allocated column in the previous table has 0 for both `GetPricesDirectlyFromCache` and for `GetStockPriceFor`).

(*) Of course, you can't say that the overhead of async machinery when the async method runs synchronously is 15% for all possible cases. The percentage is very specific to the amount of work the method is doing. Measuring a pure method call overhead for async method (that does nothing) with a synchronous method (that does nothing) will show a huge difference. The idea of this benchmark is to show that the overhead of an async method that does a relatively small amount of work is moderate.

How is it possible that the call to `InitializeMapIfNeededAsync` caused no allocations at all? I've mentioned in the first post of the series that the async method *have to* allocate at least one object in the managed head - the task instance itself. Let's explore this aspect.

## Optimization #1. Cache the task instance if possible

The answer to the previous question is very simple: **`AsyncMethodBuilder` uses a single task instance for every successfully completed async operations**. An async method that returns `Task` relies on `AsyncMethodBuilder` that has the following logic in [`SetResult`](http://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/AsyncMethodBuilder.cs,378) method:

```csharp
// AsyncMethodBuilder.cs from mscorlib
public void SetResult()
{
    // I.e. the resulting task for all successfully completed
    // methods is the same -- s_cachedCompleted.
    m_builder.SetResult(s_cachedCompleted);
}
```

`SetResult` method is called only for async methods that completed successfully and **the successful result for every `Task`-based method can be easily shared**. We can even observe this behavior with the following test:

```csharp
[Test]
public void AsyncVoidBuilderCachesResultingTask()
{
    var t1 = Foo();
    var t2 = Foo();

    // These two tasks are equal
    Assert.AreEqual(t1, t2);
            
    async Task Foo() { }
}
```

But this is not the only optimization that could happen. `AsyncTaskMethodBuilder<T>` does a similar optimization: it caches tasks for `Task<bool>` and for some other primitive types. For instance, it caches all the default values for a bunch of integral types and has a special cache for `Task<int>` for the values in range [-1; 9) (see `AsyncTaskMethodBuilder<T>.GetTaskForResult()` for more details).

The following test proofs that this is indeed the case:

```csharp
[Test]
public void AsyncTaskBuilderCachesResultingTask()
{
    // These values are cached
    Assert.AreSame(Foo(-1), Foo(-1));
    Assert.AreSame(Foo(8), Foo(8));

    // But these are not
    Assert.AreNotSame(Foo(9), Foo(9));
    Assert.AreNotSame(Foo(int.MaxValue), Foo(int.MaxValue));

    async Task<int> Foo(int n) => n;
}
```

You **should not rely on this behavior too much** but its good to know that the language and framework authors try their best to fine-tune performance in every possible way. Caching a task is a common optimization pattern that is used in other places as well. For instance, new [`Socket`](https://github.com/dotnet/corefx/blob/12e6bb4a7f525323b827e3ee0d26bdd2691c6a34/src/System.Net.Sockets/src/System/Net/Sockets/Socket.Tasks.cs#L27) implementation in [corefx repo](https://github.com/dotnet/corefx/) heavily relies on this optimization and uses [cached tasks](https://github.com/dotnet/corefx/blob/12e6bb4a7f525323b827e3ee0d26bdd2691c6a34/src/System.Net.Sockets/src/System/Net/Sockets/Socket.Tasks.cs#L575) whenever possible.

## Optimization #2: use `ValueTask`
The optimization mentioned above works only in a few cases. Instead of relying on it, we can use `ValueTask<T>` (**): a special task-like value type that will not allocate if the method completes synchronously.

`ValueTask<T>` is effectively a discriminated union of `T` and `Task<T>`: if the "value task" is completed then the underlying value would be used. If the underlying promise is not finished yet, then the task would be allocated.

This special type helps to avoid unnecessary heap allocations when the operation completes synchronously. To use `ValueTask<T>` we just need to change the return type of `GetStockPriceForAsync` from `Task<decimal` to `ValueTask<decimal>`:

```csharp
public async ValueTask<decimal> GetStockPriceForAsync(string companyId)
{
    await InitializeMapIfNeededAsync();
    return DoGetPriceFromCache(companyId);
}
```

And now we can measure the difference with this additional benchmark:

```csharp
[Benchmark]
public decimal GetStockPriceWithValueTaskAsync()
{
    return _stockPrices.GetStockPriceValueTaskForAsync("MSFT").GetAwaiter().GetResult();
}
```

```
                          Method |     Mean | Scaled |  Gen 0 | Allocated |
-------------------------------- |---------:|-------:|-------:|----------:|
      GetPricesDirectlyFromCache | 1.260 us |   0.90 |      - |       0 B |
                GetStockPriceFor | 1.399 us |   1.00 |      - |       0 B |
           GetStockPriceForAsync | 1.552 us |   1.11 | 0.0267 |      88 B |
 **GetStockPriceWithValueTaskAsync | 1.519 us |   1.09 |      - |       0 B |**
 ```

As you may see the `ValueTask`-based version is just a bit faster than the `Task`-based version. The main difference is the lack of heap allocations. We'll discuss in a moment whether it worth doing this switch or not, but before that, I would like cover one tricky optimization.

## Optimization #3: avoid async machinery on a common path

If you have an extremely widely used async method and you want to reduce the overhead even more, you may consider the following optimization: you can remove `async` modifier, check the task's state inside the method and perform the entire operation synchronously without dealing with async machinery at all.

Sounds complicated? Let's look at the example.

```csharp
public ValueTask<decimal> GetStockPriceWithValueTaskAsync_Optimized(string companyId)
{
    var task = InitializeMapIfNeededAsync();

    if (task.IsCompleted)
    {
        // Optimizing a common case: no async machinery involved.
        return new ValueTask<decimal>(DoGetPriceFromCache(companyId));
    }

    return DoGetStockPricesForAsync(task, companyId);

    async ValueTask<decimal> DoGetStockPricesForAsync(Task initializeTask, string localCompanyId)
    {
        await initializeTask;
        return DoGetPriceFromCache(localCompanyId);
    }
}
```

In this case, the method `GetStockPriceWithValueTaskAsync_Optimized` does not have `async` modifier and when it gets the task from `InitializeMapIfNeededAsync` method it checks whether the task is completed or not. If the task is completed, it just calls `DoGetPriceFromCache` to get the results immediately. But if the initialization task is still running, it calls the local function to await the results.

Using a local function is not the only option but one of the simplest one. But there is a caveat. The most natural implementation of the local function would capture an enclosing state: the local variable and the argument:

```csharp
public ValueTask<decimal> GetStockPriceWithValueTaskAsync_Optimized(string companyId)
{
    // Oops! This will lead to a closure allocation at the beginning of the method!
    var task = InitializeMapIfNeededAsync();

    // Optimizing for a common case: no async machinery involved.
    if (task.IsCompleted)
    {
        return new ValueTask<decimal>(DoGetPriceFromCache(companyId));
    }

    return DoGetStockPricesForAsync();

    async ValueTask<decimal> DoGetStockPricesForAsync()
    {
        // Use locals and arguments from the enclosing method
        await task;
        return DoGetPriceFromCache(companyId);
    }
}
```

But unfortunately, due to [a compiler bug](https://github.com/dotnet/roslyn/issues/18946) this code would allocate a closure even when the method completes on a common path. Here how this method looks under the hood:

```csharp
public ValueTask<decimal> GetStockPriceWithValueTaskAsync_Optimized(string companyId)
{
    var closure = new __DisplayClass0_0()
    {
        __this = this,
        companyId = companyId,
        task = InitializeMapIfNeededAsync()
    };

    if (closure.task.IsCompleted)
    {
        return ...
    }

    // The rest of the code
}
```

As we discussed in ["Dissecting the local functions in C#"](https://blogs.msdn.microsoft.com/seteplia/2017/10/03/dissecting-the-local-functions-in-c-7/) the compiler uses the shared closure instance for all locals/arguments in the given scope. So this code generation kind-of makes sense, but it makes the whole fight with heap allocations useless.

**TIP** This optimization is very tricky. The benefits are very small and even you write the original local function **right** you can easily make the change in the future and accidentally capture an enclosing state causing a heap allocation. You still may use the optimization if you work on a highly reusable library like BCL on a method that will be definitely used on hot paths.

## The overhead of awaiting the task
So far we've covered only one specific case: the overhead of an async method that completes synchronously. This was intentional. "Smaller" the async method is, more visible the overhead would be for its overall performance. Fine-grained async methods tend to do less work and tend to complete synchronously more often. And we tend to call them more frequently.

But we should know the overhead of the async machinery when a method "awaits" a non-completed task. To measure this overhead we change `InitializeMapIfNeededAsync` to call `Task.Yield()` even when the cache is initialized:

```csharp
private async Task InitializeMapIfNeededAsync()
{
    if (_stockPricesCache != null)
    {
        await Task.Yield();
        return;
    }

    // Old initialization logic
}
```

Let's extend our performance benchmark suite with the following methods:

```csharp
[Benchmark]
public decimal GetStockPriceFor_Await()
{
    return _stockPricesThatYield.GetStockPriceFor("MSFT").GetAwaiter().GetResult();
}

[Benchmark]
public decimal GetStockPriceForAsync_Await()
{
    return _stockPricesThatYield.GetStockPriceForAsync("MSFT").GetAwaiter().GetResult();
}

[Benchmark]
public decimal GetStockPriceWithValueTaskAsync_Await()
{
    return _stockPricesThatYield.GetStockPriceValueTaskForAsync("MSFT").GetAwaiter().GetResult();
}
```

```
                                    Method |      Mean | Scaled |  Gen 0 |  Gen 1 | Allocated |
------------------------------------------ |----------:|-------:|-------:|-------:|----------:|
                          GetStockPriceFor |  2.332 us |   1.00 |      - |      - |       0 B |
                     GetStockPriceForAsync |  2.505 us |   1.07 | 0.0267 |      - |      88 B |
           GetStockPriceWithValueTaskAsync |  2.625 us |   1.13 |      - |      - |       0 B |
                    GetStockPriceFor_Await |  6.441 us |   2.76 | 0.0839 | 0.0076 |     296 B |
               GetStockPriceForAsync_Await | 10.439 us |   4.48 | 0.1577 | 0.0122 |     553 B |
     GetStockPriceWithValueTaskAsync_Await | 10.455 us |   4.48 | 0.1678 | 0.0153 |     577 B |
 ```

As we can see the difference is way more visible, both in terms of speed and memory. Here is a short explanation of the results.

* Each 'await' operation for an unfinished task takes about 4us and allocates almost 300B (**) per invocation. This explains why `GetStockPriceFor` is almost twice as fast than `GetStockPriceForAsync` and why its allocate less memory.
* A `ValueTask`-based async method is a bit slower than a `Task`-based async method when the method is not completed synchronously. The state machine of a `ValueTask<T>`-based method needs to keep more data compared to a state machine for a `Task<T>`-based method.

(**) It depends on the platform (x64 vs. x86), and a number of local variables/arguments of the async method.

## Async methods performance 101
* If the async method completes synchronously the performance overhead is fairly small.
* If the async method completes synchronously the following memory overhead will occur: for `async Task` methods there is no overhead, for `async Task<T>` methods the overhead is 88 bytes per operation (on x64 platform).
* `ValueTask<T>` can remove the overhead mentioned above for async methods that complete synchronously.
* A `ValueTask<T>`-based async method is a bit faster than a `Task<T>`-based method if the method completes synchronously and a bit slower otherwise.
* A performance overhead of async methods that await non-completed task is way more substantial (~300 bytes per operation on x64 platform).

And, as always, measure first. If you see that an async operation causes a performance problem, you may switch from `Task<T>` to `ValueTask<T>`, cache a task or make a common execution path synchronous if possible. But you may also try to make your async operations coarser grained. This can improve performance, simplify debugging and overall make your code easier to reason. **Not every small piece of code has to be asynchronous**.

## Additional references
* [Dissecting the async methods in C#](https://blogs.msdn.microsoft.com/seteplia/2017/11/30/dissecting-the-async-methods-in-c/)
* [Extending the async methods in C#](https://blogs.msdn.microsoft.com/seteplia/2018/01/11/extending-the-async-methods-in-c/)
* [Stephen Toub's comment about `ValueTask`'s usage scenarios](https://github.com/dotnet/corefx/issues/4708#issuecomment-160658188)
* ["Dissecting the local functions in C#"](https://blogs.msdn.microsoft.com/seteplia/2017/10/03/dissecting-the-local-functions-in-c-7/)


OLD
## Async perf characteristics 101
* The overhead of an async method that completes synchronously is barely noticeable for a vast majority of applications.
* If an async method is too slow (and you that from the profiling session with a real workflow), consider making async operations coarser grained.
* If you have a widely used async method that completes synchronously most of the time and task allocations has reasonable performance impact, consider switching to `ValueTask<T>`.
* Switching from `Task<T>` to `ValueTask<T>` most likely would have the same perf characteristics with memory improvements (TODO: finish)
* If one of the async methods is on extreme hot path, consider using a trick that will avoid async machinery.