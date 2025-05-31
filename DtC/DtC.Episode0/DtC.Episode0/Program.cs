using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

namespace DtC.Episode0
{
    // The struct is blittable, because there is no gaps between
    // the fields
    public struct Blittable(long L, int N, short S1, short S2)
    {
        public long L = L;
        public int N = N;
        public short S1 = S1;
        public short S2 = S2;
    }

    /// <summary>
    /// The struct is non-blittable, because the first "field" is a reference type.
    /// </summary>
    public struct NonBlittable(string Str, int N)
    {
        public string Str = Str;
        public int N = N;

        public static IEnumerable<NonBlittable> Generate(int count)
        {
            // The hash code of all the instances is going to be the same!
            return Enumerable.Range(1, count)
                .Select(n => new NonBlittable(Str: string.Empty, N: n));
        }
    }

    /// <summary>
    /// The struct is non-blittable, but records properly override Equals and GetHashCode
    /// </summary>
    public record struct NonBlittableRecord(string Str, int N);

    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.Method)]
    public class CacheLookupBenchmark
    {
        private Blittable _blittable = new Blittable(L: 42, N: -1, S1: 1, S2: 2);
        private NonBlittable _nonBlittable = new NonBlittable(Str: "42", N: -1);
        private NonBlittableRecord _nonBlittableRecord = new NonBlittableRecord(Str: "42", N: -1);
        
        private ConcurrentDictionary<Blittable, int> _blittables;
        private ConcurrentDictionary<NonBlittable, int> _nonBlittables;
        private ConcurrentDictionary<NonBlittableRecord, int> _nonBlittablerecords;

        [Params(1, 10, 100)]
        public int Count { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _blittables = new (
                Enumerable
                .Range(0, Count)
                .Select(n => KeyValuePair.Create(new Blittable(L: 42, n, 1, 2), 42)));

            _nonBlittables = new (
                Enumerable
                .Range(0, Count)
                .Select(n => KeyValuePair.Create(new NonBlittable(Str: "42", n), 42)));
            
            _nonBlittablerecords = new (
                Enumerable
                .Range(0, 10)
                .Select(n => KeyValuePair.Create(new NonBlittableRecord(L: 42, n, 1), 42)));
        }

        [Benchmark]
        public bool Blittable_Contains()
            => _blittables.ContainsKey(_blittable);
        
        [Benchmark]
        public bool NonBlittable_Contains()
            => _nonBlittables.ContainsKey(_nonBlittable);

        [Benchmark]
        public bool NonBlittableRecord_Contains() 
            => _nonBlittablerecords.ContainsKey(_nonBlittableRecord);
    }


    public class FakeService
    {
        public struct RequestKey
        {
            public required string Operation { get; init; }
            public int Payload { get; init; }
        }

        private readonly ConcurrentDictionary<RequestKey, Response> _cache = new();

        // This is the operation we're optimizing
        public async Task<Response> ProcessRequest(Request request)
        {
            var requestKey = ToRequestKey(request);
            
            // Checking the results from in the cache
            if (_cache.TryGetValue(requestKey, out var result))
            {
                return result;
            }

            // Performing the operation
            result = await ProcessRequestCore(request);
            
            // And updating the cache
            _cache.TryAdd(requestKey, result);
            return result;
        }

        private async Task<Response> ProcessRequestCore(Request request)
        {
            return new Response();
        }

        private static RequestKey ToRequestKey(Request request)
        {
            return new RequestKey() {Operation = nameof(ProcessRequest), Payload = Guid.NewGuid().GetHashCode()};
        }

        public class Response
        {
        }

        public class Request
        {
        }
    }

    internal class Program
    {
        

        

        static void Main(string[] args)
        {
            BenchmarkRunner.Run<CacheLookupBenchmark>();
        }
    }

    
}
