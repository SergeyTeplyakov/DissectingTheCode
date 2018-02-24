``` ini

BenchmarkDotNet=v0.10.12, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.192)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical cores and 4 physical cores
Frequency=2531249 Hz, Resolution=395.0619 ns, Timer=TSC
.NET Core SDK=2.1.2
  [Host]     : .NET Core 2.0.3 (Framework 4.6.25815.02), 64bit RyuJIT
  DefaultJob : .NET Core 2.0.3 (Framework 4.6.25815.02), 64bit RyuJIT


```
|                       Method |     Mean |    Error |   StdDev |   Median |
|----------------------------- |---------:|---------:|---------:|---------:|
| AggregateForNonReadOnlyField | 73.83 us | 1.473 us | 3.668 us | 72.65 us |
|    AggregateForReadOnlyField | 70.64 us | 1.354 us | 1.267 us | 70.78 us |
