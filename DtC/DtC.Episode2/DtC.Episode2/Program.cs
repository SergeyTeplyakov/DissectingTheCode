
using System.Diagnostics;
using System.Runtime.CompilerServices;
using DtC.Episode2.EventHandlers;

namespace DtC.Episode2
{
    internal class Program
    {
        public static void UseDataRefresher()
        {
            Func<byte[]> fetcher = () =>
            {
                Console.WriteLine("Fetching data...");
                return new byte[1];
            };
            Console.ReadLine();
            var refresher = new DataRefresherFixed(fetcher);
            Thread.Sleep(2000); // Simulate some work
        }

        static void FooBar()
        {
            FooBar();
        }

        static void Main(string[] args)
        {
            UseDataRefresher();

            GC.Collect();
            Console.WriteLine("Forced the GC");
            Console.ReadLine();
        }
    }
}
