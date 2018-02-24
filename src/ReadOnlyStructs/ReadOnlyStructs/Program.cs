using BenchmarkDotNet.Running;
using System;

namespace ReadOnlyStructs
{
    readonly struct Foo
    {

    }
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmarks>();
        }
    }
}
