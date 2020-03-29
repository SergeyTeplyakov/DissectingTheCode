using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Ex;

namespace ValueTasks.CustomAwaiter
{
public struct LazyAwaiter<T> : INotifyCompletion
{
    private readonly Lazy<T> _lazy;

    public LazyAwaiter(Lazy<T> lazy) => _lazy = lazy;

    public T GetResult() => _lazy.Value;

    public bool IsCompleted => true;

    public void OnCompleted(Action continuation) {}

    public LazyAwaiter<T> GetAwaiter() => this;
}

public static class LazyAwaiterExtensions
{
    public static LazyAwaiter<T> GetAwaiter<T>(this Lazy<T> lazy)
    {
        return new LazyAwaiter<T>(lazy);
    }
}

    public static class Example
    {
//public static async ValueTask<object> Foo()
//{
//    var lazy = new Lazy<int>(() => 42);
//    var result = await lazy;
//    Console.WriteLine(result);
//    var result2 = await lazy;
//    Console.WriteLine(result2);
//    return null;
//}
    }
}