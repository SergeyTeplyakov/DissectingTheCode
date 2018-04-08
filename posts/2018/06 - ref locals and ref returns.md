# Performance traps of ref locals and ref returns in C#

The C# language from the very first version supported passing arguments by value or by reference. But before C# 7 the C# compiler supported only one way of returning a value from a method (or a property) - returning by value. This has been changed in C# 7 with two new features: ref returns and ref locals.

But unlike other features that were recently added to the C# language I've found these two a bit more controversial than others.

## The motivation
There are many differences between the arrays and other collections from the CLR perspectives. The arrays were added to the CLR from the very beginning and you can think of them as of built-in generics. The CLR and the JIT-compiler are aware of the arrays but besides that, they're special in one more aspect: **the indexer of the array returns the element by reference, not by value**.

To demonstrate this behavior we have to go to the dark side -- use mutable value types:

```csharp
public struct Mutable
{
    private int _x;
    public Mutable(int x) => _x = x;

    public int X => _x;

    public void IncrementX() { _x++; }
}

[Test]
public void CheckMutability()
{
    var ma = new[] {new Mutable(1)};
    ma[0].IncrementX();
    // X has been changed!
    Assert.That(ma[0].X, Is.EqualTo(2));

    var ml = new List<Mutable> {new Mutable(1)};
    ml[0].IncrementX();
    // X hasn't been changed!
    Assert.That(ml[0].X, Is.EqualTo(1));
}
```

The test will pass because the indexer of the array is quite different from the indexer of the `List<T>`.

The C# compiler emits a special instruction for the arrays indexer - [`ldelema`](https://msdn.microsoft.com/en-us/library/system.reflection.emit.opcodes.ldelema(v=vs.110).aspx) that returns a managed reference to a given array's element. Basically, array indexer returns an element by reference. But `List<T>` can't have the same behavior because it wasn't possible (*) to return an alias to the internal state in C#. That's why the `List<T>` indexer returns the element by value, i.e. returning the copy of the given element.

(*) As we'll see at the moment, it is still impossible for the `List<T>`'s indexer to return an element by reference.

This means that `ma[0].IncrementX()` calls a mutation method on the first element inside of the array, but `ml[0].IncrementX()` calls a mutation method on a copy, keeping the original list unchanged.

## Ref locals and ref return 101
The basic idea behind these features is very simple: `ref return` allows to return an alias to an existing variable and ref locals can store the alias in a local variable.

1. Simple example

```csharp
[Test]
public void RefLocalsAndRefReturnsBasics()
{
    int[] array = {1, 2};
            
    // Capture an alias to the first element into a local
    ref int first = ref array[0];
    first = 42;
    Assert.That(array[0], Is.EqualTo(42));
           
    // Local function that returns the first element by ref
    ref int GetByRef(int[] a) => ref a[0];
    // Weird syntax: the result of a function call is assignable
    GetByRef(array) = -1;
    Assert.That(array[0], Is.EqualTo(-1));
}
```

2. Ref returns and readonly ref returns

Ref returns also can return an alias to instance fields and also, can return a readonly alias using `ref readonly`:

```csharp
class EncapsulationWentWrong
{
    private readonly Guid _guid;
    private int _x;

    public EncapsulationWentWrong(int x) => _x = x;

    // Return an alias to a private field. No encapsulation any more.
    public ref int X => ref _x;

    // Return a readonly alias to the private field.
    public ref readonly Guid Guid => ref _guid;
}

[Test]
public void NoEncapsulation()
{
    var instance = new EncapsulationWentWrong(42);
    instance.X++;

    Assert.That(instance.X, Is.EqualTo(43));

    // Cannot assign to property 'EncapsulationWentWrong.Guid' because it is a readonly variable
    // instance.Guid = Guid.Empty;
}
```

