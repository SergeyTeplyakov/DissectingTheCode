

The language authors had decided to not rely on `TaskAwaiter<T>.OnComplete` that flow the execution context manually, but captured it in `AsyncMethodBuilder<T>` and restore it in `MoveNextRunner.Run` method.

I'm not sure why this decision was made, but today, the execution context is captured manually via `MoveNextRunner` and 
but instead of using `TaskAwaiter<T>.OnComplete` that will flow the execution context automatically, the generated state machine relies on `AsyncMethodBuilder` to capture execution context manually.


There is a set of methods in the BCL that explicitly will not flow the execution context
But not all methods will propagate an execution context: 

In both cases we'll see that the value, associated with `AsyncLocal` instance is equals to `42`.


We 
[`AsyncLocal<T>`]() is very similar to [`ThreadLocal<T>`]()

example with asynclocal and task awaiter.

When the delegate provided to `Task.Run` is invoked

All methods in the .NET Framework that fork asynchronous work capture and restore `ExecutionContext`.  For example, when you use Task.Run, the call to Run captures the ExecutionContext from the invoking thread, storing that ExecutionContext instance into the Task object. When the delegate provided to Task.Run is later invoked as part of that Task’s execution, it’s done so via ExecutionContext.Run using the stored context.  This is true for Task.Run, for ThreadPool.QueueUserWorkItem, for Delegate.BeginInvoke, for Stream.BeginRead, for DispatcherSynchronizationContext.Post, and for any other async API you can think of.  All of them capture the ExecutionContext, store it, and then use the stored context later on during the invocation of some code.

 multiple threads that represents one logical flow of control. 
in one thread and restore it in another thread 

ExecutionContext is really just a state bag that can be used to capture all of this state from one thread and then restore it onto another thread while the logical flow of control continues.

Execution context play the same role for asynchronous methods as thread-local storage plays for synchronous ones: it represents an bag of arbitrary associated with a logical "asynchronous" flow of execution.



Many methods in the BCL flow this information naturally.

NOTE: Execution Context is very different from Synchronization Context. Execution Context is just a bag of data associated with some logical operation that can span multiple threads. Synchronization Context on the other hand represent an execution environment like Windows Forms UI thread. When the continuation is executed under a given "execution context" it can get manipulate some ambient data. When the continuation is executed under a given "synchronization context" it can touch, for instance, UI elements because it is executed in the UI thread.

we can store "ambient" information in thread-local storage

ExecutionContext is all about “ambient” information, meaning that it stores data relevant to the current environment or “context” in which you’re running.  In many systems, such ambient information is maintained in thread-local storage (TLS), such as in a ThreadStatic field or in a ThreadLocal<T>.  In a synchronous world, such thread-local information is sufficient: everything’s happening on that one thread, and thus regardless of what stack frame you’re in on that thread, what function is being executed, and so forth, all code running on that thread can see and be influenced by data specific to that thread.  For example, one of the contexts contained by ExecutionContext is SecurityContext, which maintains information like the current “principal” and information about code access security (CAS) denies and permits.  Such information can be associated with the current thread, such that if one stack frame denies access to a certain permission and then calls into another method, that called method will still be subject to the denial set on the thread: when it tries to do something that needs that permission, the CLR will check the current thread’s denials to see if the operation is allowed, and it’ll find the data put there by the caller.

describe, add a links to Toub's article, and another one. Give an example.
can't just create an `Action` instance from the `IAsyncStateMachine.MoveNext` method because

If the awaited task is not completed, the state machine calls .

 The builder then needs to construct an `Action` instance to pass it to the `awaiter`. This action should be calling `IAsyncStateMachine.MoveNext` method, but this is not happening directly.

Every continuation should carry some special ambient information across 

ExecutionContext is really just a state bag that can be used to capture all of this state from one thread and then restore it onto another thread while the logical flow of control continues. 

in the correct synchronization context

In this

Async_sequence_state_machine

The state machine is a struct in release mode and to avoid redundant copies of potentially big struct, the stateme
is passed by reference in this case, because I'd get the decompiled version of this code from the release bits, and

State machine:
GetStockPriceForAsync->StateMachine: Create
GetStockPriceForAsync->AsyncTaskMethodBuilder:Start(ref StateMachine)
AsyncTaskMethodBuilder-> StateMachine: MoveNext
StateMachine->StockPrice: InitializeMapIfNeededAsync
StateMachine->TaskAwaiter:IsCompplete()
StateMachine->AsyncTaskMethodBuilder:AwaitUnsafeOnCompleted
AsyncTaskMethodBuilder->StateMachine:GetCompletionAction:action
AsyncTaskMethodBuilder->TaskAwaiter:UnsafeOnCompleted(action)
TaskAwaiter->StateMachine:MoveNext

