# When to dispose a CancellationTokenSource instance?

In general, all disposable objects must be disposed once you're done with them. If a non-disposed instance wraps an unmanaged resource (*), than the resource will be freed by the finalizer and the worst thing that happen with an application is some kind of (hopefully) recoverable error due to a non-deterministic resource clean-up. On the other hand, the consequences could be way more severe if the `Dispose` method does something other than just cleaning up a managed resource: the `Dispose` method may release a lock, stop the timer, cancel a long running operation or unsubscribe from a long lived objects.

(*) An unmanaged resource is opaque to the CLR, like `IntPtr` that points to an umanaged memory, a custom handle with a custom clean up logic etc. Once an unmanaged resource is wrapped in class that implements `IDisposable` interface with an optional finalizer, it becamea managed resource.

There are definitely some exceptions to this rule. For instance, disposing a `Task` is a bad idea (see [Do I need to dispose a Task?](https://devblogs.microsoft.com/pfxteam/do-i-need-to-dispose-of-tasks/)). But what about `CancellationTokenSource` instances: should you dispose them or not?

Just a quick recap: `CancellationTokenSource` is a building block for implementing graceful cancellation in .NET. The cancellation API has two types - `CancellationToken` abd `CancellationTokenSource`. `CancellationToken` is used to listen for the cancellation request inside a cancellable operation, and `CancellationTokenSource` is used to trigger the cancellation based on some needs, like clicking a Cancel button.

There are a few common use cases when `CancellationTokenSource` is used:

1. Cancelling after a given timeout.
2. Combining two or more existing `CancellationToken` instances and cancelling the operation when one of the tokens is cancelled.
3. Gracefully cancelling an operation based on some event (like clickly a Cancel button) by explicitely calling `CancellationTokenSource.Cancel` method.

## Cancelling an operation after a given timeout
It is quite common to limit the operation duration and cancel it when the allotted time has passed. You may do something like this:

```csharp
public static async Task PerformOperationWithTimeout(TimeSpan timeout)
{
    var cts = new CancellationTokenSource();
    cts.CancelAfter(timeout);

    await PerformOperation(cts.Token);

    static async Task PerformOperation(CancellationToken token)
    {
        // A potentially long running operation that checks
        // the token to stop the execution once requested.
        await Task.Delay(TimeSpan.FromMinutes(1), token);
    }
}
```

The code looks quite innocent, but it has a serious issue.

In order to trigger the cancellation after a given time, `CancellationTokenSource` creates a `Timer` instance and disposes it in the `Dispose` method. If the `PerformOperation` usually completes before a given timeout the two things will happen: 1) the `CancellationTokenSource` instance will live longer (i.e. until the timer fires, and won't be eligible for GC once the `PerformOperationWithTimeout` is done and 2) the timer will fire anyways adding extra burden to the timer queue.

**How serious the issue for a regular application?**
For a regular line of business application the issue won't be very severe, just because .NET is a very efficient platform. But for a backend application that calls such operation thousands of times a second this can flood a timer queue and cause severe performance problems across the system. For instance, `PerformOperation` can have a synchronous path and complete very quickly, and/or the timeout can be quite long (like tens of minutes) that can increase the memory footprint of the application.

I created a sample application (TODO: point to the benchmark code) and running this operation even hundreds of thousands of times didn't affect performance too much.

## Combinging multiple `CancellationToken` instances together
Another common use case for `CancellationTokenSource` is to combine two cancellation tokens toger to trigger the cancellation when one of the token is canceled.

Here is a quite common pattern: lets say you have a long lived disposable object that can perform a long-running asynchronous operation. The resource ownership in this case can be quite tricky and in order to avoid failing the operations with `ObjectDisposedException` you may decide to cancel the operation when the `Dispose` method is called.

```csharp
```

## Graceful cancellation


some Azure Storage calls may stuck forever and you want to abort them after a given timeout.

In general, as a user, you don't really know what 