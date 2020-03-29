using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace NullCoalescing
{
public struct Struct1
{
    public int N { get; }
    public string S { get; }
    public Struct1(int n, string s = null) { N = n; S = s; }

    public override int GetHashCode() =>
        N ^
        S?.GetHashCode() ?? 0;

        public override bool Equals(object obj) => 
        obj is Struct1 other && N == other.N && string.Equals(S, other.S);
}

public struct Struct2
{
    public int N { get; }
    public string S { get; }
    public Struct2(int n, string s = null) { N = n; S = s; }

    public override int GetHashCode() => 
        S?.GetHashCode() ?? 0 ^
        N;

    public override bool Equals(object obj) => 
        obj is Struct1 other && N == other.N && string.Equals(S, other.S);
}

    public class MyBenchmark
    {
private const int count = 10000;
private static Struct1[] _arrayStruct1 =
    Enumerable.Range(1, count).Select(n => new Struct1(n)).ToArray();
private static Struct2[] _arrayStruct2 =
    Enumerable.Range(1, count).Select(n => new Struct2(n)).ToArray();

[Benchmark]
public int Struct1() => new HashSet<Struct1>(_arrayStruct1).Count;

[Benchmark]
public int Struct2() => new HashSet<Struct2>(_arrayStruct2).Count;
    }

    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<MyBenchmark>();
        }
    }
}
