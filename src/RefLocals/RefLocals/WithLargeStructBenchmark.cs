using BenchmarkDotNet.Attributes;

namespace RefLocals
{
    public class WithLargeStructBenchmark
    {
        private const int count = 1000_000;
        private WithLargeStruct wls = new WithLargeStruct();

        [Benchmark]
        public int L48()
        {
            int result = 0;
            for (int i = 0; i < count; i++)
            {
                result += wls.L48.N;
            }

            return result;
        }

        [Benchmark]
        public int L48_RO()
        {
            int result = 0;
            for (int i = 0; i < count; i++)
            {
                result += wls.L48_NonReadOnly.N;
            }

            return result;
        }

        //[Benchmark]
        //public int L32()
        //{
        //    int result = 0;
        //    for (int i = 0; i < count; i++)
        //    {
        //        result += wls.L32.N;
        //    }

        //    return result;
        //}

        //[Benchmark]
        //public int L16()
        //{
        //    int result = 0;
        //    for (int i = 0; i < count; i++)
        //    {
        //        result += wls.L16.N;
        //    }

        //    return result;
        //}

        //[Benchmark]
        //public int L4()
        //{
        //    int result = 0;
        //    for (int i = 0; i < count; i++)
        //    {
        //        result += wls.L4.N;
        //    }

        //    return result;
        //}

        [Benchmark]
        public int L48_ByValue()
        {
            int result = 0;
            for (int i = 0; i < count; i++)
            {
                result += wls.L48V.N;
            }

            return result;
        }

        [Benchmark]
        public int L48_ByValue_RO()
        {
            int result = 0;
            for (int i = 0; i < count; i++)
            {
                result += wls.L48NonReadOnnlyVal.N;
            }

            return result;
        }

        //[Benchmark]
        //public int L32_ByValue()
        //{
        //    int result = 0;
        //    for (int i = 0; i < count; i++)
        //    {
        //        result += wls.L32V.N;
        //    }

        //    return result;
        //}

        //[Benchmark]
        //public int L16_ByValue()
        //{
        //    int result = 0;
        //    for (int i = 0; i < count; i++)
        //    {
        //        result += wls.L16V.N;
        //    }

        //    return result;
        //}

        //[Benchmark]
        //public int L4_ByValue()
        //{
        //    int result = 0;
        //    for (int i = 0; i < count; i++)
        //    {
        //        result += wls.L4V.N;
        //    }

        //    return result;
        //}
    }
}