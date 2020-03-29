``` ini

BenchmarkDotNet=v0.10.12, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.248)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical cores and 4 physical cores
Frequency=2531250 Hz, Resolution=395.0617 ns, Timer=TSC
.NET Core SDK=2.1.2
  [Host]     : .NET Core 2.0.3 (Framework 4.6.25815.02), 64bit RyuJIT
  DefaultJob : .NET Core 2.0.3 (Framework 4.6.25815.02), 64bit RyuJIT


```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
| AggregatePassedByValue |  71.24 us | 0.3150 us | 0.2278 us |
|    AggregatePassedByIn | 124.02 us | 3.2885 us | 9.6963 us |
