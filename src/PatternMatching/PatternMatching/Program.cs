using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace PatternMatching
{
    class Program
    {
        public static void WithNullPropagation(IEnumerable<string> s)
        {
            if (s?.FirstOrDefault(str => str.Length >= 10)?.Length is var n && n > 42)
            {
                Console.WriteLine($"Matched: {n}");
            }
            else
            {
                Console.WriteLine("Miss!");
            }
        }

        //public class Nested
        //{
        //    public (string s, string s2)? V = null;
        //}

        //public static void WithNullPropagation(Nested n)
        //{
        //    if (n?.V?.s?.Length is var length)
        //    //if (n?.Length is int n)
        //    //if (s?.FirstOrDefault(str => str.Length > 10)?.Length is int n)
        //    {
        //        //Console.WriteLine("Got it");
        //        Console.WriteLine($"Matched: {length}");
        //    }
        //    else
        //    {
        //        Console.WriteLine("Miss!");
        //    }
        //}
        enum MyEnum
        {
            v1
        }

        static void Main(string[] args)
        {
            //object o = 42;
            //bool b = o is MyEnum.v1;
            //b = o is (short) 4;
            //Console.WriteLine(b);
            BenchmarkRunner.Run<Benchmarks>();
            //WithNullPropagation(Enumerable.Empty<string>());
            //WithNullPropagation(new string[]{"123456789", "1234567890"});
        }
    }
}
