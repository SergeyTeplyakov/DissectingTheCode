using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ReadOnlyStructs.Benchmark2
{
public readonly struct FairlyLargeStruct
{
    private readonly long l1, l2, l3, l4;
    public int N { get; }
    public FairlyLargeStruct(int n) : this() => N = n;
}

    public static class Ext
    {
        public static string ToS(ref this FairlyLargeStruct fs)
        {
            return string.Empty;
        }
    }

    public class Benchmarks
    {
private readonly int[] _data = Enumerable.Range(1, 100_000).ToArray();

[Benchmark]
public int AggregatePassedByValue()
{
    return DoAggregate(new FairlyLargeStruct(42));

    int DoAggregate(FairlyLargeStruct largeStruct)
    {
        int result = 0;
        foreach (int n in _data)
            result += n + largeStruct.N;
        return result;
    }
}

[Benchmark]
public int AggregatePassedByIn()
{
    return DoAggregate(new FairlyLargeStruct(42));

    int DoAggregate(in FairlyLargeStruct largeStruct)
    {
        int result = 0;
        foreach (int n in _data)
            result += n + largeStruct.N;
        return result;
    }
}
    }
}
