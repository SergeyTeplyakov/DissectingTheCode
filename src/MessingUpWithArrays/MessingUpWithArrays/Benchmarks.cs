using System.Diagnostics;
using BenchmarkDotNet.Attributes;

namespace MessingUpWithArrays
{
public struct ObjectWrapper
{
    public readonly object Instance;
    public ObjectWrapper(object instance)
    {
        Instance = instance;
    }
}
internal class ObjectPool<T> where T : class
{
    [DebuggerDisplay("{Value,nq}")]
    private struct Element
    {
        internal T Value;
    }

    // Storage for the pool objects. The first item is stored in a dedicated field because we
    // expect to be able to satisfy most requests from it.
    private T _firstItem;
    private readonly Element[] _items;

    // other members ommitted for brievity
}


    public class Benchmarks
    {
private const int ArraySize = 100_000;
private object[] _objects = new object[ArraySize];
private ObjectWrapper[] _wrappers = new ObjectWrapper[ArraySize];
private object _objectInstance = new object();
private ObjectWrapper _wrapperInstanace = new ObjectWrapper(new object());

[Benchmark]
public void WithCheck()
{
    for (int i = 0; i < _objects.Length; i++)
    {
        _objects[i] = _objectInstance;
    }
}

[Benchmark]
public void WithoutCheck()
{
    for (int i = 0; i < _objects.Length; i++)
    {
        _wrappers[i] = _wrapperInstanace;
    }
}
    }
}