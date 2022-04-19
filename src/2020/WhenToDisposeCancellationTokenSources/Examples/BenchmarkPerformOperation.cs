using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using BenchmarkDotNet.Attributes;
using BuildXL.Utilities;
using Iced.Intel;
using Timer = System.Threading.Timer;

namespace WhenToDisposeCancellationTokenSources.Examples
{
    public static class OperationsHelper
    {
        public static async Task<TimeSpan> PerformOperationAsync(Func<Task> func)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                await func();
                return sw.Elapsed;
            }
            catch(Exception)
            {
                throw;
            }
        }
        
        private static ObjectPool<System.Timers.Timer> _timerObjectPool = new ObjectPool<System.Timers.Timer>(() => new System.Timers.Timer(), timer => timer.Stop());

        public static async Task<TimeSpan> PerformOperationWithTimerAsync(Func<Task> func, TimeSpan interval)
        {
            using var timer = new Timer(_ => Console.WriteLine("In progress!"));
            timer.Change(interval, interval);

            var sw = Stopwatch.StartNew();
            try
            {
                await func();
                return sw.Elapsed;
            }
            catch(Exception)
            {
                throw;
            }
        }

        private readonly struct TimerHolder : IDisposable
        {
            private readonly PooledObjectWrapper<System.Timers.Timer> _timer;
            private readonly ElapsedEventHandler _timerCallback;

            public TimerHolder(TimeSpan interval, Action callback)
            {
                _timer = _timerObjectPool.GetInstance();
                _timer.Instance.Interval = interval.TotalMilliseconds;
                _timer.Instance.Enabled = true;
                _timerCallback = (sender, args) => callback();
                _timer.Instance.Elapsed += _timerCallback;
            }

            public void Dispose()
            {
                _timer.Instance.Elapsed -= _timerCallback;
                _timer.Instance.Enabled = false;
                _timer.Dispose();
            }
        }

        private static TimerHolder GetTimer(Action callback, TimeSpan interval)
        {
            return new TimerHolder(interval, callback);
        }
        
        public static async Task<TimeSpan> PerformOperationWithPooledTimerAsync(Func<Task> func, TimeSpan interval,
            string operationName)
        {
            using var timer = GetTimer(() => Console.WriteLine($"In progress '{operationName}'"), interval);

            var sw = Stopwatch.StartNew();
            try
            {
                await func();
                return sw.Elapsed;
            }
            catch(Exception)
            {
                throw;
            }
        }
    }

    [MemoryDiagnoser]
    public class BenchmarkPerformOperation
    {
        private Task[] _pendingLongRunningTasks;

        [BenchmarkDotNet.Attributes.Params(100, 1000, 10_000)]
        public int NumberOfPendingLongRunningOperations { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _pendingLongRunningTasks = Enumerable.Range(1, NumberOfPendingLongRunningOperations)
                .Select(n =>
                OperationsHelper.PerformOperationWithTimerAsync(
                    async () => await Task.Delay(TimeSpan.FromHours(1)),
                    interval: TimeSpan.FromHours(1))).ToArray();
        }

        [Benchmark(Baseline = true)]
        public async Task Baseline()
        {
            await OperationsHelper.PerformOperationAsync(async () =>
            {
                await Task.Yield();
            });
        }


        [Benchmark]
        public async Task WithTimer()
        {
            await OperationsHelper.PerformOperationWithTimerAsync(async () =>
            {
                await Task.Yield();
            },
                TimeSpan.FromSeconds(10));
        }
        
        [Benchmark]
        public async Task WithPooledTimer()
        {
            await OperationsHelper.PerformOperationWithPooledTimerAsync(async () =>
            {
                await Task.Yield();
            },
                TimeSpan.FromSeconds(10),
                operationName: "WithPooledTimer");
        }
    }
}