using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace RefLocals
{
    public struct KVP<TKey, TValue>
    {
        private readonly TKey key;
        private readonly TValue value;

        public KVP(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }

        // Ref returns are not suitable for structs.
        //public ref readonly TKey Key => ref key;
        //public ref readonly TValue Value => ref value;
    }

    class ReadonlyRefSample
    {
        public readonly int n;
        public readonly string s;

        private int mutableN;

        // Ok
        public ref readonly int GetN() => ref n;

        // Ok.
        public ref int MutableN => ref mutableN;
        public ref int GetMutable() => ref mutableN;

        public int GetValue() => mutableN;

        // Error: A readonly field cannot be returned by writable reference
        //public ref int GetN2() => ref n;
        
        // Ok
        public ref readonly int N => ref n;
    }
    class Program
    {

        static void M1(in string s)
        {
        }

        //static void M1(string s) { }
        static void M1(string s) { }

        static ref int Foo(ref int s)
        {
            return ref s;
        }

        static ref readonly double Foo(in double s)
        {
            return ref s;
        }

        static T foo<T>(T arg)
        {
            return default(T);

        }

        static T bar<T>(ref T arg)
        {
            return default(T);

        }

        static int bar2(ref int arg)
        {
            return default(int);

        }

        private static int n;

        ref int Callee(ref string arg)
        {
            return ref n;
        }

        //ref T Caller<T>() where T : class
        //{
        //    T t = default(T);
        //    //string str = string.Empty;

        //    // DANGER!! returning a reference to the local data
        //    //return ref Callee(ref str);
        //    return ref t;
        //}

        //ref string Foo()
        //{
        //    string s = "";
        //    ref string rf = ref s;
        //    Func<RefLocals > a = () => Console.WriteLine(s);
        //    //string str = string.Empty;

        //    // DANGER!! returning a reference to the local data
        //    //return ref Callee(ref str);
        //    return ref s;
        //}

        delegate ref int MyD();

        static ref int baz(ref int n)
        {
            int n2 = 42;
            // Works, but changing it to ref n2 will not.
            ref var refn = ref n;

            return ref refn;
        }

        public delegate ref string ByRef();

        private static string s;
        static ref string Foo() => ref s;

        static void Main(string[] args)
        {
            //ByRef d = Foo;
            //BenchmarkDotNet.Running.BenchmarkRunner.Run<WithLargeStructBenchmark>();
            BenchmarkDotNet.Running.BenchmarkRunner.Run<ListBenchmark>();
            
            //return;

            //var smpl = new ReadonlyRefSample();

            
            //string str = string.Empty;
            //ref var xx = ref str;
            ////ref var xx = ref args;

            //smpl.MutableN++;
            //smpl.GetMutable()++;


            //// Not LHS
            //// smpl.GetValue()++;

            //string s = "foo";
            //int n = 42;

            ////ref int x = ref baz();

            ////bar(ref baz());
            ////Foo(Foo(ref n));
            //ref string rf = ref s;

            //ref var rf2 = ref s;
            // var rf3 = ref s; // compiler error

            //M1(s);
        }
    }
}
