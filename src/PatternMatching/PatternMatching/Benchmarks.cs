using System;
using BenchmarkDotNet.Attributes;

namespace PatternMatching
{
    [MemoryDiagnoser]
    public class Benchmarks
    {

        private static int n = 42;
        [Benchmark]
        public bool IsCheck()
        {
            return n is 42;
        }

        [Benchmark]
        public bool CheckWithOperator()
        {
            return n == 42;
        }
    }
}