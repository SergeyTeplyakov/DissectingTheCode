using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NonAsynchronousAsyncMethods
{
    public static class TaskExtensions
    {
        public static void FireAndForget(this Task task, [CallerMemberName]string operation = null)
        {
            var sw = Stopwatch.StartNew();

            task.ContinueWith(t =>
            {
                Console.WriteLine($"Async operation '{operation}' is finished. Status={t.Status} in {sw.ElapsedMilliseconds}ms");
            });
        }
    }

    public class Cache
    {
        public async Task StartupAsync()
        {
            Console.WriteLine($"StartupAsync start. ThreadId={Thread.CurrentThread.ManagedThreadId}");
            var sw = Stopwatch.StartNew();

            // Should take 500ms
            await ReloadSessionsAsync();

            // Should be fast: no await!
            SelfCheckCacheAsync().FireAndForget();

            Console.WriteLine($"StartupAsync stop in {sw.ElapsedMilliseconds}. ThreadId={Thread.CurrentThread.ManagedThreadId}");
        }

        public async Task SelfCheckCacheAsync()
        {
            Console.WriteLine($"SelfCheckCacheAsync start. ThreadId={Thread.CurrentThread.ManagedThreadId}");
            var sw = Stopwatch.StartNew();
            await Task.Yield();
            // Slow synchronous operation, like directory enumeration.
            // May take relatively long time on HDD disks.
            Thread.Sleep(2000);

            // Some other pure async IO
            await Task.Delay(2000);
            Console.WriteLine($"SelfCheckCacheAsync stop in {sw.ElapsedMilliseconds}ms. ThreadId={Thread.CurrentThread.ManagedThreadId}");
        }

        private async Task ReloadSessionsAsync()
        {
            Console.WriteLine($"SelfCheckCacheAsync start. ThreadId={Thread.CurrentThread.ManagedThreadId}");
            var sw = Stopwatch.StartNew();

            await Task.Delay(500);

            Console.WriteLine($"SelfCheckCacheAsync stop in {sw.ElapsedMilliseconds}ms. ThreadId={Thread.CurrentThread.ManagedThreadId}");
        }
    }
}
