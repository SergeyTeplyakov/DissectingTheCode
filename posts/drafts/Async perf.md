# Async method performance

Outline:
Compare task-based async, valuetask-based async and sync methods. See other perf benchmarks from ben adams and others.
Explain the perf: see how the generated state machine is looks like
Discuss the perf of my project and explain why you shouldn't care too much: if the perf matters, most likely you should think about coarser-grained async first, and then consider switching to value-task-based.

Impact on performance
Memory overhead when the method is synchronous.
Memory overhead when the method is asynchronous.
Memory layout of the generated state machine.
Conclusion: perf is not so bad today. Tasks and async methods were designed to handle thousands of operations per second (and this is true), but you should think about granulatiry, both in terms of speed and in terms of debuggability (mention Ben.Simplifier).


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

# Next blog post
ValueTasks

# Next blog post
Error handling in async methods
synchronous vs. asynchronous. (precondition check.). Even if the method is synchronous, the error goes into a task, but not directly to the caller.
Task unobserved exception: why the rule was changed.

