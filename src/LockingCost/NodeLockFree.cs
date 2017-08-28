using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LockingCost.NodeLockFree
{
    public class Node
    {
        public const int InvalidId = -1;
        private static int s_idCounter;
        private object syncRoot = new object();
        private int m_id = InvalidId;

        public int Id
        {
            get
            {
                if (m_id == InvalidId)
                {
                    //lock(this)
                    {
                        //if (m_id == InvalidId)
                        {
                            m_id = Interlocked.Increment(ref s_idCounter);
                        }
                    }
                }

                return m_id;
            }
        }

        static Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();

        public static void Measure(int numberOfNodes, bool isWarmUp)
        {
            var nodes = Enumerable.Range(1, numberOfNodes).Select(n => new Node()).ToList();
            if (!isWarmUp)
            {
                Console.WriteLine("Press any key to get Id's");
                Console.ReadLine();
            }
            var mem1 = Process.GetCurrentProcess().WorkingSet64;
            var sw = Stopwatch.StartNew();
            long count = nodes
            .AsParallel()
            .WithDegreeOfParallelism(16)
            .Select(n => (long)n.Id).Sum();
            var mem2 = Process.GetCurrentProcess().WorkingSet64;
            if (!isWarmUp)
            {
                System.Console.WriteLine($"No lock: {count}, at {sw.ElapsedMilliseconds}ms, allocs: {mem2 - mem1}");
            }
        }
    }
}
