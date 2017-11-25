using System;
using System.Collections.Concurrent;

namespace Tuples
{
    public static class ConcurrentDictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue, TState>(
            this ConcurrentDictionary<TKey, TValue> d,
            
            TKey key,
            Func<TKey, TState, TValue> valueFactory, TState state)
        {
            throw new NotImplementedException();
        }
    }

    class Sample
    {
        private readonly ConcurrentDictionary<int, string> _cd = new ConcurrentDictionary<int, string>();

        public string GetValue(int value, bool extraState)
        {
            return _cd.GetOrAdd(
                value,
                (key, tpl) =>
                {
                    return tpl.@this.ComputeValue(key, tpl.extraState);
                },
                (extraState, @this: this));
        }

        private string ComputeValue(int value, bool extraState) => value.ToString();

        public void FooBar(int x)
        {
            // (Sample, int x)
            var tpl = (this, x);
            
        }
    }
}