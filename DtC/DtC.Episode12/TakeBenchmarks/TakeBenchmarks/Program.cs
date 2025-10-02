using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using Microsoft.VSDiagnostics;

namespace TakeBenchmarks
{
    public record Product(string Id, string Name);

    [MemoryDiagnoser]
    public class SkipAndTakeBenchmarks
    {
        private static List<Product> GetProducts()
        {
            return Enumerable.Range(0, 40).Select(n => new Product($"Id_{n}", $"Name_{n}")).ToList();
        }

        [Benchmark(Baseline = true)]
        public List<Product> SkipTakeMethod()
        {
            var productList = GetProducts();
            productList = productList.Skip(1).Take(3).ToList();
            return productList;
        }
        
        [Benchmark]
        public List<Product> TakeRange()
        {
            var productList = GetProducts();
            productList = productList.Take(1..4).ToList();
            return productList;
        }
    }
    
    [MemoryDiagnoser]
    [DotNetObjectAllocDiagnoser]
    [DotNetObjectAllocJobConfiguration()]
    //[ShortRunJob]
    public class SkipAndTakeBenchmarksFixed
    {
        private readonly List<Product> _products = GetProducts();

        private static List<Product> GetProducts()
        {
            return Enumerable.Range(0, 40).Select(n => new Product($"Id_{n}", $"Name_{n}")).ToList();
        }

        [Benchmark(Baseline = true)]
        public int SkipTakeMethod()
        {
            var products = _products.Skip(1).Take(3);
            return products.Count();
        }
        
        [Benchmark]
        public int TakeRange()
        {
            var products = _products.Take(1..4);
            return products.Count();
        }

        //[Benchmark]
        //public int SkipFastTakeFast()
        //{
        //    var products = _products.SkipFast(1).TakeFast(3);
        //    return products.Count();
        //}
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<SkipAndTakeBenchmarksFixed>();
        }
    }
}
