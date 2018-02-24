# The state machine layout of async methods

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