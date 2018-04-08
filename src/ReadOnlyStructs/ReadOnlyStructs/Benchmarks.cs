using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ReadOnlyStructs
{
public readonly struct FairlyLargeStruct
{
    private readonly long l1, l2, l3, l4;
    public int N { get; }
    public FairlyLargeStruct(int n) : this() => N = n;
}

    //[StructLayout(LayoutKind.Auto, Size = 32)]
    //public struct FairlyLargeStruct
    //{
    //    public int N { get; }
    //    public FairlyLargeStruct2(int n) => N = n;
    //}

    public class Benchmarks
    {
private FairlyLargeStruct _nonReadOnlyStruct = new FairlyLargeStruct(42);
private readonly FairlyLargeStruct _readOnlyStruct = new FairlyLargeStruct(42);
private readonly int[] _data = Enumerable.Range(1, 100_000).ToArray();
        
[Benchmark]
public int AggregateForNonReadOnlyField()
{
    int result = 0;
    foreach (int n in _data)
        result += n + _nonReadOnlyStruct.N;
    return result;
}

[Benchmark]
public int AggregateForReadOnlyField()
{
    int result = 0;
    foreach (int n in _data)
        result += n + _readOnlyStruct.N;
    return result;
}
    }
}
