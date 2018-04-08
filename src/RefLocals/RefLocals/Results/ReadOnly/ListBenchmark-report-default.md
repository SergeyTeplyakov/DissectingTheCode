
BenchmarkDotNet=v0.10.13, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.309)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical cores and 4 physical cores
Frequency=2531251 Hz, Resolution=395.0616 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2633.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2633.0


                    Method |   Categories |     Mean |     Error |    StdDev | Scaled | ScaledSD |
-------------------------- |------------- |---------:|----------:|----------:|-------:|---------:|
              TestArray_48 | BigStruct_48 | 265.1 us | 3.2939 us | 2.9200 us |   1.00 |     0.00 |
            TestListOfT_48 | BigStruct_48 | 490.6 us | 3.0316 us | 2.5315 us |   1.85 |     0.02 |
 TestNaiveImmutableList_48 | BigStruct_48 | 300.6 us | 5.8178 us | 5.7139 us |   1.13 |     0.02 |
                           |              |          |           |           |        |          |
              TestArray_32 | BigStruct_32 | 177.8 us | 2.2721 us | 1.8973 us |   1.00 |     0.00 |
            TestListOfT_32 | BigStruct_32 | 233.4 us | 1.7472 us | 1.5489 us |   1.31 |     0.02 |
 TestNaiveImmutableList_32 | BigStruct_32 | 218.0 us | 2.4994 us | 2.3380 us |   1.23 |     0.02 |
                           |              |          |           |           |        |          |
              TestArray_16 | BigStruct_16 | 144.7 us | 3.7124 us | 3.4726 us |   1.00 |     0.00 |
             TestListOfT16 | BigStruct_16 | 191.8 us | 2.5730 us | 2.4068 us |   1.33 |     0.03 |
  TestNaiveImmutableList16 | BigStruct_16 | 168.8 us | 2.0041 us | 1.8747 us |   1.17 |     0.03 |
                           |              |          |           |           |        |          |
               TestArray_4 |  BigStruct_4 | 121.3 us | 0.3932 us | 0.3678 us |   1.00 |     0.00 |
             TestListOfT_4 |  BigStruct_4 | 178.9 us | 4.6555 us | 3.8876 us |   1.48 |     0.03 |
  TestNaiveImmutableList_4 |  BigStruct_4 | 145.3 us | 2.8698 us | 4.0230 us |   1.20 |     0.03 |
