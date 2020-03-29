using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace Tuples
{
    public class TypeTuple : IEquatable<TypeTuple>
    {
        public bool Equals(TypeTuple other)
        {
            if (ReferenceEquals(null, other)) return false;
            return EqualityComparer<Type>.Default.Equals(this.Source, other.Source) &&
                    EqualityComparer<Type>.Default.Equals(this.Destination, other.Destination);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TypeTuple);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Source?.GetHashCode() ?? 0) << 16) ^ ((Destination?.GetHashCode() ?? 0) & 65535);
            }
        }

        public static bool operator ==(TypeTuple left, TypeTuple right)
        {
            if (ReferenceEquals(null, left)) return ReferenceEquals(null, right);
            return left.Equals(right);
        }

        public static bool operator !=(TypeTuple left, TypeTuple right)
        {
            if (ReferenceEquals(null, left)) return !ReferenceEquals(null, right);
            return !left.Equals(right);
        }

        public Type Source { get; }
        public Type Destination { get; }

        public TypeTuple(Type source, Type destination)
        {
            this.Source = source;
            this.Destination = destination;
        }
    }

    public class TypeTuple2<T1, T2> : IEquatable<TypeTuple2<T1, T2>>
    {
        public bool Equals(TypeTuple2<T1, T2> other)
        {
            if (ReferenceEquals(null, other)) return false;
            return EqualityComparer<T1>.Default.Equals(this.Source, other.Source) &&
                    EqualityComparer<T2>.Default.Equals(this.Destination, other.Destination);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TypeTuple2<T1, T2>);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Source?.GetHashCode() ?? 0) << 16) ^ ((Destination?.GetHashCode() ?? 0) & 65535);
            }
        }

        public static bool operator ==(TypeTuple2<T1, T2> left, TypeTuple2<T1, T2> right)
        {
            if (ReferenceEquals(null, left)) return ReferenceEquals(null, right);
            return left.Equals(right);
        }

        public static bool operator !=(TypeTuple2<T1, T2> left, TypeTuple2<T1, T2> right)
        {
            if (ReferenceEquals(null, left)) return !ReferenceEquals(null, right);
            return !left.Equals(right);
        }

        public T1 Source { get; }
        public T2 Destination { get; }

        public TypeTuple2(T1 source, T2 destination)
        {
            this.Source = source;
            this.Destination = destination;
        }
    }

    public class StructTypeTuple : IEquatable<StructTypeTuple>
    {
        public bool Equals(StructTypeTuple other)
        {
            if (ReferenceEquals(null, other)) return false;
            return Source == other.Source && Destination == other.Destination;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as StructTypeTuple);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Source?.GetHashCode() ?? 0) << 16) ^ ((Destination?.GetHashCode() ?? 0) & 65535);
            }
        }

        public static bool operator ==(StructTypeTuple left, StructTypeTuple right)
        {
            if (ReferenceEquals(null, left)) return ReferenceEquals(null, right);
            return left.Equals(right);
        }

        public static bool operator !=(StructTypeTuple left, StructTypeTuple right)
        {
            if (ReferenceEquals(null, left)) return !ReferenceEquals(null, right);
            return !left.Equals(right);
        }

        public Type Source { get; }
        public Type Destination { get; }

        public StructTypeTuple(Type source, Type destination)
        {
            this.Source = source;
            this.Destination = destination;
        }
    }

    class CustomerDto
    {
        
    }

    class Customer
    {
    }

    [MemoryDiagnoser]
    public class DelegateStorageRetrieve
    {
        private Dictionary<(Type, Type), Delegate> dictTupleStorage = new Dictionary<(Type, Type), Delegate>();
        private Dictionary<TypeTuple, Delegate> dictTypeTupleStorage = new Dictionary<TypeTuple, Delegate>();
        private Dictionary<TypeTuple2<Type, Type>, Delegate> genDictTypeTupleStorage = new Dictionary<TypeTuple2<Type, Type>, Delegate>();
        private Dictionary<System.Tuple<Type, Type>, Delegate> regularDictTypeTupleStorage = new Dictionary<System.Tuple<Type, Type>, Delegate>();
        private Dictionary<StructTypeTuple, Delegate> dictStructTypeTupleStorage = new Dictionary<StructTypeTuple, Delegate>();

        [GlobalSetup]
        public void Init()
        {
            Func<CustomerDto, Customer> activator = (x) => new Customer();
            dictTupleStorage.Add((typeof(CustomerDto), typeof(Customer)), activator);
            dictTypeTupleStorage.Add(new TypeTuple(typeof(CustomerDto), typeof(Customer)), activator);
            genDictTypeTupleStorage.Add(new TypeTuple2<Type, Type>(typeof(CustomerDto), typeof(Customer)), activator);
            dictStructTypeTupleStorage.Add(new StructTypeTuple(typeof(CustomerDto), typeof(Customer)), activator);
            regularDictTypeTupleStorage.Add(new Tuple<Type, Type>(typeof(CustomerDto), typeof(Customer)), activator);
        }

        [Benchmark]
        public Delegate DictionaryTuple()
        {
            var key = (typeof(CustomerDto), typeof(Customer));
            return dictTupleStorage[key];
        }

        [Benchmark]
        public Delegate DictionaryTypeTuple()
        {
            var key = new TypeTuple(typeof(CustomerDto), typeof(Customer));
            return dictTypeTupleStorage[key];
        }

        [Benchmark]
        public Delegate DictionaryGenericTypeTuple()
        {
            var key = new TypeTuple2<Type, Type>(typeof(CustomerDto), typeof(Customer));
            return genDictTypeTupleStorage[key];
        }

        [Benchmark]
        public Delegate DictionaryStructTypeTuple()
        {
            var key = new StructTypeTuple(typeof(CustomerDto), typeof(Customer));
            return dictStructTypeTupleStorage[key];
        }

        [Benchmark]
        public Delegate DictionarySystemTuple()
        {
            var key = new Tuple<Type, Type>(typeof(CustomerDto), typeof(Customer));
            return regularDictTypeTupleStorage[key];
        }
    }

    [MemoryDiagnoser]
    public class LocalFunctionBenchmark
    {
        private static int n = 42;

        [Benchmark]
        public bool DelegateInvocation()
        {
            Func<bool> fn = () => n == 42;
            return fn();
        }

        [Benchmark]
        public bool LocalFunctionInvocation()
        {
            return LocalFunctionInvocation_g__fn1_0();
        }

        //[CompilerGenerated]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static bool LocalFunctionInvocation_g__fn1_0()
        {
            return n == 42;
        }
    }

