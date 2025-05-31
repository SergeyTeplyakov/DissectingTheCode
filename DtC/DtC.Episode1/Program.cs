
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DtC.Episode1
{
    //public class RequestProcessor
    //{
    //    [MethodImpl(MethodImplOptions.NoInlining)]
    //    public void ProcessRequest(int data)
    //    {
    //        // Not using any instance state.
    //        var wr = new WeakReference(this);
    //        GC.Collect();
    //        Console.WriteLine("Is this alive? " + wr.IsAlive);
    //    }
    //}
    
    public class RequestProcessor
    {
        //private int processedRequests;

        //[MethodImpl(MethodImplOptions.NoInlining| MethodImplOptions.AggressiveOptimization)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessRequest(int data)
        {
            //processedRequests++;
            //var wr = new WeakReference(this);
            //GC.Collect();
            //Console.WriteLine($"Instance is {(wr.IsAlive ? "Alive" : "Dead")} " +
            //                  $"before finishing ProcessRequest");
        }
    }

    internal class Program
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void ProcessRequest(int data)
        {
            var processor = new RequestProcessor();
            var wr = new WeakReference(processor);
            GC.Collect();
            Debug.WriteLine($"Processor is {(wr.IsAlive ? "Alive" : "Dead")}");
            
            processor.ProcessRequest(data);
            
            // Setting processor to null to force the GC!
            //processor = null;

            
            //Console.WriteLine($"Processor is {(wr.IsAlive ? "Alive" : "Dead")}");

            // Simulate more work
            //Thread.Sleep(1000);
        }
        public static async Task ProcessRequestAsync(int data)
        {
            var processor = new RequestProcessor();
            await Task.Yield();
            processor.ProcessRequest(42);
            
            var wr = new WeakReference(processor);
            processor = null;

            GC.Collect();

            Console.WriteLine($"Processor is {(wr.IsAlive ? "Alive" : "Dead")}");

            // A very long operations, or a loop that takes forever.
            await Task.Yield();
        }

        static void Main(string[] args)
        {
            //ProcessRequest(42);
            ProcessRequestAsync(42).GetAwaiter().GetResult();
        }





        
        public static void ProcessRequestWithWeakReference(int data)
        {
            

            // Potentially a lot of other code.
            // But processor is no longer used!
        }

        //[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        static void OldMain()
        {
#if false
            var wr = ProcessRequest2(42);
            GC.Collect();
            Console.WriteLine($"Main: Processor is {(wr.IsAlive ? "Alive" : "Dead")}");
#endif

#if false
            ProcessRequest3(42);
#endif

#if false
            ProcessRequestAsync(42).GetAwaiter().GetResult();
#endif
            //GC.Collect();
            //Console.WriteLine($"Main: Processor is {(wr.IsAlive ? "Alive" : "Dead")}");
        }

        public static WeakReference ProcessRequest2(int data)
        {
            var processor = new RequestProcessor();
            var wr = new WeakReference(processor);
            processor.ProcessRequest(data);
            return wr;
        }


        //[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void ProcessRequest4(int data)
        {
            // The simplest version that makes assembly the simplest.
            var processor = new RequestProcessor();
            processor.ProcessRequest(data);
        }

        
    }
}
