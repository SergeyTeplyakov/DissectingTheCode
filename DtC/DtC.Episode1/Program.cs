using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace DtC.Episode1
{
    public class RequestProcessor
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ProcessRequest(int data)
        {
            // Not using any instance state.
            var wr = new WeakReference(this);
            GC.Collect();
            Console.WriteLine("Is this alive? " + wr.IsAlive);
        }
    }

    [DisassemblyDiagnoser]
    [ShortRunJob]
    public class Benchamrk
    {
        [Benchmark]
        public void Run()
        {
            var processor = new RequestProcessor();
            processor.ProcessRequest(42);
            AnotherMethod();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void AnotherMethod()
        {

        }
    }

    internal class GcIsWeird
    {
        ~GcIsWeird()
        {
            Console.WriteLine("Finalizing instance.");
        }

        public int data = 42;

        public void DoSomething()
        {
            Console.WriteLine("Doing something. The answer is ... " + data);
            CheckReachability(this);
            Console.WriteLine("Finished doing something.");
        }

        public static void CheckReachability(object d)
        {
            var weakRef = new WeakReference(d);
            Console.WriteLine("Calling GC.Collect...");

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            string message = weakRef.IsAlive ? "alive" : "dead";
            Console.WriteLine("Object is " + message);
        }
    }

    internal class Program
    {

        static void Run(int iterationCount)
        {
            var processor = new RequestProcessor();
            var wr = new WeakReference(processor);
            GC.Collect();

            Console.WriteLine($"Before processor.ProessReqeust. wr.IsAlive: " + wr.IsAlive);
            processor.ProcessRequest(iterationCount);
        }

        static void Run2(int iterationCount)
        {
            var processor = new RequestProcessor();
            //var wr2 = new WeakReference(processor);
            processor.ProcessRequest(iterationCount);
            //GC.KeepAlive(processor);

            GC.Collect();
            Console.WriteLine("GC.Collect is done");
            //Console.WriteLine($"Is processor alive? {wr2.IsAlive}. Iteration: {iterationCount}.");

            //System.Diagnostics.Debugger.Launch();
            //if (iterationCount > 100)
            //{
            //    System.Diagnostics.Debugger.Launch();
            //}
        }

        static async Task Async()
        {
            object? o = "hello!";
            object? o2 = "world";
            // IDE0059: Unnecessary assignment of a variable to 'o'
            // This is correct!
            o = null;
            await Task.Yield();

            // IDE0059: Unnecessary assignment of a variable to 'o2'
            // This assignment is not corret, since we're setting a state
            // machine state to null!
            o2 = null;
            // a very long running method that potentially never finishes!
        }

        static async Task RunAsync(int data)
        {
            var processor = new RequestProcessor();
            var wr2 = new WeakReference(processor);
            processor.ProcessRequest(data);
            //await Task.Yield();

            GC.KeepAlive(processor);
            processor = null;
            GC.Collect();
            Console.WriteLine(wr2.IsAlive);
            //Console.WriteLine($"Is processor alive? {wr2.IsAlive}. Iteration: {data}.");

            await Task.Yield();

        }

        static void Main(string[] args)
        {
            //RunAsync(42).GetAwaiter().GetResult();

            //GcIsWeird.CheckReachability(42);
            ////BenchmarkRunner.Run<Benchamrk>();
            Run(42);
            //for (int i = 0; i < 5000; i++)
            //{
            //    Run(i);
            //}
        }
    }
}
