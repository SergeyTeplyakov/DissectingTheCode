using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CommandLine;

namespace WhenToDisposeCancellationTokenSources.Examples
{
    public class LockedAverage
    {
        private int _value;
        private int _count = 1;

        public void Report(int value)
        {
            lock (this)
            {
                _value += value;
                _count++;
            }
        }

        public double Average
        {
            get
            {
                lock (this)
                {
                    return (double)_value / _count;
                }
            }
        }
    }

    public class InterlockedAverage
    {
        private int _value;
        private int _count = 1;

        public void Report(int value)
        {
            {
                Interlocked.Add(ref _value, value);
                Interlocked.Increment(ref _count);
            }
        }

        public double Average
        {
            get
            {
                {
                    var count = Volatile.Read(ref _count);
                    return (double)_value / count;
                }
            }
        }
    }
    
    public class PackedInterlockedAverage
    {
        private long _store;

        public void Report(int value)
        {
            {

                Interlocked.Add(ref _store, ((long)value << 8) | 1);
            }
        }

        public double Average
        {
            get
            {
                {
                    var store = Volatile.Read(ref _store);
                    var sum = (int) (store >> 32);
                    int count = (int) store;
                    return (double) sum/ count;
                }
            }
        }
    }

    public class SimpleAverage
    {
        private int _value;
        private int _count = 1;

        public void Report(int value)
        {
            //lock (this)
            {
                _value += value;
                _count++;
            }
        }

        public double Average
        {
            get
            {
                //lock (this)
                {
                    return (double)_value / _count;
                }
            }
        }
    }

    public class AsyncBenchmark
    {
        private const int WorkerCount = 8;
        private const int Size = 1_000_000;

        [Benchmark]
        public double CountLockedAverage()
        {
            var average = new LockedAverage();

            var tasks = Enumerable.Range(0, WorkerCount).Select(
                n => Task.Run(() =>
                {
                    for (int i = 0; i < Size; i++)
                    {
                        average.Report(n);
                    }
                })).ToList();

            Task.WhenAll(tasks).GetAwaiter().GetResult();
            return average.Average;
        }
        
        [Benchmark]
        public double ForkJoin()
        {
            var tasks = Enumerable.Range(0, WorkerCount).Select(
                n => Task.Run(() =>
                {
                    var average = new SimpleAverage();
                    for (int i = 0; i < Size; i++)
                    {
                        average.Report(n);
                    }

                    return average.Average;
                })).ToList();

            var averages = Task.WhenAll(tasks).GetAwaiter().GetResult();
            return averages.Average();
        }
        
        [Benchmark]
        public double ForkJoin4()
        {
            var tasks = Enumerable.Range(0, WorkerCount/2).Select(
                n => Task.Run(() =>
                {
                    var average = new SimpleAverage();
                    for (int i = 0; i < Size; i++)
                    {
                        average.Report(n);
                    }

                    return average.Average;
                })).ToList();

            var averages = Task.WhenAll(tasks).GetAwaiter().GetResult();
            return averages.Average();
        }

        //[Benchmark]
        //public void CountLockedAverageWithThreads()
        //{
        //    var average = new LockedAverage();

        //    var threads = new Thread[WorkerCount];
        //    for (int n = 0; n < WorkerCount; n++)
        //    {
        //        threads[n] = new Thread(value =>
        //        {
        //            int v = (int)value;
        //            for (int i = 0; i < Size; i++)
        //            {
        //                average.Report(v);
        //            }
        //        }, n);
        //    }

        //    foreach (var t in threads) { t.Start(); }
        //    foreach (var t in threads) { t.Join(); }
        //}

        //[Benchmark]
        //public void InterlockedAverage()
        //{
        //    var average = new InterlockedAverage();

        //    var tasks = Enumerable.Range(1, WorkerCount).Select(
        //        n => Task.Run(() =>
        //        {
        //            for (int i = 0; i < Size; i++)
        //            {
        //                average.Report(n);
        //            }
        //        })).ToList();

        //    Task.WhenAll(tasks).GetAwaiter().GetResult();
        //}
        //

        [Benchmark]
        public void InterlockedAverageWithLongRunningTasks()
        {
            var average = new InterlockedAverage();

            var tasks = Enumerable.Range(1, WorkerCount).Select(
                n => Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < Size; i++)
                    {
                        average.Report(n);
                    }
                }, TaskCreationOptions.LongRunning)).ToList();

            Task.WhenAll(tasks).GetAwaiter().GetResult();
        }

        //[Benchmark]
        //public void PackedInterlockedAverage()
        //{
        //    var average = new PackedInterlockedAverage();

        //    var tasks = Enumerable.Range(1, WorkerCount).Select(
        //        n => Task.Run(() =>
        //        {
        //            for (int i = 0; i < Size; i++)
        //            {
        //                average.Report(n);
        //            }
        //        })).ToList();

        //    Task.WhenAll(tasks).GetAwaiter().GetResult();
        //}

        [Benchmark]
        public double SimpleAverage()
        {
            var average = new SimpleAverage();

            for (int outer = 0; outer < WorkerCount; outer++)
            {
                for (int i = 0; i < Size; i++)
                {
                    average.Report(outer);
                }
            }

            return average.Average;
        }
    }
}