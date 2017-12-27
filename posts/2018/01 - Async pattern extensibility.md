# Extending the async methods in C#

In the [previous blog post](blogs.msdn.microsoft.com/seteplia/2017/11/30/dissecting-the-async-methods-in-c/) we discussed how the C# compiler transforms asynchronous methods. In this post we'll focus on extensibility points that the C# compiler provides for customizing the behavior of async methods.

There are 3 ways how you can control the async machinery:
1. Provide your own async method builder in the `System.Runtime.CompilerServices` namespace.
2. Use custom task awaiters.
3. Define your own task-like types.

## Custom types fromm `System.Runtime.CompilerServices` namespace

As we know from the previous post, the C# compiler transforms async methods into a generated state machine that relies on some predefined types. But the C# compiler does not expect that these well-known types come from a specific assembly. For instance, you can provide your own implementation of [`AsyncVoidMethodBuilder`](http://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/AsyncMethodBuilder.cs,b07562c618ee846c) and the C# compiler will "bind" async machinery to your custom type.

This is a good way to explore what the underlying machinery is and to see what's happening at runtime:

```csharp
namespace System.Runtime.CompilerServices
{
    // AsyncVoidMethodBuilder.cs in your project
    public class AsyncVoidMethodBuilder
    {
        public AsyncVoidMethodBuilder() 
            => Console.WriteLine(".ctor");

        public static AsyncVoidMethodBuilder Create() 
            =>  new AsyncVoidMethodBuilder();

        public void SetResult() => Console.WriteLine("SetResult");

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            Console.WriteLine("Start");
            stateMachine.MoveNext();
        }

        // AwaitOnCompleted, AwaitUnsafeOnCompleted, SetException 
        // and SetStateMachine are empty
    }
}
```

Now, every async method in your project will use the custom version of `AsyncVoidMethodBuilder`. We can test this with a simple async method:

```csharp
[Test]
public void RunAsyncVoid()
{
    Console.WriteLine("Before VoidAsync");
    VoidAsync();
    Console.WriteLine("After VoidAsync");

    async void VoidAsync() { }
}
```

The output of this method is:

```
Before VoidAsync
.ctor
Start
SetResult
After VoidAsync
```

