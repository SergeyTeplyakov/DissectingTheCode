using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReadOnlyStructs
{
    public struct FairlyLargeStruct
    {
        public readonly long l1;
        public readonly long l2;
        public readonly long l3;
        public int N;

        public FairlyLargeStruct(int n)
        {
            N = n;
            l1 = 1;
            l2 = 2;
            l3 = 3;
        }

    }
    public class Benchmarks
    {
        private FairlyLargeStruct _nonReadOnlyStruct = new FairlyLargeStruct(42);
        private readonly FairlyLargeStruct _readOnlyStruct = new FairlyLargeStruct(42);

        private readonly int[] _data = Enumerable.Range(1, 100_000).ToArray();

        [Benchmark]
        public int AggregateForNonReadOnlyField()
        {
            int result = 0;
            foreach(int n in _data)
            {
                result += (n + _nonReadOnlyStruct.N);
            }

            return result;
        }

        [Benchmark]
        public int AggregateForReadOnlyField()
        {
            int result = 0;
            foreach (int n in _data)
            {
                result += (n + _readOnlyStruct.N);
            }

            return result;
        }
    }
}
