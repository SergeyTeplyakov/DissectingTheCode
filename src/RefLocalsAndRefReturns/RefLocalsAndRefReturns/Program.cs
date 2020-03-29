using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RefLocalsAndRefReturns
{
    struct CustomStruct
    {
        
    }

    class CustomList<T>
    {
        private readonly T[] m_data = new T[0];
        public ref T this[int v]
        {
            get { return ref m_data[v]; }
        }
    }
    class Program
    {
        //[return: TupleElementNames(new string[] {
        //    "x",
        //    "y"
        //})]
        public virtual ValueTuple<int, int> Foo()
        {
            return new ValueTuple<int, int>(1, 2);
        }

        private static CustomStruct cs2;
        public static ref CustomStruct ByRef(CustomStruct[] cs)
        {
            //ref var x = cs[0];
            return ref cs2;
        }


        private static readonly (int x, int y) s_x = (1, 2);

        static void Main(string[] args)
        {
            List<(int x, int y)> li = new List<(int x, int y)>{(1,2)};

            var tpl = Tuple.Create(1, 2);
            var (left, right) = tpl;

            //IncrementX(li[0]);

            return;

            ConcurrentDictionary<int, int> cd = null;
            
            ref var br = ref ByRef(null);
            CustomStruct[] cs = new CustomStruct[0];
            ref CustomStruct br2 = ref cs2;
        }
        public static void IncrementX(ref (int x, int y) tpl)
        {
            tpl.x++;
        }
    }

    public static class TupleExtensions
    {
        
    }

    public static class ConcurrentDictionaryEx
    {
        public static TValue GetOrAdd<TKey, TValue, TArg>(this ConcurrentDictionary<TKey, TValue> cd, TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument)
        {
            //if (key == null) ThrowKeyNullException();
            //if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));

            if (cd.TryGetValue(key, out var value))
            {
                return value;
            }

            return default(TValue);
            //return cd.TryAdd(key, valueFactory(key, factoryArgument));
            //int hashcode = _comparer.GetHashCode(key);

            //TValue resultingValue;
            //if (!TryGetValueInternal(key, hashcode, out resultingValue))
            //{
            //    TryAddInternal(key, hashcode, valueFactory(key, factoryArgument), false, true, out resultingValue);
            //}
            //return resultingValue;
        }
    }
}
