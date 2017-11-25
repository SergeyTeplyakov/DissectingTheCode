using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Ex;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using ValueTasks.CustomAwaiter;
using ValueTaskInt = System.Threading.Tasks.Ex.ValueTask<int>;

namespace ValueTasks
{
    /*
     * Results may be affected because of inlining. 
     * x86
                   Method |       Mean |     Error |    StdDev |     Median |  Gen 0 | Allocated |
------------------------- |-----------:|----------:|----------:|-----------:|-------:|----------:|
               TaskBased1 |   5.744 ns | 0.1603 ns | 0.2542 ns |   5.720 ns | 0.0140 |      44 B |
               ValueTask1 |   1.629 ns | 0.0526 ns | 0.0466 ns |   1.619 ns |      - |       0 B |
          TaskBased1Async | 123.326 ns | 2.4549 ns | 3.2772 ns | 123.168 ns | 0.0138 |      44 B |
          ValueTask1Async | 129.562 ns | 2.6006 ns | 4.1248 ns | 129.304 ns |      - |       0 B |
       TaskBasedWithAwait | 144.991 ns | 2.9130 ns | 4.5353 ns | 144.724 ns | 0.0279 |      88 B |
 ValueTaskWithManualAwait |   1.797 ns | 0.0992 ns | 0.1143 ns |   1.778 ns |      - |       0 B |
       ValueTaskWithAwait | 137.267 ns | 1.2828 ns | 1.1999 ns | 136.987 ns |      - |       0 B |
     TaskBasedWith2Awaits | 152.916 ns | 3.3300 ns | 9.1718 ns | 150.814 ns | 0.0417 |     132 B |
     ValueTaskWith2Awaits | 147.959 ns | 3.0129 ns | 6.3552 ns | 144.386 ns |      - |       0 B |
     * */


    //[HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.BranchInstructions)]
    //[DisassemblyDiagnoser(printAsm: true, printSource: true)]
    [MemoryDiagnoser]
    public class Benchmarks
    {
        private static Lazy<int> s_lazy = new Lazy<int>(() => 42);
        private ValueTaskInt v_t;

        public Benchmarks()
        {
            v_t = new ValueTask<int>(42);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Benchmark]
        public int RegularMethod()
        {
            return s_lazy.Value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Benchmark]
        public ValueTaskInt NonAsyncValueTask_Copy()
        {
            return new ValueTaskInt(s_lazy.Value);
        }

        //[Benchmark]
        //public async ValueTaskInt AwaitLazyValueTask_Copy()
        //{
        //    return await s_lazy;
        //}

        //[Benchmark]
        //public ValueTaskInt AwaitLazyIfNotCompleted_Copy()
        //{
        //    var awaiter = s_lazy.GetAwaiter();
        //    if (awaiter.IsCompleted)
        //    {
        //        return new ValueTaskInt(awaiter.GetResult());
        //    }

        //    return Await(awaiter);

        //    async ValueTaskInt Await(LazyAwaiter<int> lazyAwaiter)
        //    {
        //        return await lazyAwaiter;
        //    }
        //}

        [Benchmark]
        public async Task<int> RegularTask()
        {
            return await s_lazy;
        }

        ////[Benchmark]
        //public Task<int> TaskBased1() => Task.FromResult(42);

        ////[Benchmark]
        //public ValueTaskInt ValueTask1() => new ValueTaskInt(42);

        ////[Benchmark]
        //public async Task<int> TaskBased1Async() => 42;

        ////[Benchmark]
        //public async ValueTaskInt ValueTask1Async() => 42;

        //[Benchmark]
        //public async Task<int> TaskBasedWithAwait()
        //{
        //    await TaskBased1();
        //    return 42;
        //}

        //[MethodImpl(MethodImplOptions.NoInlining)]
        //[Benchmark]
        //public ValueTaskInt ValueTaskWithManualAwait()
        //{
        //    var task = ValueTask1();
        //    if (task.IsCompleted)
        //    {
        //        return new ValueTaskInt(42);
        //    }

        //    return ValueTaskWithManualAwait(task);
        //    async ValueTaskInt ValueTaskWithManualAwait(ValueTaskInt tsk)
        //    {
        //        await tsk;
        //        return 42;
        //    }
        //}

        //[Benchmark]
        //public async ValueTaskInt ValueTaskWithAwait()
        //{
        //    await ValueTask1();
        //    return 42;
        //}

        //[Benchmark]
        //public async Task<int> TaskBasedWith2Awaits()
        //{
        //    await TaskBased1();
        //    await TaskBased1();
        //    return 42;
        //}

        //[Benchmark]
        //public async ValueTaskInt ValueTaskWith2Awaits()
        //{
        //    await ValueTask1();
        //    await ValueTask1();
        //    return 42;
        //}
    }
}