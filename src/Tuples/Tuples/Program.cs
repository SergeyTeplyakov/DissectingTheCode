using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace Tuples
{
    /*
                  Method |      Mean |     Error |    StdDev |
------------------------ |----------:|----------:|----------:|
      DelegateInvocation | 1.5041 ns | 0.0060 ns | 0.0053 ns |
 LocalFunctionInvocation | 0.9298 ns | 0.0063 ns | 0.0052 ns |
     * */
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<DelegateStorageRetrieve>();
        }
    }
}
