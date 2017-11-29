# Everything you need to know about async method internals

The C# language is great for developer's productivity and I'm glad for the recent push towards making it more suitable for high-performance applications. 

Here is an example: C# 5 introduced 'async' methods. The feature is very useful from a user's point of view because it helps combining several task-based operations into one. But this abstraction comes at a cost. Tasks are reference types causing heap allocations everywhere they're created, even in cases where the 'async' method completes synchronously. With C# 7, async methods can return task-like types such as `ValueTask` to reduce the number of heap allocations or avoid them altogether in some scenarios.

In order to understand how all of this is possible, we need to look under the hood and see how async methods are implemented.

But first, a little bit of history.

Classes `Task` and `Task<T>` were introduced in .NET 4.0 and, from my perspective, made a huge mental shift in area of asynchronous and parallel programming in .NET. Unlike older asynchronous patterns such as the `BeginXXX`/`EndXXX` pattern from .NET 1.0 (also known as ["Asynchronous Programming Model"](https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/asynchronous-programming-model-apm)) or [Event-based Asynchronous Pattern](https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/event-based-asynchronous-pattern-overview) like [`BackgroundWorker`](https://docs.microsoft.com/en-us/dotnet/framework/winforms/controls/backgroundworker-component) class from .NET 2.0, tasks are composable.

A task represents a unit of work with a promise to give you results back in the future. That promise could be backed by IO-operation or represent a computation-intensive operation. It doesn't matter. What does matter is that the result of the operation is self-sufficient and is a first-class citizen. You can pass a "future" around: you can store it in a variable, return it from a method, or pass it to another method. You can "join" two "futures" together to form another one, you can wait for results synchronously or you can "await" the result by adding "continuation" to the "future". You can decide what to do if the operation succeeded, failed or was canceled, just using a task instance alone.

Task Parallel Library (TPL) had changed the way we think about concurrency and C# 5 language made a step forward by introducing `async`/`await`. Async/await helps to compose tasks and gives the user an ability to use well-known constructs like `try/catch`, `using` etc. But like any other abstraction `async/await` feature has its cost. And to understand what the cost is, you have to look under the hood.

## Async method internals

A regular method has just one entry point and one exit point (it could have more than one `return` statement but at the runtime there is just one exist point for a given call). But async methods (*) and iterators (methods with `yield return`) are different. In the case of an async method, a method caller can get the result (i.e. `Task` or `Task<T>`) almost immediately and then "await" the actual result of the method via the resulting task.

(*) I'm using the term "async method" in one specific way: a method marked with contextual keyword `async`. It doesn't mean that the entire method is asynchronous. It doesn't mean that the method is asynchronous at all. It only means that the compiler performs some special transformation to the method.

Let's consider the following async method:

```csharp
class StockPrices
{
    private Dictionary<string, decimal> _stockPrices;
    public async Task<decimal> GetStockPriceForAsync(string companyId)
    {
        await InitializeMapIfNeededAsync();
        _stockPrices.TryGetValue(companyId, out var result);
        return result;
    }

    private async Task InitializeMapIfNeededAsync()
    {
        if (_stockPrices == null)
            return;

        await Task.Delay(42);
        // Getting the stock prices from the external source.
        _stockPrices = new Dictionary<string, decimal> { {"MSFT", 42} };
    }
}
```

Method `GetStockPriceForAsync` ensures that the cache is initialized and then gets the value from the cache. This is a common pattern when the method is IO bound and relatively slow if the cache is cold and is way more efficient for all subsequent invocations.

To better understand what the compiler does or can do, let's try to write a transformation by hand.

## Deconstructing an async method by hand

The TPL provides two main building blocks that helps us constructing and joining tasks: task continuation using `Task.ContinueWith` and `TaskCompletionSource<T>` class for constructing tasks by hand.

```csharp
class GetStockPriceForAsync_StateMachine
{
    enum State { Start, Step1, }
    private readonly StockPrices @this;
    private readonly string _companyId;
    private readonly TaskCompletionSource<decimal> _tcs;
    private Task _initializeMapIfNeededTask;
    private State _state = State.Start;

    public GetStockPriceForAsync_StateMachine(StockPrices @this, string companyId)
    {
        this.@this = @this;
        _companyId = companyId;
    }

    public void Start()
    {
        try
        {
            if (_state == State.Start)
            {
                // The Code from the start of the method to the first 'await'.

                if (string.IsNullOrEmpty(_companyId))
                    throw new ArgumentNullException();
                
                _initializeMapIfNeededTask = @this.InitializeMapIfNeeded();

                // Schedule continuation
                _state = State.Step1;
                _initializeMapIfNeededTask.ContinueWith(_ => Start());
            }
            else if (_state == State.Step1)
            {
                // Need to check the error and the cancel case first
                if (_initializeMapIfNeededTask.Status == TaskStatus.Canceled)
                    _tcs.SetCanceled();
                else if (_initializeMapIfNeededTask.Status == TaskStatus.Faulted)
                    _tcs.SetException(_initializeMapIfNeededTask.Exception.InnerException);
                else
                {
                    // The code between first await and the rest of the method

                    @this._store.TryGetValue(_companyId, out var result);
                    _tcs.SetResult(result);
                }
            }
        }
        catch (Exception e)
        {
            _tcs.SetException(e);
        }
    }

    public Task<decimal> Task => _tcs.Task;
}

public Task<decimal> GetStockPriceForAsync(string companyId)
{
    var stateMachine = new GetStockPriceForAsync_StateMachine(this, companyId);
    stateMachine.Start();
    return stateMachine.Task;
}
```

The code is verbose but relatively straightforward. All the logic from `GetStockPriceForAsync` is moved to `GetStockPriceForAsync_StateMachine.Start` method that uses ["continuation passing style"](https://en.wikipedia.org/wiki/Continuation-passing_style). The original method is split into pieces using `await` statements as a separator. The first block is the code from the start of the method to the first `await`. The second block - from the first `await` to the second `await`. The third block - from the second `await` to the third one or until the end of the method, and so forth:

```csharp
// State 1 of the generated state machine:

if (string.IsNullOrEmpty(_companyId)) throw new ArgumentNullException();
_initializeMapIfNeededTask = @this.InitializeMapIfNeeded();
```

Every "awaitable" task now become a field of the state machine, and the `Start` method subscribe itself as a continuation of it:

```csharp
_state = State.Step1;
_initializeMapIfNeededTask.ContinueWith(_ => Start());
```

Then, when the task finishes, the `Start` method is called back, the `_state` field is checked to understand what stage we're in. The logic then checks whether the task was finished successfully, was canceled, or successful. In the latter case, the state machine moves forward and runs the next block of code. When everything is done, the state machine sets the result of the `TaskCompletionSource<T>` instance and the resulting task returned from `GetStockPricesForAsync` changes its state to completion.

```csharp
@this._store.TryGetValue(_companyId, out var result);
_tcs.SetResult(result); // The caller gets the result back
```

This "implementation" has few serious drawbacks:
* Lots of heap allocations: 1 allocation for the state machine, 1 allocation for `TaskCompletionSource<T>`, 1 allocation for task inside a `TaskCompletionSource<T>`, 1 allocation for continuation delegate.
* Lack of "hot path optimizations": if the "awaitable" task was already finished there is no sense for creating a continuation.
* Lack of extensibility: the implementation is tightly coupled with Task-based classes that makes impossible other scenarios, like awaiting other types or returning types other than `Task` or `Task<T>`.

Now let's take a look at the actual async machinery to see how these concerns are addressed.

## Async machinery

The overall approach that compiler takes for async method transformation is very similar to one mentioned above. To get the desired behavior the compiler relies on the following types:

1. Generated state machine that acts like a stack frame for an asynchronous method and contains all the logic from the original async method (**).
2. [`AsyncTaskMethodBuilder<T>`](http://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/AsyncMethodBuilder.cs,5916df9e324fc0a1) that keeps the completed task (very similar to `TaskCompletionSource<T>` type) and manages the state transition of the state machine.
3. [`TaskAwaiter<T>`](http://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/TaskAwaiter.cs,2c48fb3bdfc69022) that wraps a task and schedules a continuation of it if needed.
4. [`MoveNextRunner`](http://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/AsyncMethodBuilder.cs,c7b2691b131812c7) that knows how to call [`IStateMachine.MoveNext`](http://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/IAsyncStateMachine.cs,22e8d7df64651ee3) method in a correct execution context.

**The generated state machine is a class in debug mode and a struct in release mode**. All the other types (except `MoveNextRunner` class) are defined in the BCL as structs.

(**) The compiler generate type name like `<YourMethodNameAsync>d__1`. To avoid name collisions the generated name contains invalid character and can't be defined or referenced by the user. But for simplicity sake in all the following examples, I will use valid identifiers by replacing `<` and `>` characters with `_` or simplifying them a little bit.

### The original method

Original "asynchronous" method creates a state machine instance, initializes it with the captured state (including `this` pointer if the method is not static) and then starts the execution by calling [`AsyncTaskMethodBuilder<T>.Start`](http://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/AsyncMethodBuilder.cs,454) with the state machine instance passed by reference.

```csharp
[AsyncStateMachine(typeof(_GetStockPriceForAsync_d__1))]
public Task<decimal> GetStockPriceForAsync(string companyId)
{
    _GetStockPriceForAsync_d__1 _GetStockPriceForAsync_d__;
    _GetStockPriceForAsync_d__.__this = this;
    _GetStockPriceForAsync_d__.companyId = companyId;
    _GetStockPriceForAsync_d__.__builder = AsyncTaskMethodBuilder<decimal>.Create();
    _GetStockPriceForAsync_d__.__state = -1;
    AsyncTaskMethodBuilder<decimal> __builder = _GetStockPriceForAsync_d__.__builder;
    __builder.Start<_GetStockPriceForAsync_d__1>(ref _GetStockPriceForAsync_d__);
    return _GetStockPriceForAsync_d__.__builder.Task;
}
```

**Passing by reference** is an important optimization, because a state machine tends to be fairly large struct (>100 bytes) and passing it by reference avoids a redundant copy.

### The state machine

The generated state machine looks complicated, but in essence, it is very similar to one we created by hand:

```csharp
struct _GetStockPriceForAsync_d__1 : IAsyncStateMachine
{
    public StockPrices __this;
    public string companyId;
    public AsyncTaskMethodBuilder<decimal> __builder;
    public int __state;
    private TaskAwaiter __task1Awaiter;

    public void MoveNext()
    {
        decimal result;
        try
        {
            TaskAwaiter awaiter;
            if (__state != 0)
            {
                // State 1 of the generated state machine:
                if (string.IsNullOrEmpty(companyId))
                    throw new ArgumentNullException();

                awaiter = __this.InitializeLocalStoreIfNeededAsync().GetAwaiter();
                        
                // Hot path optimization: if the task is completed,
                // the state machine automatically moves to the next step
                if (!awaiter.IsCompleted)
                {
                    __state = 0;
                    __task1Awaiter = awaiter;
                            
                    // The following call will eventually cause boxing of the state machine.
                    __builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
                    return;
                }
            }
            else
            {
                awaiter = __task1Awaiter;
                __task1Awaiter = default(TaskAwaiter);
                __state = -1;
            }

            // GetResult returns void, but it'll throw if the awaited task failed.
            // This exception is catched later and changes the resulting task.
            awaiter.GetResult();
            __this._stocks.TryGetValue(companyId, out result);
        }
        catch (Exception exception)
        {
            // Final state: failure
            __state = -2;
            __builder.SetException(exception);
            return;
        }

        // Final state: success
        __state = -2;
        __builder.SetResult(result);
    }

    void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
    {
        __builder.SetStateMachine(stateMachine);
    }
}
```

Even though the state machine is similar to a hand-crafted one it has a few very important differences:

**1. "Hot path" optimization**

Unlike our naive approach, the generated state machine is aware that an awaited task could be completed already.

```csharp
awaiter = __this.InitializeLocalStoreIfNeededAsync().GetAwaiter();
                        
// Hot path optimization: if the task is completed,
// the state machine automatically moves to the next step
if (!awaiter.IsCompleted)
{
    // Irrelevant stuff

    // The following call will eventually cause boxing of the state machine.
    __builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
    return;
}
```

If the awaited task is already finished (successfully or not) the state machine moves forward to the next step:

```csharp
awaiter.GetResult();
__this._stocks.TryGetValue(companyId, out result);

```

**It means that if all awaited tasks are already completed the entire state machine will leave on the stack**. An async method even today could have an extremely small memory overhead if all awaited tasks are completed already or will be completed synchroonously. The only remaining allocation will be for the task itself!

**2. Error handling**
As you may see there is no special logic that covers faulted or canceled state of the awaited tasks. The state machine calls `awaiter.GetResult()` that will throw `TaskCancelledException` if the task was canceled or another exception if the task was failed. This is a quite elegant solution that works fine here because `GetResult()` is a bit different in terms of error handling than `task.Wait()` or `task.Result`.

Both `task.Wait()` and `task.Result` throw `AggregateException` even when there is just one exception that caused a task to fail. The reason for this is pretty simple: a task can represent not only IO-bound operation that usually has just one failure but the result of a parallel computation as well. In the latter case, the operation can have more than one error and `AggregateException` is designed to carry all these errors in one place.

But `async/await` pattern is designed specifically for asynchronous operations that usually have at most one error. So the language authors decided that it will make more sense if `awaiter.GetResult()` will "unwrap" an `AggregateException` and throw just the first failure. This design decision is not perfect and in one of the next posts, we'll see when this abstraction can leak.

The async state machine represents just one piece of the puzzle. To understand the whole picture we need to know how a state machine instance interacts with `TaskAwaiter<T>` and `AsyncTaskMethodBuilder<T>`.

### How different pieces are glued together?

(https://github.com/SergeyTeplyakov/DissectingTheCode/blob/master/posts/Images/Async_sequence_state_machine.png "Async workflow")

The chart looks overly complicated but each piece is well-design and plays an important role. The most interesting collaboration is happening when an awaited task is not finished (marked with the brown rectangle in the diagram):

* The state machine calls [`__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);`](http://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/AsyncMethodBuilder.cs,535) to register itself as the task's continuation.
* The builder makes sure that when the task is finished a [`IAsyncStateMachine.MoveNext`](http://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/IAsyncStateMachine.cs,25) method gets called:
    * The builder [captures](http://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/AsyncMethodBuilder.cs,916) the current `ExecutionContext` and creates a [`MoveNextRunner`](http://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/AsyncMethodBuilder.cs,049bf1fe30f53fe3) instance to associate it with the current state machine instance. Then it creates an `Action` instance from [`MoveNextRunner.Run`](http://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/AsyncMethodBuilder.cs,8539e615974ae806) that will move the state machine forward under the captured execution context.
    * The builder calls `TaskAwaiter.UnsafeOnCompleted(action)` that schedules a given action as a continuation of an awaited task.

When the awaited task completes, the given callback is called and the state machine runs the next code block of the asynchronous method.

### Execution Context

One may wonder: what is the execution context and why we need all that complexity?

In the synchronous world, each thread keeps ambient information in a thread-local storage. It can be security-related information, culture-specific data, or something else. When 3 methods are called sequentially in one thread this information flows naturally between all of them. But this is no longer true for asynchronous methods. Each "section" of an asynchronous method can be executed in different threads that makes thread-local information unusable.

Execution context keeps the information for one logical flow of control even when it spans multiple threads.

Methods like `Task.Run` or `ThreadPool.QueueUserWorkItem` do this automatically. `Task.Run` method captures `ExecutionContext` from the invoking thread and stores it with the `Task` instance. When the `TaskScheduler` associated with the task runs a given delegate, it runs it via `ExecutionContext.Run` using the stored context.

We can use [`AsyncLocal<T>`](http://referencesource.microsoft.com/#mscorlib/system/threading/asynclocal.cs,ef9ce034697240ba) to demonstrate this concept in action:

```csharp
static Task ExecutionContextInAction()
{
    var li = new AsyncLocal<int>();
    li.Value = 42;

    return Task.Run(() =>
    {
        // Task.Run restores the execution context
        Console.WriteLine("In Task.Run: " + li.Value);
    }).ContinueWith(_ =>
    {
        // The continuation restores the execution context as well
        Console.WriteLine("In Task.ContinueWith: " + li.Value);
    });
}
```

In these cases, the execution context flows through `Task.Run` and then to `Task.ContinueWith` method. So if you run this method you'll see:

```
In Task.Run: 42
In Task.ContinueWith: 42
```

But not all methods in the BCL will automatically capture and restore the execution context. Two exceptions are `TaskAwaiter<T>.UnsafeOnComplete` and `AsyncMethodBuilder<T>.AwaitUnsafeOnComplete`. It looks weird that the language authors decided to add "unsafe" methods to flow the execution context manually using `AsyncMethodBuilder<T>` and `MoveNextRunner` instead of relying on a built-in facilities like [`AwaitTaskContinuation`](http://referencesource.microsoft.com/#mscorlib/system/threading/Tasks/TaskContinuation.cs,543). I suspect there were some performance reasons or anther restrictions on the existing implementation.

Here is an example that demonstrates the difference:

```csharp
static async Task ExecutionContextInAsyncMethod()
{
    var li = new AsyncLocal<int>();
    li.Value = 42;
    await Task.Delay(42);

    // The context is implicitely captured. li.Value is 42
    Console.WriteLine("After first await: " + li.Value);

    var tsk2 = Task.Yield();
    tsk2.GetAwaiter().UnsafeOnCompleted(() =>
    {
        // The context is not captured: li.Value is 0
        Console.WriteLine("Inside UnsafeOnCompleted: " + li.Value);
    });

    await tsk2;

    // The context is captured: li.Value is 42
    Console.WriteLine("After second await: " + li.Value);
}
```

The output is:

```
After first await: 42
Inside UnsafeOnCompleted: 0
After second await: 42
```

## Conclusion

* Async methods are very different from the synchronous methods.
* The compiler generates a state machine per each method and moves all the logic of the original method there.
* The generated code is highly optimized for a synchronous scenario: if all awaited tasks are completed, then the overhead of an async method is minimal.
* If an awaited task is not completed, the logic relies on a lot of helper types to get the job done.

## References
If you want to learn more about execution context, I highly recommend the following two blog pots:

* [ExecutionContext vs SynchronizationContext] by Stephen Toub(https://blogs.msdn.microsoft.com/pfxteam/2012/06/15/executioncontext-vs-synchronizationcontext/) and
* [Implicit Async Context ("AsyncLocal")](https://blog.stephencleary.com/2013/04/implicit-async-context-asynclocal.html) by Stephen Cleary

**Next**: we'll explore an extensibility model of asynchronous methods in C#.