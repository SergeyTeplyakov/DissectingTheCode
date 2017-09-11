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

    
    public class Benchmarks
    {
private const int ArraySize = 1;
private object[] _objects = new object[ArraySize];
private ObjectWrapper[] _wrappers = new ObjectWrapper[ArraySize];
private object _objectInstance = new object();
private ObjectWrapper _wrapperInstanace = new ObjectWrapper(new object());

[Benchmark]
public void WithCheck()
{
    _objects[0] = _objectInstance;
            //for (int i = 0; i < _objects.Length; i++)
            //{
            //    _objects[i] = _objectInstance;
            //}
        }

[Benchmark]
public void WithoutCheck()
{
    _wrappers[0] = _wrapperInstanace;
            //for (int i = 0; i < _objects.Length; i++)
            //{
            //    _wrappers[i] = _wrapperInstanace;
            //}
        }
}
}