using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace CSharpCompilerStrictMode
{
    public readonly struct RelativePath
    {
        private readonly string x;
        /// <summary>
        /// Invalid path for uninitialized fields.
        /// </summary>
        public static readonly RelativePath Invalid = default(RelativePath);

        public static bool TryCreate(out RelativePath result)
        {
            result = default;
            return true;
        }
    }

    public static class Goo<T>
    {
        static Goo()
        {
            
            Goo<T>.Y = 3; // this is ok!
            Goo<int>.X = 1; // but this is not ok, actually!
        }

        public static readonly int X;
        public static int Y { get; }
    }

    static class Program
    {
        public enum Color { Red, Blue, Green }
        public static void M<T>(T t) { }

        public static void Foo()
        {
            // Operator '-' cannot be applied to operands of type 'int' and 'Color'
            M(1 - Color.Red);
        }

        public static bool Check(object o) 
            // The second operand of an 'is' or 'as' operator may not be static type 'Program'
            => o is Program;
        
        public readonly struct EnumerateDirectoryResult //: IEquatable<EnumerateDirectoryResult>
        {
        }

        public static string GetNativeErrorMessage(this EnumerateDirectoryResult result)
        {
            Contract.Requires(result != null);
        }

            public static void FooBar(Guid g)
        {
            Debug.Assert(g != null);
            
            StructWithValue g2 = default; // Guid.NewGuid();
            Debug.Assert(g2 != null);
        }
        public static void DelegateCreationByRef()
        {
            // Cannot use 'DelegateCreationByRef' as a ref or out value because it is a 'method group'
            Action a = new Action(ref DelegateCreationByRef);

            // '<null>' is not a reference type as required by the lock statement
            lock(null) { }

            // Possibly mistaken empty statement
            lock (a) ;

            for (int i = 0; i < 10; i++) ;

            var v = new { x = default(StructWithReference)};
            // The result of expression is always 'true' since a value of type 'int' is never equal to 'null'
            Contract.Assert(v.x != null);
            //if (x == null)
            //{ }
            
            // Comparing with null of type 'int?' always produces 'false'
            bool b = (int?)null < 42;

            StructWithReference r1;
            var r2 = r1; // this is ok all the time it seems like

            //StructWithValue v1;
            //var v2 = v1;
        }

        public struct StructWithReference
        {
            //string PrivateData;
        }
        public readonly struct StructWithValue
        {
            readonly int PrivateData;
        }


        public static void GenericsAndLock<T>(T item) // where T: class
        {
            // 'T' is not a reference type as required by the lock statement
            lock(item)
            { }
        }

        public static void UseOfUnassignedLocal(bool arg, Dictionary<int, int> d)
        {
            //if (d.Count > 0)
            //{

            //}
            //if ((arg && d != null) || d.TryGetValue(42, out var result))
            //if (arg || RelativePath.TryCreate(out var result))
            if (arg || tryCreate(out var result))
            {
                //Console.WriteLine(result.ToString());
            }

            static bool tryCreate(out RelativePath v)
            {
                v = default;
                return true;
            }
        }

        static void Main(string[] args)
        {
            UseOfUnassignedLocal(true, null);
            Console.WriteLine("Hello World!");
        }
    }
}
