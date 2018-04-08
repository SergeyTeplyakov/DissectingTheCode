using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RefLocals
{
public class NaiveImmutableList<T>
{
    private readonly int _length;
    private readonly T[] _data;
    public NaiveImmutableList(params T[] data) 
        => (_data, _length) = (data, data.Length);

    public ref readonly T this[int idx]
    {
        get
        {
            if ((uint) idx >= (uint) _length)
                ThrowIndexOutOfRangeException();

            return ref _data[idx];
        }
    }

    private static void ThrowIndexOutOfRangeException() =>
        throw new IndexOutOfRangeException();
}
}