You can implement `UnsafeAwaitOnComplete` method to test the behavior of an async method with `await` clauses that returns non-completed task as well. The full example can be found at [github](https://github.com/SergeyTeplyakov/EduAsync/blob/master/src/01_AsyncVoidBuilder/AsyncVoidSample.cs).

To change the behavior for `async Task` and `async Task<T>` methods you may provide your own version of [`AsyncTaskMethodBuilder`](http://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/AsyncMethodBuilder.cs,c983aa3f7c40052f) and [`AsyncTaskMethodBuilder<T>`](http://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/AsyncMethodBuilder.cs,5916df9e324fc0a1)

The full example of these types can be found at my github project called [EduAsync](https://github.com/SergeyTeplyakov/EduAsync) (*) in [AsyncTaskBuilder.cs](https://github.com/SergeyTeplyakov/EduAsync/blob/master/src/02_AsyncTaskBuilder/AsyncTaskMethodBuilder.cs) and [AsyncTaskMethodBuilderOfT.cs](https://github.com/SergeyTeplyakov/EduAsync/blob/master/src/03_AsyncTaskBuilderOfT/AsyncTaskMethodBuilderOfT.cs) respectively.

(*) Thanks [Jon Skeet](https://codeblog.jonskeet.uk/category/eduasync/) for inspiration for this project. This is a really good way to learn async machinery deeper.

## Custom awaiters

The previous example is "hacky" and not suitable for production. We can learn the async machinery that way, but you definitely don't want to see such a code in your codebase. The C# language authors built-in a proper extensibility points into the compiler that allow to "await" different types in async methods.

In order for a type to be "awaitable" (i.e. to be valid in context of an `await` expression) the type should follow a special pattern:

* Compiler should be able to find an instance or an extension method called `GetAwaiter`.
The return type of this method should follow certain requirements:
* The type should implement [`INotifyCompletion`](http://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/INotifyCompletion.cs,23) interface.
* The type should have `bool IsCompleted {get;}` property and `T GetResult()` method.

This means that we can easily make `Lazy<T>` awaitable:

```csharp
public struct LazyAwaiter<T> : INotifyCompletion
{
    private readonly Lazy<T> _lazy;

    public LazyAwaiter(Lazy<T> lazy) => _lazy = lazy;

    public T GetResult() => _lazy.Value;

    public bool IsCompleted => true;

    public void OnCompleted(Action continuation) {}
}

public static class LazyAwaiterExtensions
{
    public static LazyAwaiter<T> GetAwaiter<T>(this Lazy<T> lazy)
    {
        return new LazyAwaiter<T>(lazy);
    }
}

public static async Task Foo()
{
    var lazy = new Lazy<int>(() => 42);
    var result = await lazy;
    Console.WriteLine(result);
}
```

The example could looked too contrived but this extensibility point is actually very helpful and is used in the wild. For instance, [Reactive Extensions for .NET](https://github.com/Reactive-Extensions/Rx.NET) provides a [custom awaiter](https://github.com/Reactive-Extensions/Rx.NET/blob/fa1629a1e12a8fc21c95aeff7863425c2485defd/Rx.NET/Source/src/System.Reactive/Linq/Observable.Awaiter.cs#L21) for awaiting `IObservable<T>` instances in async methods. The BCL itself has [`YieldAwaitable`](http://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/YieldAwaitable.cs,45) used by [`Task.Yield`](http://referencesource.microsoft.com/#mscorlib/system/threading/Tasks/Task.cs,3031) and [`HopToThreadPoolAwaitable`](http://referencesource.microsoft.com/#mscorlib/system/security/cryptography/cryptostream.cs,328):

```csharp
public struct HopToThreadPoolAwaitable : INotifyCompletion
{
    public HopToThreadPoolAwaitable GetAwaiter() => this;
    public bool IsCompleted => false;

    public void OnCompleted(Action continuation) => Task.Run(continuation);
    public void GetResult() { }
}
```

The following unit test demonstrate the last awaiter in action:

```csharp
[Test]
public async Task Test()
{
    var testThreadId = Thread.CurrentThread.ManagedThreadId;
    await Sample();

    async Task Sample()
    {
        Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, testThreadId);

        await default(HopToThreadPoolAwaitable);
        Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, testThreadId);
    }
}
```

The first part of any "async" method (before the first `await` statement) runs synchronously. In most cases this is fine and is desirable for eager argument validation, but some times we would like to make sure that the method body would not block the caller's thread. `HopToThreadPoolAwaitable` makes sure that the rest of the method is executed in the thread pool thread rather than in the caller's thread.

## Task-like types

Custom awaiters were available from the very first version of the compiler that supported async/await (i.e. from C# 5). This extensibility point is very useful, but limited because all the async methods should've returned `void`, `Task` or `Task<T>`. Starting from C# 7.2 the compiler support task-like types.

[Task-like type](https://github.com/dotnet/roslyn/blob/master/docs/features/task-types.md) is a class or a struct with an associated *builder type* identified by `AsyncMethodBuilderAttribute` (**). To make the task-like type useful it should be *awaitable* in a way we describe in the previous section. Basically, task-like types combine the first two extensibility points described before by making the first way officially supported one.

(**) Today you have to define this attribute yourself. The example can be found at [my github repo](https://github.com/SergeyTeplyakov/EduAsync/blob/master/src/07_CustomTaskLikeTypes/AsyncMethodBuilder.cs#L9).

Here is a simple example of a custom task-like type defined as a struct:

```csharp

[System.Runtime.CompilerServices.AsyncMethodBuilder(typeof(TaskLikeMethodBuilder))]
public struct TaskLike
{
    public TaskLikeAwaiter GetAwaiter() => default(TaskLikeAwaiter);
}

public sealed class TaskLikeMethodBuilder
{
    public TaskLikeMethodBuilder()
        => Console.WriteLine(".ctor");

    public static TaskLikeMethodBuilder Create()
        => new TaskLikeMethodBuilder();

    public void SetResult() => Console.WriteLine("SetResult");

    public void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    {
        Console.WriteLine("Start");
        stateMachine.MoveNext();
    }

    // AwaitOnCompleted, AwaitUnsafeOnCompleted, SetException 
    // and SetStateMachine are empty
}

public struct TaskLikeAwaiter : INotifyCompletion
{
    public void GetResult() { }

    public bool IsCompleted => true;

    public void OnCompleted(Action continuation) { }
}
```

And now we can define a method that returns `TaskLike` type and even use different task-like types in the method body:

```csharp
public async TaskLike FooAsync()
{
    await Task.Yield();
    await default(TaskLike);
}
```

The main reason for having task-like types is an ability to reduce the overhead of an async operations. Every async operation that returns `Task`/`Task<T>` allocates at least one object in the managed heap - the task itself. This is perfectly fine for a vast majority of applications especially when they deal with coarse-grained async operations. But this is not the case for infrastructure-level code that could span thousands of small tasks per second. For such kind of scenarios reducing one allocation per call could reasonably increase performance.

I don't think that everyone will use these extensibility mechanisms but you should be at least aware of them.

## Async pattern extensibility 101
* The C# compiler provides various ways for extending async methods.
* You can change the behavior for existing Task-based async methods by providing your own version of `AsyncTaskMethodBuilder` type. 
* You can make a type "awaitable" by implementing "awaitable pattern". 
* Starting from C# 7 you can build your own task-like types.

## Additional resources
* [Dissecting the async methods in C#](https://blogs.msdn.microsoft.com/seteplia/2017/11/30/dissecting-the-async-methods-in-c/)
* [EduAsync repo](https://github.com/SergeyTeplyakov/EduAsync/) on github.
* [Task-like types](https://github.com/dotnet/roslyn/blob/master/docs/features/task-types.md)

Next time we'll discuss the perf characteristics of async methods and will see how the newest task-like value type called `System.ValueTask` affects performance.

