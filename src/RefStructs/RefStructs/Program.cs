using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace RefStructs
{
    public readonly struct RefStruct
    {
        public int X { get; }
        public int Y { get; }

        public RefStruct(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public struct RefStructWithNoModifier
    {
        public int X { get; }
        public int Y { get; }

        public RefStructWithNoModifier(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public readonly struct Small
    {
        public readonly byte N;

        public Small(byte n) => N = n;
    }

    public class MyBenchmark
    {
        private readonly RefStruct m_rf = new RefStruct(1, 42);
        private readonly RefStructWithNoModifier m_rf2 = new RefStructWithNoModifier(1, 42);

        private readonly Small m_small = new Small(42);

        //[MethodImpl(MethodImplOptions.NoInlining)]
        //[Benchmark]
        //public int WithReadOnlyModifier()
        //{
        //    return m_rf.X + m_rf.Y;
        //}

        //[MethodImpl(MethodImplOptions.NoInlining)]
        //[Benchmark]
        //public int WithoutReadOnlyModifier()
        //{
        //    return m_rf2.X + m_rf2.Y;
        //}

        //[MethodImpl(MethodImplOptions.NoInlining)]
        //[Benchmark]
        //public int PassedByValue()
        //{
        //    return GetN2(m_small);
        //}

        //[MethodImpl(MethodImplOptions.NoInlining)]
        //[Benchmark]
        //public int PassedByIn()
        //{
        //    return GetN(m_small);
        //}

        //[MethodImpl(MethodImplOptions.NoInlining)]
        //private static int GetN(in Small s) => s.N;

        //[MethodImpl(MethodImplOptions.NoInlining)]
        //private static int GetN2(Small s) => s.N;
    }

    class Program
    {
        static void Main(string[] args)
        {
            var s = "foobar";

            Span<char> cs = new Span<char>(s.ToCharArray());

            foreach (var c in cs)
            {
                Console.WriteLine(c);
            }


        }
    }
}
