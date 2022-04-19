using InterpolatedStrings;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpolatedStringImprovementsWithDotNetFramework
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int n = 0;

            // Contract is not violated! No messages will be constructed!
            string str = "foo bar";
            Contract.Requires(true, $"No side effects! n == {++n}, {str.AsSpan().Trim()}");

            Console.WriteLine($"n == {n}"); // n == 0

            try { Contract.Requires(n == 42, $"Side effect! n == {++n}"); } catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine($"n == {n}"); // n == 1

            try { Contract.Requires(false, $"Side effect! n == {await FooAsync()}"); } catch (Exception e) { Console.WriteLine(e); }

            
        }

        static Task<int> FooAsync() => Task.FromResult(42);
    }
}
