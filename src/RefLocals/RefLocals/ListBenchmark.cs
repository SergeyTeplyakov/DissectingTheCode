#define UseReadOnly
#define UseStructs

using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Configs;

namespace RefLocals
{
    
    class LargeClass_32
    {
        public int N { get; }
        private long l1, l2, l3;
        
        public LargeClass_32(int n) => N = n;
    }
#if UseReadOnly
    readonly
#endif
#if UseStructs
    struct
#else
class
#endif
        
        LargeStruct_48
{
    private readonly long l1, l2, l3, l4, l5;
    public int N { get; }

        public LargeStruct_48(int n)
#if UseStructs
        : this()
#endif
        => N = n;
}

    struct
        LargeStruct_48_NotReadonly
    {
        public int N { get; }
        private readonly long l1, l2, l3, l4, l5;
        
        public LargeStruct_48_NotReadonly(int n)
            : this()
            => N = n;
    }

#if UseReadOnly
    readonly
#endif
#if UseStructs
        struct
#else
class
#endif
        LargeStruct_40
    {
        private readonly long l1, l2, l3, l4;
        public int N { get; }

        public LargeStruct_40(int n)
#if UseStructs
            : this()
#endif
            => N = n;
    }

#if UseReadOnly
    readonly
#endif
#if UseStructs
        struct
#else
class
#endif
        LargeStruct_32
    {
        private readonly long l1, l2, l3;
        public int N { get; }

        public LargeStruct_32(int n)
#if UseStructs
            : this()
#endif
            => N = n;
    }

#if UseReadOnly
    readonly
#endif
#if UseStructs
        struct
#else
class
#endif
        LargeStruct_16
    {
        private readonly long l1;
        public int N { get; }
        public LargeStruct_16(int n)
#if UseStructs
            : this()
#endif
            => N = n;
    }

#if UseReadOnly
    readonly
#endif
#if UseStructs
        struct
#else
class
#endif
        Struct_4
    {
        public int N { get; }
        public Struct_4(int n)
#if UseStructs
            : this()
#endif
            => N = n;
    }

