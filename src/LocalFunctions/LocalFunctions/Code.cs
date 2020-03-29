using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LocalFunctions
{
    public class File
    {
        public static Task<string> ReadAllTextAsync(string fileName)
        {
            throw new NotImplementedException();
        }
    }
    /*
                  Method |      Mean |     Error |    StdDev | Allocated |
------------------------ |----------:|----------:|----------:|----------:|
      DelegateInvocation | 2.3035 ns | 0.0847 ns | 0.0869 ns |       0 B |
 LocalFunctionInvocation | 0.0142 ns | 0.0176 ns | 0.0137 ns |       0 B |
     * */
    public class Code
    {
        internal sealed class c__DisplayClass0_0
        {
            public int arg;
            public int local;

            internal int ImplicitAllocation_b__0()
                => this.arg;

            internal int ImplicitAllocation_g__Local1()
                => this.local;
        }

        public int ImplicitAllocation(int arg)
        {
            var c__DisplayClass0_ = new c__DisplayClass0_0 { arg = arg };
            if (c__DisplayClass0_.arg == int.MaxValue)
            {
                var func = new Func<int>(c__DisplayClass0_.ImplicitAllocation_b__0);
            }
            c__DisplayClass0_.local = 42;
            return c__DisplayClass0_.ImplicitAllocation_g__Local1();
        }
    }
}