## Performance implications

The overall machinery has non-negligible effect on performance. The language authors tried to optimize as much as possible, but state-machine-based method is way more complicated that the regular synchronous method. Let's compare what the difference is.


### Awaitable pattern
This should go to the next blogpost.

In C# 5 and 6 method marked with `async` keyword can return `void`, `Task` or `Task<T>`. The set was fixed. But the second aspect of "async" methods -- "awaitable part" -- was extensible: the right hand side of `await` clause could be anything that follows a specific pattern. The compiler should be able to find `GetAwaiter` method (instance method or an extension method) that returns something that has the property `bool IsCompleted {get;}`, the method `T GetResult()` and implement [`INotifyCompletion`](http://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/INotifyCompletion.cs,0670f9b4b67cecd6) interface.

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

This pattern-based approach allows to compose asynchronous methods not only based on tasks, but on other primitives, like `IObservable<T>`, `IAsyncEnumerator<T>` etc.

## Size of the generated struct
We can use ObjectLayoutInspector to see the layout of the generated struct.

```
Type layout for '<GetStockPriceFor>d__1'
Size: 56 bytes (x64)
|============================================================|
|   0-7: StockPrices <>4__this (8 bytes)                     |
|------------------------------------------------------------|
|  8-15: String companyId (8 bytes)                          |
|------------------------------------------------------------|
| 16-19: Int32 <>1__state (4 bytes)                          |
|------------------------------------------------------------|
| 24-47: AsyncTaskMethodBuilder`1 <>t__builder (24 bytes)    |
| |========================================================| |
| |   0-7: Task`1 m_task (8 bytes)                         | |
| |--------------------------------------------------------| |
| |  8-23: AsyncMethodBuilderCore m_coreState (16 bytes)   | |
| | |====================================================| | |
| | |   0-7: IAsyncStateMachine m_stateMachine (8 bytes) | | |
| | |----------------------------------------------------| | |
| | |  8-15: Action m_defaultContextAction (8 bytes)     | | |
| | |====================================================| | |
| |========================================================| |
|------------------------------------------------------------|
| 48-55: TaskAwaiter <>u__1 (8 bytes)                        |
| |==============================|                           |
| |   0-7: Task m_task (8 bytes) |                           |
| |==============================|                           |
|============================================================|
```

To support this type of scenario, the compiler generate a special state machine type that relies on two helper types to achieve this goal.


That made possible 


## Asynchronous programming models in .NET

The .Net Framework and C# language always had facilities for building asynchronous applications. First, it was the "Asynchronous Programming Model". If you wanted to make an asynchronous counterpart for a method `Result Foo(Arg a)` you had to define two methods: `IAsyncResult BeginFoo(Arg a)` and `Result EndFoo()`.

The implementation of the async pattern was tedious and usage pattern was far from perfect as well. Asynchronous methods were not composable, common language constructs like `using`-blocks were unusable and

One of the changes in C# 7 looks very insignificant from the first glance but is very important.

To understand why the changes that were made in the latest version of the C# compiler are so important we have too go back in history.


There were several key milestones in the .NET World that make
Today we see a huge push towards high performance, low allocation solution in 

1. Tasks were introduced in .NET 4.0 and that was good.
2. When async/await was added to the language the feature was made extensible:


The very first version of .NET Framework supported what is known as "Asynchronous Programming Model". The idea of APM is simple: for synchronous method `Result Foo(T arg)` you should create two methods


Here is a little bit of history about task-based and asynchronous programming in C#. `Task` and `Task<T>` classes where introduced with .NET Framework 4.0 as a powerful library feature. I do believe this was the major step from old-school thread-based concurrency towards way less complicated task-based concurrency. At that time there was no language support that helps composing different operations that return tasks into higher level logic.



# TODO:
1. Measure ValueTask<int> async with custom awaiter vs. regular method to see the overhead of async state machine.

# Outline
Original method
Async state machine
Coordination (with uml diagram)

Impact on performance
Memory overhead when the method is synchronous.
Memory overhead when the method is asynchronous.
Memory layout of the generated state machine.
Conclusion: perf is not so bad today. Tasks and async methods were designed to handle thousands of operations per second (and this is true), but you should think about granulatiry, both in terms of speed and in terms of debuggability (mention Ben.Simplifier).

# Next blog post
ValueTasks

# Next blog post
Error handling in async methods
synchronous vs. asynchronous. (precondition check.). Even if the method is synchronous, the error goes into a task, but not directly to the caller.
Task unobserved exception: why the rule was changed.

