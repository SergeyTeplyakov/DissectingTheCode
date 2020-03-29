using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;

namespace ReadOnlyStructs
{
    readonly struct Foo
    {

    }
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmark2.Benchmarks>();
        }
    }
}