    //[RPlotExporter, AsciiDocExporter, MarkdownExporter]
    /*

                    Method |   Categories |     Mean |    Error |   StdDev | Scaled | ScaledSD |
-------------------------- |------------- |---------:|---------:|---------:|-------:|---------:|
              TestArray_48 | BigStruct_48 | 262.5 us | 4.707 us | 4.403 us |   1.00 |     0.00 |
            TestListOfT_48 | BigStruct_48 | 505.8 us | 2.721 us | 2.412 us |   1.93 |     0.03 |
 TestNaiveImmutableList_48 | BigStruct_48 | 472.8 us | 8.813 us | 8.244 us |   1.80 |     0.04 |
                           |              |          |          |          |        |          |
              TestArray_32 | BigStruct_32 | 176.5 us | 2.980 us | 2.787 us |   1.00 |     0.00 |
            TestListOfT_32 | BigStruct_32 | 234.3 us | 1.673 us | 1.397 us |   1.33 |     0.02 |
 TestNaiveImmutableList_32 | BigStruct_32 | 219.4 us | 2.934 us | 2.601 us |   1.24 |     0.02 |
                           |              |          |          |          |        |          |
              TestArray_16 | BigStruct_16 | 143.9 us | 2.036 us | 1.700 us |   1.00 |     0.00 |
             TestListOfT16 | BigStruct_16 | 237.1 us | 4.629 us | 6.785 us |   1.65 |     0.05 |
  TestNaiveImmutableList16 | BigStruct_16 | 214.3 us | 4.192 us | 5.301 us |   1.49 |     0.04 |
                           |              |          |          |          |        |          |
               TestArray_4 |  BigStruct_4 | 129.3 us | 1.678 us | 1.570 us |   1.00 |     0.00 |
             TestListOfT_4 |  BigStruct_4 | 186.5 us | 3.724 us | 5.098 us |   1.44 |     0.04 |
  TestNaiveImmutableList_4 |  BigStruct_4 | 132.6 us | 2.081 us | 1.845 us |   1.03 |     0.02 |        
        
        Method |     Mean | Scaled |
-------------------------- |---------:|-------:|
              TestArray_48 | 258.3 us |   1.00 |
            TestListOfT_48 | 488.9 us |   1.89 |
 TestNaiveImmutableList_48 | 444.8 us |   1.72 |
                           |          |        |
              TestArray_32 | 174.4 us |   1.00 |
            TestListOfT_32 | 233.8 us |   1.34 |
 TestNaiveImmutableList_32 | 219.2 us |   1.26 |
                           |          |        |
              TestArray_16 | 143.7 us |   1.00 |
             TestListOfT16 | 192.5 us |   1.34 |
  TestNaiveImmutableList16 | 167.8 us |   1.17 |
                           |          |        |
               TestArray_4 | 121.7 us |   1.00 |
             TestListOfT_4 | 174.7 us |   1.44 |
  TestNaiveImmutableList_4 | 133.1 us |   1.09 |


Readonly

                    Method |     Mean | Scaled |
-------------------------- |---------:|-------:|
              TestArray_48 | 265.1 us |   1.00 |
            TestListOfT_48 | 490.6 us |   1.85 |
 TestNaiveImmutableList_48 | 300.6 us |   1.13 |
                           |          |        |
              TestArray_32 | 177.8 us |   1.00 |
            TestListOfT_32 | 233.4 us |   1.31 |
 TestNaiveImmutableList_32 | 218.0 us |   1.23 |
                           |          |        |
              TestArray_16 | 144.7 us |   1.00 |
             TestListOfT16 | 191.8 us |   1.33 |
  TestNaiveImmutableList16 | 168.8 us |   1.17 |
                           |          |        |
               TestArray_4 | 121.3 us |   1.00 |
             TestListOfT_4 | 178.9 us |   1.48 |
  TestNaiveImmutableList_4 | 145.3 us |   1.20 |


        With bounds checking

                    Method |      Mean |     Error |    StdDev |
------------------ |----------:|----------:|----------:|
      TestArray_48 | 175.08 us | 3.4231 us | 5.9047 us |
    TestListOfT_48 | 314.42 us | 5.7630 us | 4.8123 us |
 TestNaiveImmutableList_48 | 399.11 us | 9.7882 us | 9.1559 us |
      TestArray_32 | 110.65 us | 2.2046 us | 2.0622 us |
    TestListOfT_32 | 149.73 us | 2.6543 us | 2.4828 us |
 TestNaiveImmutableList_32 | 257.76 us | 2.5878 us | 2.4206 us |
      TestArray_16 |  92.26 us | 1.0444 us | 0.9770 us |
     TestListOfT16 | 124.56 us | 1.5026 us | 1.4055 us |
  TestNaiveImmutableList16 | 246.43 us | 3.8644 us | 3.6147 us |
       TestArray_4 |  79.72 us | 0.6676 us | 0.6245 us |
     TestListOfT_4 | 111.51 us | 0.9640 us | 0.8545 us |
  TestNaiveImmutableList_4 | 260.10 us | 4.9178 us | 4.6001 us |
     */
    [RPlotExporter, AsciiDocExporter, MarkdownExporter]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public class ListBenchmark
    {
        private static LargeStruct_48[] CreateArray_48() => Enumerable.Range(1, elementsCount).Select(v => new LargeStruct_48(v)).ToArray();
        
        private readonly List<LargeStruct_48> _list48 = new List<LargeStruct_48>(CreateArray_48());
        private readonly NaiveImmutableList<LargeStruct_48> _naiveImmutableList48 = new NaiveImmutableList<LargeStruct_48>(CreateArray_48());

        private static LargeStruct_48_NotReadonly[] CreateArray_48_NotReadOnly() => Enumerable.Range(1, elementsCount).Select(v => new LargeStruct_48_NotReadonly(v)).ToArray();

        private readonly LargeStruct_48_NotReadonly[] _array48NotRO = CreateArray_48_NotReadOnly();
        private readonly List<LargeStruct_48_NotReadonly> _list48NotRO = new List<LargeStruct_48_NotReadonly>(CreateArray_48_NotReadOnly());
        private readonly NaiveImmutableList<LargeStruct_48_NotReadonly> _naiveImmutableList48NotRO = new NaiveImmutableList<LargeStruct_48_NotReadonly>(CreateArray_48_NotReadOnly());

        private static LargeStruct_32[] CreateArray_32() => Enumerable.Range(1, elementsCount).Select(v => new LargeStruct_32(v)).ToArray();

        private readonly LargeStruct_32[] _array32 = CreateArray_32();
        private readonly List<LargeStruct_32> _list32 = new List<LargeStruct_32>(CreateArray_32());
        private readonly NaiveImmutableList<LargeStruct_32> _naiveImmutableList32 = new NaiveImmutableList<LargeStruct_32>(CreateArray_32());

        private static LargeStruct_16[] CreateArray_16() => Enumerable.Range(1, elementsCount).Select(v => new LargeStruct_16(v)).ToArray();

        private readonly LargeStruct_16[] _array16 = CreateArray_16();
        private readonly List<LargeStruct_16> _list16 = new List<LargeStruct_16>(CreateArray_16());
        private readonly NaiveImmutableList<LargeStruct_16> _naiveImmutableList16 = new NaiveImmutableList<LargeStruct_16>(CreateArray_16());

        private static Struct_4[] CreateArray4() => Enumerable.Range(1, elementsCount).Select(v => new Struct_4(v)).ToArray();

        private readonly Struct_4[] _array4 = CreateArray4();
        private readonly List<Struct_4> _list4 = new List<Struct_4>(CreateArray4());
        private readonly NaiveImmutableList<Struct_4> _naiveImmutableList4 = new NaiveImmutableList<Struct_4>(CreateArray4());

        private static LargeClass_32[] CreateArrayClass32() => Enumerable.Range(1, elementsCount).Select(v => new LargeClass_32(v)).ToArray();

        private readonly LargeClass_32[] arrayClass32 = CreateArrayClass32();
        private readonly List<LargeClass_32> listClass32 = new List<LargeClass_32>(CreateArrayClass32());
        private readonly NaiveImmutableList<LargeClass_32> _naiveImmutableListClass32 = new NaiveImmutableList<LargeClass_32>(CreateArrayClass32());

        private const int elementsCount = 100_000;
        private readonly LargeStruct_48[] _array48 = CreateArray_48();

        [BenchmarkCategory("BigStruct_48")]
        [Benchmark(Baseline = true)]
        public int TestArray_48()
        {
            int result = 0;
            // Using elementsCound but not array.Length to force the bounds check
            // on each iteration.
            for (int i = 0; i < elementsCount; i++)
            {
                result = _array48[i].N;
            }

            return result;
        }

        [BenchmarkCategory("BigStruct_48")]
        [Benchmark]
        public int TestListOfT_48()
        {
            int result = 0;
            for (int i = 0; i < elementsCount; i++)
            {
                result = _list48[i].N;
            }

            return result;
        }

        [BenchmarkCategory("BigStruct_48")]
        [Benchmark]
        public int TestNaiveImmutableList_48()
        {
            int result = 0;
            for (int i = 0; i < elementsCount; i++)
            {
                result = _naiveImmutableList48[i].N;
            }

            return result;
        }

        [BenchmarkCategory("BigStruct_32")]
        [Benchmark(Baseline = true)]
        public int TestArray_32()
        {
            int result = 0;
            // Using elementsCound but not array.Length to force the bounds check
            // on each iteration.
            for (int i = 0; i < elementsCount; i++)
            {
                result = _array32[i].N;
            }

            return result;
        }

        [BenchmarkCategory("BigStruct_32")]
        [Benchmark]
        public int TestListOfT_32()
        {
            int result = 0;
            for (int i = 0; i < elementsCount; i++)
            {
                result = _list32[i].N;
            }

            return result;
        }

        [BenchmarkCategory("BigStruct_32")]
        [Benchmark]
        public int TestNaiveImmutableList_32()
        {
            int result = 0;
            for (int i = 0; i < elementsCount; i++)
            {
                result = _naiveImmutableList32[i].N;
            }

            return result;
        }

        [BenchmarkCategory("BigStruct_16")]
        [Benchmark(Baseline = true)]
        public int TestArray_16()
        {
            int result = 0;
            // Using elementsCound but not array.Length to force the bounds check
            // on each iteration.
            for (int i = 0; i < elementsCount; i++)
            {
                result = _array16[i].N;
            }

            return result;
        }

        [BenchmarkCategory("BigStruct_16")]
        [Benchmark]
        public int TestListOfT16()
        {
            int result = 0;
            for (int i = 0; i < elementsCount; i++)
            {
                result = _list16[i].N;
            }

            return result;
        }

        [BenchmarkCategory("BigStruct_16")]
        [Benchmark]
        public int TestNaiveImmutableList16()
        {
            int result = 0;
            for (int i = 0; i < elementsCount; i++)
            {
                result = _naiveImmutableList16[i].N;
            }

            return result;
            
        }

        [BenchmarkCategory("BigStruct_4")]
        [Benchmark(Baseline = true)]
        public int TestArray_4()
        {
            int result = 0;
            // Using elementsCound but not array.Length to force the bounds check
            // on each iteration.
            for (int i = 0; i < elementsCount; i++)
            {
                result = _array4[i].N;
            }

            return result;
        }

        [BenchmarkCategory("BigStruct_4")]
        [Benchmark]
        public int TestListOfT_4()
        {
            int result = 0;
            for (int i = 0; i < elementsCount; i++)
            {
                result = _list4[i].N;
            }

            return result;
        }

        [BenchmarkCategory("BigStruct_4")]
        [Benchmark]
        public int TestNaiveImmutableList_4()
        {
            int result = 0;
            for (int i = 0; i < elementsCount; i++)
            {
                result = _naiveImmutableList4[i].N;
            }

            return result;
        }
    }
}