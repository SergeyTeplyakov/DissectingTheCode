``` ini

BenchmarkDotNet=v0.10.12, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.248)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical cores and 4 physical cores
Frequency=2531250 Hz, Resolution=395.0617 ns, Timer=TSC
.NET Core SDK=2.1.2
  [Host]     : .NET Core 2.0.3 (Framework 4.6.25815.02), 64bit RyuJIT
  DefaultJob : .NET Core 2.0.3 (Framework 4.6.25815.02), 64bit RyuJIT


```
|                       Method |     Mean |    Error |   StdDev |
|----------------------------- |---------:|---------:|---------:|
| AggregateForNonReadOnlyField | 91.19 us | 1.811 us | 2.597 us |
|    AggregateForReadOnlyField | 89.25 us | 1.775 us | 3.705 us |
