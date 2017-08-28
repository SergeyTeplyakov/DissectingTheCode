using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LockingCost
{
    public class Node
    {
        private static int s_id;
        private static object o = new object();
        private int m_id = -1;

        //private readonly object syncRoot = new object();

        private int m_hashCode;
        private object syncRoot = new object();

        public Node(bool useHashCode)
        {
            if (useHashCode)
            {
                m_hashCode = EqualityComparer<Node>.Default.GetHashCode();
                //m_hashCode = this.GetHashCode();
            }
        }

        public int GetId()
        {
            if (m_id != -1)
            {
                return m_id;
            }

            //lock(this)
            {
                if (m_id != -1)
                {
                    return m_id;
                }

                m_id = Interlocked.Increment(ref s_id);
                return m_id;
            }
        }
    }

    [MemoryDiagnoser]
    public class NodeLocksTest
    {
        const int Count = 100_000;

        [BenchmarkDotNet.Attributes.Setup]
        public void SetUp()
        {
            //m_nodeWithLocks = Enumerable.Range(1, Count).Select(n => new NodeWithLock.Node()).ToList();
            //m_nodeWithNoLocks = Enumerable.Range(1, Count).Select(n => new NodeNoLock.Node()).ToList();
            //m_nodeWithLocksAndGetHashCode = Enumerable.Range(1, Count).Select(n => new NodeWithLockAndHashCode.Node()).ToList();
        }

        List<NodeWithLock.Node> m_nodeWithLocks => Enumerable.Range(1, Count).Select(n => new NodeWithLock.Node()).ToList();

        [Benchmark]
        public long NodeWithLock()
        {
            // m_nodeWithLocks has 5 million instances
            return m_nodeWithLocks
                .AsParallel()
                .WithDegreeOfParallelism(16)
                .Select(n => (long)n.Id).Sum();
        }

        [Benchmark]
        public long NodeWithNoLock()
        {
            // m_nodeWithNoLocks has 5 million instances
            return m_nodeWithNoLocks
                .AsParallel()
                .WithDegreeOfParallelism(16)
                .Select(n => (long)n.Id).Sum();
        }

        List<NodeNoLock.NoLockNode> m_nodeWithNoLocks => Enumerable.Range(1, Count).Select(n => new NodeNoLock.NoLockNode()).ToList();
        List<NodeWithLockAndHashCode.Node> m_nodeWithLocksAndGetHashCode => Enumerable.Range(1, Count).Select(n => new NodeWithLockAndHashCode.Node()).ToList();

        [Benchmark]
        public long NodeWithLocksAndGetHashCode()
        {
            return m_nodeWithLocksAndGetHashCode
            .AsParallel()
            .WithDegreeOfParallelism(16)
            .Select(n => (long)n.Id).Sum();
        }
    }
    
    class Program
    {
        public static void CheckInParallel(int number, bool print, bool useHashCode)
        {
            var nodes = Enumerable.Range(1, number).Select(n => new Node(useHashCode));
            var sw = Stopwatch.StartNew();
            List<int> ids = nodes.AsParallel().WithDegreeOfParallelism(16).Select(n => n.GetId()).ToList();
            var duration = sw.ElapsedMilliseconds;

            if (print)
            {
                Console.WriteLine($"Converted {number} items in {duration}ms");
            }
        }

        static void Main(string[] args)
        {
            const int count = 10_000_000;

            //NodeWithLock.Node.Measure(100, isWarmUp: true);
            //NodeWithLock.Node.Measure(count, isWarmUp: false);

            //NodeNoLock.Node.Measure(100, isWarmUp: true);
            //NodeNoLock.Node.Measure(count, isWarmUp: false);

            //NodeWithLockAndHashCode.Node.Measure(100, isWarmUp: true);
            //NodeWithLockAndHashCode.Node.Measure(count, isWarmUp: false);

            //NodeLockOnSyncRootWithHashCode.Node.Measure(100, isWarmUp: true);
            //NodeLockOnSyncRootWithHashCode.Node.Measure(count, isWarmUp: false);
            //new CopyToBenchmark().ObjectWrapper_BufferCopy();
            
            //BenchmarkDotNet.Running.BenchmarkRunner.Run<CopyToBenchmark>();
            BenchmarkDotNet.Running.BenchmarkRunner.Run<NodeLocksTest>();

            //CheckInParallel(10, false, true);
            //CheckInParallel(10, false, true);
            //CheckInParallel(10, false, true);
            //Console.WriteLine("Press Enter");
            //Console.ReadLine();
            //CheckInParallel(10_000_000, true, false);
            //Console.WriteLine("Press Enter to collect ");
            //Console.ReadLine();
            //GC.Collect();

            //Console.WriteLine("Press Enter");
            //Console.ReadLine();
            //CheckInParallel(10_000_000, true, true);
            Console.WriteLine("Press Enter");
            Console.ReadLine();
        }
    }
}
