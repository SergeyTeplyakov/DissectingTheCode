``` ini

BenchmarkDotNet=v0.10.13, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.309)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical cores and 4 physical cores
Frequency=2531251 Hz, Resolution=395.0616 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2633.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2633.0


```
|         Method |       Mean |     Error |    StdDev |     Median |
|--------------- |-----------:|----------:|----------:|-----------:|
|            L48 | 1,726.5 us | 34.395 us | 86.921 us | 1,754.2 us |
|         L48_RO |   795.7 us | 17.660 us | 25.327 us |   783.3 us |
|    L48_ByValue | 1,334.0 us |  6.673 us |  6.242 us | 1,334.0 us |
| L48_ByValue_RO | 1,584.0 us |  4.574 us |  4.055 us | 1,584.8 us |
