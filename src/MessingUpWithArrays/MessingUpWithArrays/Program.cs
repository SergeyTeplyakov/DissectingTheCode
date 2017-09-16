using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace MessingUpWithArrays
{
    class Program
    {
        static T GetValue<T>(T[] a)
        {
            var o = a[0];
            o.ToString();
            return o;
        }

        static void IlStuff()
        {
            int[] ints = {1};
            ints[0] = 2;

            DateTime[] dates = {DateTime.Now};
            dates[0] = new DateTime();

            object[] o = {null};
            o[0] = null;
        }

        static void Main(string[] args)
        {
string[] strings = new[] { "1", "2" };
object[] objects = strings;

objects[0] = 42; //runtime error
            //var ip = new IpAddress(255, 255, 255, 0);

            //// FFFFFF00
            //Console.WriteLine(ip.Value.ToString("X"));
            //// 255.255.255.0
            //Console.WriteLine($"{ip.Byte0}.{ip.Byte1}.{ip.Byte2}.{ip.Byte3}");

            BenchmarkRunner.Run<Benchmarks>();
            return;

            //string s1 = "1";
            //string s2 = "2";
            //string s3 = "3";
            //string[] strings = { s1, s2, s3 };
            //string[] strings2 = { s1, s2 };
            ////object o = GetValue<object>(strings);
            ////Console.WriteLine(o);
            ////Console.ReadLine();

            ////object[] objs = strings;
            ////objs[0] = new object();

            //StringBuilder[] builders = { new StringBuilder(), new StringBuilder() };

            //int[] numbers = {1, 2};
            //Debugger.Break();
            ////object[] obs = strings;
            //////Array a = obs;
            //////a.SetValue(1, 0);
            //////obs[0] = 1;

            ////var s1Addr = GetAddress(s1);
            ////var s2Addr = GetAddress(s2);
            ////var s3Addr = GetAddress(s3);
            ////var stringsAddress = GetAddress(strings);

            ////var hacker = new ArrayHacker(strings);
            ////Console.WriteLine($"Length: {hacker.ArrayInternals.Length}.");
            ////Console.WriteLine($"Type: {hacker.ArrayInternals.Type.Equals(typeof(string).TypeHandle)}");
            ////Console.WriteLine($"[{hacker.ArrayInternals.Eement0}, {hacker.ArrayInternals.Element1}]");

            //var ae = new ArrayExplorer();
            //ae.Array = new string[] { "first string", "second string", "434" };

            //if (ae.Layout.Element0 == "second string")
            //{
            //    Console.WriteLine("New array layout");
            //}
            //else
            //{
            //    Console.WriteLine("Old array layout!");
            //}

            //Console.WriteLine("Length: " + ae.Layout.Length);
            //Console.WriteLine("Type: " + ae.Layout.Type);
            //Console.WriteLine("Element0: " + ae.Layout.Element0);
            //Console.WriteLine("Element1: " + ae.Layout.Element1);
        }

        //unsafe private static IntPtr GetAddress(object o)
        //{
        //    TypedReference tr = __makeref(o);
        //    IntPtr ptr = **(IntPtr**)(&tr);
        //    return ptr;
        //}
    }
}
