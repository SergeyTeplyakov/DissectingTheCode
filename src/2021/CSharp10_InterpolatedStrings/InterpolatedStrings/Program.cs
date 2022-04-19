
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using System;

namespace InterpolatedStrings
{
    public struct MyStruct { }

    [MemoryDiagnoser]
    public class PerformanceBenchmark
    {
        private readonly DateTime _when = DateTime.Now;
        private readonly long _v1 = 1;
        private readonly long _v2 = 2;
        private readonly long _v3 = 3;

        [Benchmark]
        public string StringFormat()
        {
            return string.Format("When: {0}, V1={1}, V2={2}, V3={2}", _when, _v1, _v2, _v3);
        }

        [Benchmark]
        public string NewInterpolation()
        {
            return $"When: {_when}, V1={_v1}, V2={_v2}, V3={_v2}";
        }
    }

    internal static class Program
    {
        static unsafe void Main(string[] args)
        {
            int x = 42;
            try
            {
                Contract2.Assert(x > 10, $"x == {42}.");
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            //BenchmarkRunner.Run<PerformanceBenchmark>();
        }
    }
}