* Methods and properties could return "alias" to an internal state. **The property, in this case, could not have a setter.**
* Return by reference breaks the encapsulation because the client obtains the full control over the object's internal state.
* Return by reference avoids a copy operation for value types.
* Returning by readonly reference avoids a redundant copy for value types but prevents the client from mutating the internal state.

3. Existing restrictions
Returning an alias could be dangerous: using an alias to a stack-allocated variable after a method is finished will crash the app. To make the feature safe, the C# compiler enforces various restrictions:

* You can not return a reference to a local variable.
* You can not return a reference to `this` in structs.
* You can return a reference to heap-allocated variable (like class members).
* You can return a reference to ref/out parameters.

For more information see an amazing post [Safe to return rules for ref returns](http://mustoverride.com/safe-to-return/) by Vladimir Sadov, the author of this feature in the C# compiler.

Now, once we know what these features are, let's see when they can be useful.

## Using ref returns for indexers

To test the performance impact, we're going to create a custom immutable collection called `NaiveImmutableList<T>` and will compare it with the `T[]` and the `List<T>` for structs of different sizes (4, 16, 32 and 48).

```csharp
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
            // Extracting 'throw' statement into a different
            // method helps the jitter to inline this property.
            if ((uint) idx >= (uint) _length)
                ThrowIndexOutOfRangeException();

            return ref _data[idx];
        }
    }

    private static void ThrowIndexOutOfRangeException() =>
        throw new IndexOutOfRangeException();
}

struct LargeStruct_48
{
    public int N { get; }
    private readonly long l1, l2, l3, l4, l5;
        
    public LargeStruct_48(int n) : this() 
        => N = n;
}

// Other structs like LargeStruct_16, LargeStruct_32 etc
```

The benchmarks iterate over the collections and sum all the `N` property values for each elements:

```csharp
private const int elementsCount = 100_000;
private readonly LargeStruct_48[] _array48 = CreateArray_48();

[Benchmark]
public int TestArray_48()
{
    int result = 0;
    // Using elementsCound but not Length to force the bounds check
    // on each iteration regarding of a collection type.
    for (int i = 0; i < elementsCount; i++)
    {
        result = _array48[i].N;
    }

    return result;
}

```

And here the results:

```
                    Method |     Mean | Scaled |
-------------------------- |---------:|-------:|
              TestArray_48 | 258.3 us |   1.00 |
            TestListOfT_48 | 488.9 us |   1.89 |
 TestNaiveImmutableList_48 | 444.8 us |   1.72 |
                           |          |        |
              TestArray_32 | 174.4 us |   1.00 |
            TestListOfT_32 | 233.8 us |   1.34 |
 TestNaiveImmutableList_32 | 219.2 us |   1.26 |
                           |          |        |
              TestArray_16 | 143.7 us |   1.00 |
             TestListOfT16 | 192.5 us |   1.34 |
  TestNaiveImmutableList16 | 167.8 us |   1.17 |
                           |          |        |
               TestArray_4 | 121.7 us |   1.00 |
             TestListOfT_4 | 174.7 us |   1.44 |
  TestNaiveImmutableList_4 | 133.1 us |   1.09 |
```

Apparently, something is wrong! Our `NaiveImmutableList<T>` has effectively the same performance characteristics as `List<T>`. What happened?

## Readonly ref returns under the hood

As you may notice, the indexer of `NaiveImmutableList<T>` returns a readonly reference via `ref readonly`. This makes a perfect sense because we want to restrict our clients from mutating the underlying state of the immutable collection. But the structs we've been using in our benchmarks are regular non-readonly structs.

The following test will help us understand the underlying behavior:

```csharp
[Test]
public void CheckMutabilityForNaiveImmutableList()
{
    var ml = new NaiveImmutableList<Mutable>(new Mutable(1));
    ml[0].IncrementX();
    // X has been changed, right?
    Assert.That(ml[0].X, Is.EqualTo(2));
}
```

The test fails! Why? Because "readonly references" are similar to `in`-modifiers and `readonly` fields in respect to structs: the compiler emits a defensive copy every time a struct member is used. It means that `ml[0].` still creates a copy of the first element but not by the indexer: the copy is created in the calling method.

In fact, this makes a perfect sense. The C# compiler supports passing arguments by value, by reference, and by "readonly reference" using `in`-modifier (for more details see my post [The `in`-modifier and the readonly structs in C#](https://blogs.msdn.microsoft.com/seteplia/2018/03/07/the-in-modifier-and-the-readonly-structs-in-c/)). And now the compiler supports 3 different ways of returning a value from a method: by value, by reference and by readonly reference.

"Readonly references" are so similar, that the compiler reuses the same `InAttribute` to distinguish readonly and non-readonly return values:

```csharp
private int _n;
public ref readonly int ByReadonlyRef() => ref _n;
```

In this case the method `ByReadonlyRef` is effectively compiled to:

```csharp
[InAttribute]
[return: IsReadOnly]
public int* ByReadonlyRef()
{
    return ref this._n;
}
```

The similarity between `in`-modifier and readonly references means that these features are not very friendly to regular structs and could cause performance issues. Here is an example:

```csharp
struct BigStruct
{
    // Other fields
    public int X { get; }
    public int Y { get; }
}

public ref readonly BigStruct GetBigStructByRef() => ref _bigStruct;

ref readonly var bigStruct = GetBigStructByRef();
int result = bigStruct.X + bigStruct.Y;
```

Besides a weird syntax of variable declaration for `bigStruct` the code looks good. The intent is clear: `BigStruct` is returned by reference for performance reasons. Unfortunately, because `BigStruct` is a non-readonly struct, each time a member is accessed, the defensive copy is created.

## Using ref returns for indexers. Attempt #2

Let's try the same set of benchmarks with **readonly structs** of different sizes:

```
                    Method |     Mean | Scaled |
-------------------------- |---------:|-------:|
              TestArray_48 | 265.1 us |   1.00 |
            TestListOfT_48 | 490.6 us |   1.85 |
 TestNaiveImmutableList_48 | 300.6 us |   1.13 |
                           |          |        |
              TestArray_32 | 177.8 us |   1.00 |
            TestListOfT_32 | 233.4 us |   1.31 |
 TestNaiveImmutableList_32 | 218.0 us |   1.23 |
                           |          |        |
              TestArray_16 | 144.7 us |   1.00 |
             TestListOfT16 | 191.8 us |   1.33 |
  TestNaiveImmutableList16 | 168.8 us |   1.17 |
                           |          |        |
               TestArray_4 | 121.3 us |   1.00 |
             TestListOfT_4 | 178.9 us |   1.48 |
  TestNaiveImmutableList_4 | 145.3 us |   1.20 |
```

Now the results make much more sense. The time still grows for bigger structs, but that is expected because iterating over 100K structs of bigger size take a longer amount of time. But now the time for `NaiveimmutableList<T>` is way closer to `T[]` and reasonably faster than `List<T>`.

## Conclusion

* Be cautious with ref returns because they can break encapsulation.
* Be cautious with readonly ref returns because they're more performant only for readonly structs and could cause performance issues for regular structs.
* Be cautious with readonly ref locals because they as well could cause performance issues for non-readonly structs causing defensive copy each time the variable is used.

Ref locals and ref returns are useful features for library authors and developers working on infrastructure code. But in the case of library code, these features are quite dangerous: in order to use a collection that returns elements by readonly reference efficiently every library user should know the implications: readonly reference for a non-readonly struct causes a defensive copy "at the call site". This can negate all performance gains at best or can cause severe perf degradation when a readonly ref local accessed multiple times.

P.S. Readonly references are coming to the BCL. The following PR for corefx repo ([Implementing ItemRef API Proposal](https://github.com/dotnet/corefx/pull/25738/files#diff-fa508ecac55e620b269a8853de2cfd66)) introduced readonly ref methods to access the elements of immutable collections.