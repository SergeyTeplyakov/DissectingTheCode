using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
#nullable enable
namespace WhenToDisposeCancellationTokenSources.Examples
{

    public static class Examples
    {
        public static async Task PerformOperationWithTimeout(TimeSpan timeout, bool disposeCts)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);
            
            using (disposeCts ? cts : null)
            {
                await PerformOperation(cts.Token);
            }

            static Task PerformOperation(CancellationToken token)
            {
                // A long running operation that respects checks the token
                // or passes it around to another methods.
                //await Task.Delay(TimeSpan.FromMilliseconds(1), token);
                //await Task.Yield();
                return Task.FromResult(0);
            }
        }

        public static async Task DemoTimerFlooding()
        {
            await RunTimerFlooding(disposeCts: true, iterations: 100_000_000);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            await RunTimerFlooding(disposeCts: false, iterations: 100_000_000);
        }

        private static async Task RunTimerFlooding(bool disposeCts, int iterations)
        {
            Console.WriteLine($"Running timer flood test. DisposeCts={disposeCts}");
            int timerQueueSize = 0;
            var cts = new CancellationTokenSource();
            var task = Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss:ffff}: QueueSize={timerQueueSize}");
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            });

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                //await PerformOperationWithTimeout(TimeSpan.FromMinutes(1));
                await PerformOperationWithTimeout(TimeSpan.FromSeconds(1), disposeCts);
                Interlocked.Increment(ref timerQueueSize);
            }

            Console.WriteLine($"Done flooding the timer queue in {sw.Elapsed}... DisposeCts={disposeCts}");
            cts.Cancel();
            await task;
        }

        

        public static Task CancelAfterAsync(this Task task, TimeSpan timeout)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);

            var tcs = new TaskCompletionSource<object?>();
            task.ContinueWith(t =>
            {
                if (t.IsCompleted) tcs.TrySetResult(null);
                else if (t.IsCanceled) tcs.TrySetCanceled();
                else if (t.IsFaulted) tcs.TrySetException(t.Exception!);
            }, cts.Token);

            cts.Token.Register(() => tcs.TrySetException(new TimeoutException()));
            
            return tcs.Task;
        }
    }
}