# Managed object internals, Part 3. The layout of a managed array

Arrays are one of the basic building blocks for any applications. Even if you're not using arrays directly every day you're definitely using them indirectly as part of almost any library.

C# has arrays from the very beginning and back in the day that was the only "generic"-like and type safe data structure available. Today you may use them less frequently directly but every time you need to squeeze the performance, there is a chance you'll switch to them from some higher level data structure like `List<T>`.

[Array and the CLR has a very special relationship](http://mattwarren.org/2017/05/08/Arrays-and-the-CLR-a-Very-Special-Relationship/) but today we're going to explore them from the user's point of view. Today, we'll talk about the following:

* Will explore one of the weirdest C# feature called array covariance
* Will discuss array's internal structure
* Will explore some perf tricks that we can do to squeeze even more perf out of them

**The whole series**

* [Managed object internals, Part 1. The Layout](https://blogs.msdn.microsoft.com/seteplia/2017/05/26/managed-object-internals-part-1-layout/)
* [Managed object internals, Part 2. Object header layout and the cost of locking](https://blogs.msdn.microsoft.com/seteplia/2017/09/06/managed-object-internals-part-2-object-header-layout-and-the-cost-of-locking/)
* Managed object internals, Part 3. The layout of a managed array

## Array covariance, and a bit of history
One of the strangest features in the C# language is an array covariance: an ability to assign an array of type `T` to array of type `object` or any other base type of `T`.

```csharp
string[] strings = new [] {"1", "2"};
object[] objects = strings;
```

This conversion is not totally type safe. If the `objects` variable is used only for reading the data from it, everything is fine. But if somebody will try to modify the array then failure can occur if the argument will be of an incompatible type:

```csharp
objects[0] = 42; // runtime error
```

There is a well-known joke in the .NET community about that feature: C# authors back in the inception days were trying really hard to copy every aspect of the Java ecosystem to the CLR world, so they copied language design issues as well.

But I don't think this is the reason.

Back in the late 90-s, the CLR doesn't have generics. Right? And how in this case language users can write reusable code that deals with an array of arbitrary data types? For instance, how to write a function that dumps the data to the console?

One way to do that is to define a function that takes `object[]` and force every caller to convert their array manually by copying it into the array of objects. This will work but would be highly inefficient. Another solution is to allow conversion from any arrays of reference types to `object[]`, i.e. preserve IS-A relationship for `Derived[]` to `Base[]` where `Derived` inherits from the `Base`.

Lack of generics in the first CLR version forced designers to weaken the type system. But that decision (I assume) was deliberate, not just a copy cat from the Java ecosystem.

## The internal structure and implementation details

Array covariance opens a hole in the type system at compile time, but it doesn't mean that a type error will crash the application (similar "error" in C++ will lead to an undefined behavior). The CLR will ensure that the type safety holds, but the check will happen at runtime. To do that the CLR should store the type of an element of an array element and make a check when a user tries to change an array instance. Luckily this check is only needed for arrays of reference types because structs are 'sealed' and do not support inheritance.

Even though there is an implicit conversion between different value types (like from `int` to `byte`), there are **no** implicit or explicit conversions between `int[]` and `byte[]`. Array covariance conversion is **reference conversion** that doesn't change the layout of the converted objects and keeps the referential identity of the object being converted.

In the older version of the CLR, arrays of reference and value types had different layouts. An array of reference type had a reference to a type handle of an element in each instance:

(https://github.com/SergeyTeplyakov/DissectingTheCode/blob/master/posts/Images/Arrays_Figure_1.png "Old array layout")

This has been changed in a recent version of the CLR and now the element type is stored in the method table:

(https://github.com/SergeyTeplyakov/DissectingTheCode/blob/master/posts/Images/Arrays_Figure_2.png "New array layout")

For more information, see the following snippets:

* [`ArrayBase::GetArrayElementTypeHandle`](https://github.com/dotnet/coreclr/blob/5c07c5aa98f8a088bf25099f1ab2d38b59ea5478/src/vm/object.h#L805-L807) declaration:
```c
// Get the element type for the array, this works whether the the element
// type is stored in the array or not
inline TypeHandle GetArrayElementTypeHandle() const;
```

* [`PtrArray::GetArrayElementTypeHandle`](https://github.com/dotnet/coreclr/blob/5c07c5aa98f8a088bf25099f1ab2d38b59ea5478/src/vm/object.h#L949-L953) implementation:
```c
    TypeHandle GetArrayElementTypeHandle()
    {
        LIMITED_METHOD_CONTRACT;
        return GetMethodTable()->GetApproxArrayElementTypeHandle();
    }
```
* [`MethodTable::GetApproxArrayElementTypeHandle`](https://github.com/dotnet/coreclr/blame/559c603f2e9d2d89cca6c7c6731f720a7935e369/src/vm/methodtable.h#L2921-L2926) implementation:
```c
TypeHandle GetApproxArrayElementTypeHandle()
{
    LIMITED_METHOD_DAC_CONTRACT;
    _ASSERTE (IsArray());
    return TypeHandle::FromTAddr(m_ElementTypeHnd);
}
```
* and [`MethodTable::m_ElementTypeHnd`](https://github.com/dotnet/coreclr/blob/559c603f2e9d2d89cca6c7c6731f720a7935e369/src/vm/methodtable.h#L4173) declaration:
```c
union
{
    PerInstInfo_t m_pPerInstInfo;
    TADDR         m_ElementTypeHnd;
    TADDR         m_pMultipurposeSlot1;
};
```

I'm not sure when the layout of the array was changed (*) but it seems there was a trade off between speed and managed memory. Initial implementation (when the type handle was stored in every array instance) should've been faster due to memory locality, but definitely had a non-negligible memory overhead. Back then all arrays of reference types had shared method tables. But right now this is no longer the case: each array of reference type has its own method table that [points to the same EEClass](https://github.com/dotnet/coreclr/blob/7590378d8a00d7c29ade23fada2ce79f4495b889/src/vm/array.cpp#L272) that points to an element type handle.

(*) Maybe someone from the CLR team can shed some lights on that.

We know how the CLR stores the element type of an array and we can explore the CoreClr codebase to see how the actual check is implemented.

First, we need to find the place where the check is happening. Arrays are very special types for the CLR and there is no "go to declaration" button in the IDE that will "decompile" the array and show the source code of it. But we know that the check is happening in the indexer setter that corresponds to a set of IL instructions `StElem*`:

* [`StElem.i4`](https://msdn.microsoft.com/en-us/library/system.reflection.emit.opcodes.stelem_i4(v=vs.110).aspx) for array of integers, 
* [`StElem`](https://msdn.microsoft.com/en-us/library/system.reflection.emit.opcodes.stelem(v=vs.110).aspx) for array of arbitrary value types and 
* [StElem.ref](https://msdn.microsoft.com/en-us/library/system.reflection.emit.opcodes.stelem_ref%28v=vs.110%29.aspx) for array of reference types. 

Knowing the instruction, we can easily find the implementation in the codebase. As far as I can tell, the implementation resides in [jithelpers.cpp](https://github.com/dotnet/coreclr/blob/7590378d8a00d7c29ade23fada2ce79f4495b889/src/vm/jithelpers.cpp#L3386-L3458). Here is a slightly simplified version of the method `JIT_Stelem_Ref_Portable`:

```c
/****************************************************************************/
/* assigns 'val to 'array[idx], after doing all the proper checks */

HCIMPL3(void, JIT_Stelem_Ref_Portable, PtrArray* array, unsigned idx, Object *val)
{
    FCALL_CONTRACT;

    if (!array) 
    {
        // ST: explicit check that the array is not null
        FCThrowVoid(kNullReferenceException);
    }
    if (idx >= array->GetNumComponents()) 
    {
        // ST: bounds check
        FCThrowVoid(kIndexOutOfRangeException);
    }

    if (val) 
    {
        MethodTable *valMT = val->GetMethodTable();
        // ST: getting type of an array element
        TypeHandle arrayElemTH = array->GetArrayElementTypeHandle();

        // ST: g_pObjectClass is a pointer to EEClass instance of the `System.Object`
        // ST: if the element is `object` than the operation is successful.
        if (arrayElemTH != TypeHandle(valMT) && arrayElemTH != TypeHandle(g_pObjectClass))
        {   
            // ST: need to check that the value is compatible with the element type
            TypeHandle::CastResult result = ObjIsInstanceOfNoGC(val, arrayElemTH);
            if (result != TypeHandle::CanCast)
            {
                // ST: ArrayStoreCheck throws ArrayTypeMismatchException if the types are incompatible
                if (HCCALL2(ArrayStoreCheck,(Object**)&val, (PtrArray**)&array) != NULL)
                {
                    return;
                }
            }
        }

        HCCALL2(JIT_WriteBarrier, (Object **)&array->m_Array[idx], val);
    }
    else
    {
        // no need to go through write-barrier for NULL
        ClearObjectReference(&array->m_Array[idx]);
    }
}
```

## Increasing the performance by removing the type check
Now we know that the CLR does under the hood to guarantee type safety for arrays of reference types. We know that every "write" to the array instance has an additional check that can be non-negligible if the array is used on the extremely hot path. But before getting into a wrong conclusion, let's check how expensive the check is. 

To avoid the check we could change the CLR or we can use a well-known trick: wrap an object into a struct:

```csharp
public struct ObjectWrapper
{
    public readonly object Instance;
    public ObjectWrapper(object instance)
    {
        Instance = instance;
    }
}
```

Now we can compare the time for `object[]` and `ObjectWrapper[]`:

```csharp
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

```

The results are:

```
       Method |     Mean |     Error |    StdDev |
------------- |---------:|----------:|----------:|
    WithCheck | 807.7 us | 15.871 us | 27.797 us |
 WithoutCheck | 442.7 us |  9.371 us |  8.765 us |
```

Don't be confused with "almost 2x" performance difference. Even for the worst case, it takes less than a millisecond to assign 100K elements. The performance is extremely good. But the difference could be noticeable in the real world.

Many performance critical .NET applications are using object pools. The pool allows reusing a managed instance without creating a new one each time. This approach reduces the memory pressure and could have a very reasonable impact on the application performance.

Object pool can be implemented based on a concurrent data structure like [`ConcurrentQueue`](http://referencesource.microsoft.com/#mscorlib/system/Collections/Concurrent/ConcurrentQueue.cs,18bcbcbdddbcfdcb) or based on a simple array. Here is a snippet from the [object pool implementation](http://source.roslyn.io/#Microsoft.CodeAnalysis/ObjectPool%25601.cs,40) in the Roslyn codebase:

```csharp
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
}
```

The implementation manages an array of cached items but instead of using `T[]` the pool wraps the `T` into the struct `Element` to avoid the check at the runtime.

Some time ago I've fixed an object pool in our application to get 30% performance improvement for the parsing phase. This was not due to the trick that I'm describing here and was related to concurrent access of the pool. But the point is that object pools could be on the hot path of the application and even small performance improvements like one mentioned above could have a reasonable impact on the end to end performance.