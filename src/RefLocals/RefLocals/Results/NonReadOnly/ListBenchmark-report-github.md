``` ini

BenchmarkDotNet=v0.10.13, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.309)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical cores and 4 physical cores
Frequency=2531251 Hz, Resolution=395.0616 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2633.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2633.0


```
|                    Method |   Categories |     Mean |    Error |   StdDev | Scaled | ScaledSD |
|-------------------------- |------------- |---------:|---------:|---------:|-------:|---------:|
|              TestArray_48 | BigStruct_48 | 262.5 us | 4.707 us | 4.403 us |   1.00 |     0.00 |
|            TestListOfT_48 | BigStruct_48 | 505.8 us | 2.721 us | 2.412 us |   1.93 |     0.03 |
| TestNaiveImmutableList_48 | BigStruct_48 | 472.8 us | 8.813 us | 8.244 us |   1.80 |     0.04 |
|                           |              |          |          |          |        |          |
|              TestArray_32 | BigStruct_32 | 176.5 us | 2.980 us | 2.787 us |   1.00 |     0.00 |
|            TestListOfT_32 | BigStruct_32 | 234.3 us | 1.673 us | 1.397 us |   1.33 |     0.02 |
| TestNaiveImmutableList_32 | BigStruct_32 | 219.4 us | 2.934 us | 2.601 us |   1.24 |     0.02 |
|                           |              |          |          |          |        |          |
|              TestArray_16 | BigStruct_16 | 143.9 us | 2.036 us | 1.700 us |   1.00 |     0.00 |
|             TestListOfT16 | BigStruct_16 | 237.1 us | 4.629 us | 6.785 us |   1.65 |     0.05 |
|  TestNaiveImmutableList16 | BigStruct_16 | 214.3 us | 4.192 us | 5.301 us |   1.49 |     0.04 |
|                           |              |          |          |          |        |          |
|               TestArray_4 |  BigStruct_4 | 129.3 us | 1.678 us | 1.570 us |   1.00 |     0.00 |
|             TestListOfT_4 |  BigStruct_4 | 186.5 us | 3.724 us | 5.098 us |   1.44 |     0.04 |
|  TestNaiveImmutableList_4 |  BigStruct_4 | 132.6 us | 2.081 us | 1.845 us |   1.03 |     0.02 |