[MemoryDiagnoser]
public class MultidimentionalAarrayTests
{
    private int[,] m_multiArray = {{1,2,3}, {4,5,6}};
    private int[] m_regularArray = {1, 2};

    //[Benchmark]
    //public int MultiArrayLast()
    //{
    //    return m_multiArray.Cast<int>().First();
    //}

    [Benchmark]
    public int MultiArrayLastCustomCast()
    {
        return m_multiArray.Cast2<int>().First();
    }

        [Benchmark]
    public int MultiArrayGetEnumerator()
        {
            var e = m_multiArray.GetEnumerator();
            e.MoveNext();
            return (int) e.Current;
    }

    [Benchmark]
    public int RegularArrayLast()
    {
        return m_regularArray.First();
    }

    //[Benchmark]
    //public int RegularArrayLastWithAsEnumerable()
    //{
    //    return m_regularArray.AsEnumerable().First();
    //}

        [Benchmark]
    public int RegularArrayLastWithCast2()
    {
        return m_regularArray.Cast2<int>().First();
    }

    [Benchmark]
    public int MultiArrayWithAsEnumerable()
    {
        return m_multiArray.AsEnumerable().First();
    }
}

    public static class MultiDimentionalArrayEx
    {
        public static IEnumerable<T> AsEnumerable<T>(this T[,] array) where T:struct
        {
            foreach (var e in array) yield return e;
        }

        public static IEnumerable<T> AsEnumerable<T>(this T[] array) where T:struct
        {
            foreach (var e in array) yield return e;
        }

        public static IEnumerable<TResult> Cast2<TResult>(this IEnumerable source)
        {
            //IEnumerable<TResult> typedSource = source as IEnumerable<TResult>;
            //if (typedSource != null) return typedSource;
            //return CastIterator<TResult>(source);
            foreach (object obj in source) yield return (TResult)obj;
        }

        static IEnumerable<TResult> CastIterator<TResult>(IEnumerable source)
        {
            foreach (object obj in source) yield return (TResult)obj;
        }
    }

    public class PerfTesting
    {
        [BenchmarkDotNet.Attributes.Params(100, 200, -1, 42)]
         public int A { get; set; } // property with public setter

        [BenchmarkDotNet.Attributes.Params(10, 20, -1, 87)]
        public int B { get; set; } // public field
 
         public IEnumerable<int> ValuesForA => new[] { 100, 200, -1, 42 }; // public property
 
         public static IEnumerable<int> ValuesForB() => new[] { 10, 20, -1, 87 }; // public static method

        [Benchmark]
        public int Regular()
        {
            Regular(A, B, out int a, out int b);
            return a + b;
        }

        [Benchmark]
        public int TupleBased()
        {
            var (a, b) = TupleBased((A, B));
            return a + b;
        }

        public static (int a, int b) TupleBased((int x, int y) a)
        {
            return (a.x + a.y, a.x * a.y);
        }

        public static void Regular(int x, int y, out int a, out int b)
        {
            a = x + y;
            b = x * y;
        }
    }
}