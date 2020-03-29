# One user scenario to rule them all

**The async series**
* [Dissecting the async methods in C#](https://blogs.msdn.microsoft.com/seteplia/2017/11/30/dissecting-the-async-methods-in-c/).
* [Extending the async methods in C#](https://blogs.msdn.microsoft.com/seteplia/2018/01/11/extending-the-async-methods-in-c/).
* [The performance characteristics of async methods in C#](https://blogs.msdn.microsoft.com/seteplia/2018/01/25/the-performance-characteristics-of-async-methods/).
* **One user scenario to rule them all**.

Almost every non-trivial behavior of the async methods in C# can be explained based on one user scenario: **migration of the existing synchronous code to asynchronous should be as simple as possible**. You should be able to add `async` keyword before a method's return type, add `Async` suffix to its name, add `await` keyword here and there in the method body to get a fully functional asynchronous method.

IMAGE

This "simple" scenario drastically affects the behavior of asynchronous methods in many different ways: from scheduling task's continuations to exception handling. The scenario sounds plausible and important but it made simplicity behind the async methods very deceptive.

## Synchronization context

UI development is one of the areas where the scenario mentioned above was especially important. Long-running operations in the UI thread make applications unresponsive and asynchronous programming was always considered a good fit there.

```csharp
private async void buttonOk_ClickAsync(object sender, EventArgs args)
{
    textBox.Text = "Running.."; // 1 -- UI Thread
    var result = await _stockPrices.GetStockPricesForAsync("MSFT"); // 2 -- Usually non-UI Thread
    textBox.Text = "Result is: " + result; //3 -- Should be UI Thread
}
```

The code looks very simple, but now we have a problem. Most UI frameworks have the restrictions that only a dedicate thread can change the UI elements. This means that line 3 would fail if a task's continuation is scheduled on a thread pool's thread. Luckily this issue was relatively old and starting from .NET Framework 2.0 the notion of [Synchronization Contexts](https://msdn.microsoft.com/en-us/library/system.threading.synchronizationcontext(v=vs.80).aspx) was introduced.

Each UI framework provides special utilities for marshaling work into a dedicated UI thread (or threads). Windows Forms relies on [`Control.Invoke`](https://msdn.microsoft.com/en-us/library/system.windows.forms.control.invoke(v=vs.110).aspx), WPF - on [Dispatcher.Invoke](https://msdn.microsoft.com/en-us/library/system.windows.threading.dispatcher.invoke(v=vs.110).aspx) and yet another framework may rely on something else. The concept is similar in all the cases, but the underlying details are different. Synchronization context abstracts away the differences and provides an API for running the code in a "special" context leaving the details to the derived types like [`WindowsFormsSynchronizationContext`](http://referencesource.microsoft.com/#System.Windows.Forms/winforms/Managed/System/WinForms/WindowsFormsSynchronizationContext.cs,c7dfb662bbd6227d), [`DispatcherSynchronizationContext`](http://referencesource.microsoft.com/#WindowsBase/Base/System/Windows/Threading/DispatcherSynchronizationContext.cs) etc.

To solve the thread-affinity problem the C# language authors decided to capture a current synchronization context at the beginning of the async methods and schedule all the continuations into the captured context. Now every block between `await` statements is executed in the UI thread that made the main scenario possible. But the solution introduced a whole bunch of other challenges.

## Deadlocks

Let's review a small and relatively simple piece of code. Are there any issues here?

```csharp
// UI code
private void buttonOk_Click(object sender, EventArgs args)
{
    textBox.Text = "Running..";
    var result = _stockPrices.GetStockPricesForAsync("MSFT").Result;
    textBox.Text = "Result is: " + result;
}

// StockPrices.dll
public Task<decimal> GetStockPricesForAsync(string symbol)
{
    await Task.Yield();
    return 42;
}
```

**The code will cause a deadlock**. The UI thread starts an async operation and synchronously waits for the result. But the async method can't be finished because the second line of `GetStockPricesForAsync` is should run in the UI thread **causing the deadlock**.

You may argue, that the issue is relatively easy to spot, and I'll agree with you. Any calls to `Task.Result` or `Task.Wait` should be banned from the UI code, but the problem still possible if the component that the UI code relies on, synchronously waits for an async operation's result:

```csharp
// UI code
private void buttonOk_Click(object sender, EventArgs args)
{
    textBox.Text = "Running..";
    var result = _stockPrices.GetStockPricesForAsync("MSFT").Result;
    textBox.Text = "Result is: " + result;
}

// StockPrices.dll
public Task<decimal> GetStockPricesForAsync(string symbol)
{
    // We know that the initialization step is very fast,
    // and completes synchronously in most cases,
    // let's wait for the result synchronously for "performance reasons".
    InitializeIfNeededAsync().Wait();
    return Task.FromResult((decimal)42);
}

// StockPrices.dll
private async Task InitializeIfNeededAsync() => await Task.Delay(1);
```

This code causes the deadlock as well. And now two "well-known" best practices for asynchronous programming in C# should make much more sense:

* Do not block an async code via `Task.Wait()` or `Task.Result` and
* Use `ConfigureAwait(false)` in a library code.

The first advice should be clear already and we should have another section to explain the other one.

## Configure "awaits"

There are two reasons why the last example caused the deadlock: blocking call to `Task.Wait()` in `GetStockPricesForAsync` and the synchronization context implicitly used in the continuation inside `InitializeIfNeededAsync`. Even though C# authors discouraged use of blocking calls of async methods it was clear that there are plenty of cases when this may happen. To work-around a deadlock issue the C# language authors came up with the solution: [`Task.ConfigureAwait(continueOnCapturedContext:false)`](http://referencesource.microsoft.com/#mscorlib/system/threading/Tasks/Task.cs,9ca6b2f012ce7587).

Besides a very strange name (that's absolutely obscure when you see a method call without named argument) it does its job: it forces the continuation to run without synchronization context.

```csharp
public Task<decimal> GetStockPricesForAsync(string symbol)
{
    InitializeIfNeededAsync().Wait();
    return Task.FromResult((decimal)42);
}

private async Task InitializeIfNeededAsync() => await Task.Delay(1).ConfigureAwait(false);
```

In this case the continuation of `Task.Delay(1)` task (empty statement in this case), is scheduled in a thread pool's thread instead of the UI thread, causing the deadlock to go away.

## Detaching the synchronization context

I know that the `ConfigureAwait` is the de-facto way of dealing with this problem but I see a huge issue with it. Here is a small example:

```csharp
public Task<decimal> GetStockPricesForAsync(string symbol)
{
    InitializeIfNeededAsync().Wait();
    return Task.FromResult((decimal)42);
}

private async Task InitializeIfNeededAsync()
{
    // Initialize the cache field first
    await _cache.InitializeAsync().ConfigureAwait(false);
    // Do some work
    await Task.Delay(1);
}
```

Can you see the problem here? We've used `ConfigureAwait(false)` so everything should be fine. But not necessarily.

`ConfigureAwait(false)` returns a custom awaiter called [`ConfiguredTaskAwaitable`](http://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/TaskAwaiter.cs,358) and we know that the awaiter is used only when the task is not finished synchronously. It means that if `_cache.InitializeAsync()` is completed synchronously we still could have a deadlock.

To solve the deadlock problem **every** awaited task should be "decorated" with a `ConfigureAwait(false)` call. Annoying and error-prone.

An alternative solution is to use **a custom awaiter in every public method** to detach the synchronization context from the asynchronous method:

```csharp
private void buttonOk_Click(object sender, EventArgs args)
{
    textBox.Text = "Running..";
    var result = _stockPrices.GetStockPricesForAsync("MSFT").Result;
    textBox.Text = "Result is: " + result;
}

// StockPrices.dll
public async Task<decimal> GetStockPricesForAsync(string symbol)
{
    // The rest of the method is guarantee won't have a current sync context.
    await Awaiters.DetachCurrentSyncContext();

    // We can wait synchronously here and we won't have a deadlock.
    InitializeIfNeededAsync().Wait();
    return 42;
}
```

`Awaiters.DetachCurrentSyncContext` returns the following custom awaiter:

```csharp
public struct DetachSynchronizationContextAwaiter : ICriticalNotifyCompletion
{
    /// <summary>
    /// Returns true if a current synchronization context is null.
    /// It means that the continuation is called only when a current context
    /// is presented.
    /// </summary>
    public bool IsCompleted => SynchronizationContext.Current == null;

    public void OnCompleted(Action continuation)
    {
        ThreadPool.QueueUserWorkItem(state => continuation());
    }

    public void UnsafeOnCompleted(Action continuation)
    {
        ThreadPool.UnsafeQueueUserWorkItem(state => continuation(), null);
    }

    public void GetResult() { }

    public DetachSynchronizationContextAwaiter GetAwaiter() => this;
}

public static class Awaiters
{
    public static DetachSynchronizationContextAwaiter DetachCurrentSyncContext()
    {
        return new DetachSynchronizationContextAwaiter();
    }
}
```

`DetachSynchronizationContextAwaiter` does the following: if the async method runs with non-null synchronization context, the awaiter detects this and schedules the continuation to the thread pool's thread. But if the async method runs without synchronization context `IsCompleted` property returns `true` and the continuation of the method runs synchronously.

It means that there is close-to-0 overhead if the async method runs from the thread pool's thread and you pay only once to move the execution from the "UI"-thread to a thread pool's thread.

Other benefits of this approach.

* **The approach is less error prone.** `ConfigureAwait(false)` only works when all the awaited tasks are decorated with it. If you forget just one, the deadlock could happen. In the custom awaiter case, you should remember one thing: all the public methods of your library should start with `Awaiters.DetachCurrentSyncContext()`. Still possible to mess up, but the likelihood is lower.
* **The resulting code is more declarative and cleaner.** In my opinion, a method with several `ConfigureAwait` calls is harder to read because of the noise, and the code is way less descriptive for a newcomer.

## Exception handling

What's the difference between this two cases:

```csharp
Task mayFail = Task.FromException(new ArgumentNullException());

// Case 1
try { await mayFail; }
catch (ArgumentException e)
{
    // Handle the error
}

// Case 2
try { mayFail.Wait(); }
catch (ArgumentException e)
{
    // Handle the error
}
```

The first case does exactly what you'd expect -- handles the error, but the second one -- doesn't. TPL was designed for asynchronous and parallel programming and `Task`/`Task<T>` can represent a result of multiple operations. That's why `Task.Result` and `Task.Wait()` always throws an `AggregateException` that potentially contains more than one error.

But our main scenario changes everything: the user should be able to add async/await without changing the error handling logic. This means that `await` statement should be different from `Task.Result`/`Task.Wait()`: it should unwrap one exception from the `AggregateException` instance. Today it picks the first one.

Everything is fine if all the task-based methods are asynchronous and the tasks are not backed by a parallel computation. But this is not true all the time:

```csharp
try
{
    Task<int> task1 = Task.FromException<int>(new ArgumentNullException());

    Task<int> task2 = Task.FromException<int>(new InvalidOperationException());

    // await will rethrow the first exception
    await Task.WhenAll(task1, task2);
}
catch(Exception e)
{
    // ArgumentNullException. The second error is lost!
    Console.WriteLine(e.GetType());
}
```

`Task.WhenAll` returns a task that fails with two errors, but `await` statement extracts and propagates just the first one.

There are two ways to solve this issue:

1. Observe individual tasks manually if you have an access to them or
2. Force TPL to wrap the exception into another `AggregateException`.

```csharp
try
{
    Task<int> task1 = Task.FromException<int>(new ArgumentNullException());

    Task<int> task2 = Task.FromException<int>(new InvalidOperationException());

    // t.Result forces TPL to wrap the AggregateException into another AggregateException
    await Task.WhenAll(task1, task2).ContinueWith(t => t.Result);
}
catch (Exception e)
{
    // AggregateException
    Console.WriteLine(e.GetType());
}
```

## Async void methods

The task-based method returns a promise -- a token that can be used for processing the results in the future. If the task is lost the promise becomes unobservable by a user's code. The asynchronous operation that returns `void` makes the error case impossible to handle from the user's code. This makes them kind-of useless and, as we'll see in a moment -- dangerous. But our main scenario makes them necessary: 

```csharp
private async void buttonOk_ClickAsync(object sender, EventArgs args)
{
    textBox.Text = "Running..";
    var result = await _stockPrices.GetStockPricesForAsync("MSFT");
    textBox.Text = "Result is: " + result;
}
```

But will happen if `GetStockPricesForAsync` fail with an error? The unhandled exception of the async void method is marshaled to a current synchronization context triggering the same behavior as it was for the synchronous code (see [ThrowAsync method](http://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/AsyncMethodBuilder.cs,1018) at AsyncMethodBuilder.cs for more details). In Windows Forms an unhandled exception in the event handler triggers [`Application.ThreadException`](https://msdn.microsoft.com/en-us/library/system.windows.forms.application.threadexception(v=vs.110).aspx) event, for WPF - [`Application.DispatcherUnhandledException`](https://msdn.microsoft.com/en-us/library/system.windows.application.dispatcherunhandledexception.aspx?f=255&MSPPError=-2147217396) event etc.

But what if the async void method doesn't have a captured synchronization context? In this case, an unhandled exception will crash the app without an ability to recover from it. It won't trigger recoverable [`TaskScheduler.UnobservedTaskException`] event, it will trigger unrecoverable [`AppDomain.UnhandledException`](https://msdn.microsoft.com/en-us/library/system.appdomain.unhandledexception(v=vs.110).aspx) event and will close the app. This is intentional and this is what it should be.

Now you should understand another famous best practice: **Use async-void methods only for UI even handlers**.

Unfortunately, it is relatively easy to accidentally introduce an async void method without realizing it.

```csharp
public static Task<T> ActionWithRetry<T>(Func<Task<T>> provider, Action<Exception> onError)
{
    // Calls 'provider' N times and calls 'onError' in case of an error.
}

public async Task<string> AccidentalAsyncVoid(string fileName)
{
    return await ActionWithRetry(
        provider:
        () =>
        {
            return File.ReadAllTextAsync(fileName);
        },
        // Can you spot the issue?
        onError:
        async e =>
        {
            await File.WriteAllTextAsync(errorLogFile, e.ToString());
        });
}
```

It is very hard to tell just by looking at the lambda expression whether the function is task-based or async void and the error can easily sneak-in into your codebase even with a thorough code review.

## Conclusion
There is one user scenario -- simple migration from synchronous to asynchronous code for existing UI application -- that affected asynchronous programming in C# in so many ways:

* The continuations of async methods are scheduled into a captured synchronization context that can cause deadlocks.
* To avoid deadlocks all the async library code should be littered with `ConfigureAwait(false)` calls.
* `await task;` throws the first error making exception handling for parallel programming more complicated.
* Async void methods were introduced for handling UI events but they can be used accidentally causing the application to crash in case of an unhandled exception.

There is no such thing as a free lunch. Ease of use in one case can complicate other use cases drastically. Knowing the history of asynchronous programming in C# makes the weird behaviors less weird and should reduce the likelihood of an error in your asynchronous code.