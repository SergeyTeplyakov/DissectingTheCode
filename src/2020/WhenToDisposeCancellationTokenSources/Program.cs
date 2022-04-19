using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using WhenToDisposeCancellationTokenSources.Examples;
#nullable enable
namespace WhenToDisposeCancellationTokenSources
{
    public abstract class MachineIdSet
    {
        protected MachineIdSet()
        {
            Console.WriteLine("MachineIdSet.ctor");
        }
        
        //static MachineIdSet()
        //{
        //    //Console.WriteLine($"MachineIdSet.cctor. {Empty1 == null}, {ArrayMachineIdSet.Empty2 == null}");
        //    Console.WriteLine($"MachineIdSet.cctor.");
        //}

        public static readonly MachineIdSet Empty1 = ArrayMachineIdSet.Empty2;//TraceAndGet(ArrayMachineIdSet.Empty2, caller: "MachineIdSet.cctor");

        protected static T TraceAndGet<T>(T value, [CallerMemberName] string caller = null)
        {
            Console.WriteLine($"Calling from {caller}");
            return value;
        }
    }



    public class ArrayMachineIdSet : MachineIdSet
    {
        public ArrayMachineIdSet()
        {
            Console.WriteLine("ArrayMachineIdSet.ctor");
        }
        
        //static ArrayMachineIdSet()
        //{
        //    Console.WriteLine("ArrayMachineIdSet.cctor");
        //}

        public static MachineIdSet Empty2 { get; }= new ArrayMachineIdSet();// TraceAndGet(new ArrayMachineIdSet(), caller: "ArrayMachineIdSet.cctor");
    }
    
    public class BitMachineIdSet : MachineIdSet
    {
        public BitMachineIdSet()
        {
            Console.WriteLine("BitMachineIdSet.ctor");
        }

        public static void Foo()
        {
            Console.WriteLine($"Foo. {BitMachineIdSet.Empty3 == null}");
        }
        //static BitMachineIdSet()
        //{
        //    Console.WriteLine("BitMachineIdSet.cctor");
        //}

        public static readonly MachineIdSet Empty3 = new BitMachineIdSet(); //TraceAndGet(new BitMachineIdSet(), caller: "BitMachineIdSet.cctor");
    }

    public class Entry
    {
        private static readonly byte[] UnknownSizeBytes = new byte[42];
        public Entry(MachineIdSet set)
        {
            Console.WriteLine($"Entry.ctor. Set == null: {set == null}");
        }

        public static Entry Missing { get; } = new Entry(MachineIdSet.Empty1);
        public static int Foo()
        {
            var result = UnknownSizeBytes.Length;
            
            BitMachineIdSet.Foo();
            return result;
        }
    }
    public class Workspace
    {

    }

    class Test
    {

        private Workspace Workspace { get; set; } = new Workspace();
        //private Workspace Workspace = new Workspace();

        public async Task CheckMemoryAsync()
        {
            ReferenceWorkspace(Workspace);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            //await Task.Delay(1);
            // Workspace has been converted and is not needed anymore
            CleanWorkspaceMemory();

            await Task.Delay(1);
        }

        private void CleanWorkspaceMemory()
        {
            WeakReference wr = GetWorkspaceAnchorAndReleaseWorkspace();
            GC.Collect();
            Console.WriteLine(wr.IsAlive);
        }

        private WeakReference GetWorkspaceAnchorAndReleaseWorkspace()
        {
            var wr = new WeakReference(Workspace);
            Workspace = null;
            return wr;
        }

        private void ReferenceWorkspace(Workspace workspace)
        {
            Console.WriteLine(nameof(ReferenceWorkspace));
        }
    }

    static class Program
    {
        public static async Task<TResult> DeduplicatedOperationAsync<TResult>(this SemaphoreSlim gate, Func<TimeSpan, int, Task<TResult>> operation, Func<TimeSpan, int, Task<TResult>> duplicated = null, CancellationToken token = default)
        {
            var sw = Stopwatch.StartNew();
            var taken = await gate.WaitAsync(millisecondsTimeout: 0, token);
            if (!taken)
            {
                var currentCount = gate.CurrentCount;
                if (duplicated != null)
                {
                    return await duplicated.Invoke(sw.Elapsed, currentCount);
                }
                else
                {
                    throw new OperationCanceledException(message: "Operation could not proceed due to deduplication gate");
                }
            }

            try
            {
                var currentCount = gate.CurrentCount;
                return await operation(sw.Elapsed, currentCount);
            }
            finally
            {
                gate.Release();
            }
        }

        public const string Default = "";

        public static void FooBar(string? sectionName = Default)
        {
            baz(sectionName);

            static int baz(string s) => s.Length;
        }

        

        static async Task Main(string[] args)
        {

            using var service = new Service();
            var token = CancellationToken.None;

            //for (int i = 0; i < IterationsCount; i++)
            while(true)
            {
                await service.LongRunningOperationAsync(token, disposeRegistration: false);
                await Task.Delay(100);
            }

            //await new Test().CheckMemoryAsync();
            //var b = new AsyncBenchmark();
            //Console.WriteLine(b.SimpleAverage());
            //Console.WriteLine(b.ForkJoin());
            //BenchmarkRunner.Run<AsyncBenchmark>();


            //SemaphoreSlim _gcGate = new SemaphoreSlim(1, 1);
            //var t1 = _gcGate.DeduplicatedOperationAsync(async (duration, number) =>
            //{
            //    Console.WriteLine("Starting operation ");
            //    await Task.Delay(1000);
            //    Console.WriteLine("Done");
            //    return 42;
            //});

            //var t2 = _gcGate.DeduplicatedOperationAsync(async (duration, number) =>
            //{
            //    Console.WriteLine("Starting operation ");
            //    await Task.Delay(1000);
            //    Console.WriteLine("Done");
            //    return 42;
            //});

            //await Task.WhenAll(t1, t2);
            //Entry.Foo();
            //var e = MachineIdSet.Empty1;
            //BenchmarkRunner.Run<BenchmarkPerformOperation>();
            ////await Examples.Examples.DemoTimerFlooding();
            //await OperationsHelper.PerformOperationWithPooledTimerAsync(
            //    async () => await Task.Delay(TimeSpan.FromSeconds(5)),
            //    interval: TimeSpan.FromSeconds(1),
            //    operationName: "Op1");
            //Console.WriteLine("Done 1");
            //await OperationsHelper.PerformOperationWithPooledTimerAsync(
            //    async () => await Task.Delay(TimeSpan.FromSeconds(5)),
            //    interval: TimeSpan.FromSeconds(1),
            //    operationName: "Op2");
            //Console.WriteLine("Done 2");

            //await OperationsHelper.PerformOperationWithTimerAsync(
            //    async () => await Task.Delay(TimeSpan.FromSeconds(5)),
            //    interval: TimeSpan.FromSeconds(1));
            //Console.WriteLine("Done 3");
        }
    }
}
