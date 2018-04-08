# The 'in' modifier and the readonly structs in C#

C# 7.2 got two very important features for high-performance scenarios -- the readonly structs and the `in` parameters. But to understand why this additions are so important and how they're related to each other we should look back in history.

As you probably know, the .NET ecosystem has two family of types -- the value types (a.k.a. structs) and the reference types (a.k.a. classes) (*). There are a plenty of differences between them but the main one is **the semantics**. The value types follow the value semantics: (1) two instances of a value type are equal if all the data members are equal and (2) the value type instance by default is passed around by value, i.e. by creating a copy of the original instance. The reference types, on the other hand, follow the "reference semantics": (1) two instances of a reference type are equal if they point to the same instance in the managed heap (**) and (2) a reference type instance is passed by reference, i.e. by passing the pointer to the original instance in the managed heap.

(*) The third category is managed references but for the sake of this discussion we can ignore them.
(**) This behavior can be overriden by overriding `Equals` and `GetHashCode`.

## Readonly fields of the value types

To enforce the value semantics of value types the C# compiler performs some actions that could be not obvious from the developer's point of view. Here is an example:

```csharp
internal class ReadOnlyEnumerator
{
    private readonly List<int>.Enumerator _enumerator;

    public ReadOnlyEnumerator(List<int> list)
    {
        Contract.Requires(list.Count >= 1);
        _enumerator = list.GetEnumerator();
    }

    public void PrintTheFirstElement()
    {
        _enumerator.MoveNext();
        Console.WriteLine(_enumerator.Current);
    }
}

var roe = new ReadOnlyEnumerator(new List<int>{42});
roe.PrintTheFirstElement();
```

The output is not obvious: "0".

The `readonly` modifier has slightly different observable effects for the value types and for the reference types. A readonly field of a reference type is like a constant pointer: the compiler will make sure that the field is not reassigned outside the constructor even though the referenced object's state may change (if the referenced type is mutable). A readonly field of value type means that the value itself should be the same for the entire lifetime of the enclosing instance. To prevent any potential mutations, the compiler makes a defensive copy of the field each time a method or a property is used.

Under the hood `PrintTheFirstElement` does the following:

```csharp
public void PrintTheFirstElement_Decompiled()
{
    // Defensive copy
    var localEnumerator = _enumerator;
    localEnumerator.MoveNext();

    // Defensive copy (2)
    localEnumerator = _enumerator;
    Console.WriteLine(localEnumerator.Current);
}
```

This is a real issue and the reason why **mutable value types are evil**. A couple of months back I spent a few hours debugging a similar issue caused by a readonly [`SpinLock`](https://msdn.microsoft.com/en-us/library/system.threading.spinlock%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396) field.

## The performance implications of the defensive copies
Mutability is not the only problem. The defensive copies can affect performance even when the structs are immutable.

```csharp
public struct FairlyLargeStruct
{
    private long l1, l2, l3, l4;
    public int N { get; }
    public FairlyLargeStruct(int n) : this() => N = n;
}
```

Let's see what the difference between `readonly` and non-`readonly` access of the field:

```csharp
private FairlyLargeStruct _nonReadOnlyStruct = new FairlyLargeStruct(42);
private readonly FairlyLargeStruct _readOnlyStruct = new FairlyLargeStruct(42);

private readonly int[] _data = Enumerable.Range(1, 100_000).ToArray();

[Benchmark]
public int AggregateForNonReadOnlyField()
{
    int result = 0;
    foreach(int n in _data)
        result += n + _nonReadOnlyStruct.N;
    return result;
}

[Benchmark]
public int AggregateForReadOnlyField()
{
    int result = 0;
    foreach (int n in _data)
        result += n + _readOnlyStruct.N;
    return result;
}
```

The results are:

```
                       Method |      Mean |    Error |    StdDev |
----------------------------- |----------:|---------:|----------:|
 AggregateForNonReadOnlyField |  87.92 us | 1.800 us |  3.677 us |
    AggregateForReadOnlyField | 148.29 us | 4.226 us | 12.460 us |
```

The significant difference in the results caused by a defensive copy that is happening each time the readonly field is used. You may have heard that the size of the struct should be relatively small to avoid the overhead of passing it around to other methods. But as you can see, you may get a performance hit even when a fairly large struct is stored in a readonly field and never passed to another method.

There are at least 3 solutions to this problem:

1. Use fields instead of properties

```csharp
public struct FairlyLargeStruct
{
    private long l1, l2, l3, l4;
    public readonly int N;
    public FairlyLargeStruct(int n) : this() => N = n;
}
```

If the C# compiler sees an access to a `FairlyLargeStruct`'s field `N` via `readonly` variable, it won't create a defensive copy, because it knows that reading a field `N` is side effect free. This solution is not sustainable for a real world because `FairlyLargeStruct` could have methods as well, and even if there are no methods or properties today, it's just a matter of time when someone from your team will refactor the code to switch from fields to properties causing a performance regression.

2. Use non-readonly fields of `FairlyLargeStruct`

```csharp
// Use non-readonly field to avoid redundant defensive copy on each field access
private /*readonly*/FairlyLargeStruct _fairlyLargeStruct;
```

3. Use readonly structs

```csharp
public readonly struct FairlyLargeStruct
{
    private readonly long l1, l2, l3, l4;
    public int N {get;}
    public FairlyLargeStruct(int n) : this() => N = n;
}
```

## The readonly structs
C# 7.2 allows a user to enforce immutability for a given struct by using the `readonly` modifier. As you may see in a moment it is good for performance but it is also very useful from a design perspective: readonly structs clearly carries the intention that the instance is immutable and can't be changed (without some tricks like reflection).

The `reaodnly` modifier enforces the following behavior: 

1) The compiler checks that the struct is indeed immutable and consists only of readonly fields and/or readonly properties (properties like `public int Foo {get; private set;}` are not readonly).
2) Allows the compiler to skip defensive copies in some contexts, like when a readonly field of such a struct is used.

Here are the benchmark results for `readonly struct FairlyLargeStruct`:

```
                       Method |     Mean |    Error |   StdDev |
----------------------------- |---------:|---------:|---------:|
 AggregateForNonReadOnlyField | 91.19 us | 1.811 us | 2.597 us |
    AggregateForReadOnlyField | 89.25 us | 1.775 us | 3.705 us |
```

## The `in`-modifier

The very first version of the C# language had 3 ways of passing the arguments: by value (no modifier), by reference (with `ref` modifier) and as an output parameter (with `out` modifier) (***)

(***) Under the hood the CLR has only two options: passing by value and passing by reference. The `out` modifier is the same as `ref` modifier plus the compiler checks for definite assignment.

C# 7.2 introduces the third way of passing arguments: using `in`-modifier. 

The `in`-modifier is a way to pass the argument via readonly reference. Under the hood, the argument is passed by reference with a special attribute (`System.Runtime.CompilerServices.IsReadOnlyAttribute`), and the compiler makes sure that the method does not modify the parameter.

```csharp
public void Foo(in string s)
{
    // Cannot assign to variable 'in string' because it is a readonly variable
    s = string.Empty;
}
```

This simple language change has a large set of consequences.

1. You can't create an overload that differs only by `in`, `ref`, `out`. This is expected, because the `in` modifier is the same the `ref`-modifier under the hood with some additional logic from the compiler.

2. You can't use the `in`-modifier for async methods and iterator blocks.

```csharp
// Async methods cannot have ref or out parameters
async Task ByInAsync(in string s) => await Task.Yield();
```

This is expected as well because you can't use `ref`/`out` modifiers in these contexts as well. The restriction is just a side-effect of how [async methods are implemented]()https://blogs.msdn.microsoft.com/seteplia/2017/11/30/dissecting-the-async-methods-in-c/.

3. You **can** pass a variable from a using block as an `in`-argument, even though it is impossible for `ref`/`out` parameters:

```csharp
struct Disposable : IDisposable
{
    public void Dispose() { }
}

using (var d = new Disposable())
{
    // Ok
    ByIn(d);
    // Cannot use 'd' as a ref or out value because it is a 'using variable'
    ByRef(ref d);
}

void ByRef(ref Disposable disposable) { }
void ByIn(in Disposable disposable) { }
```

This is already interesting. Apparently, the restriction that 'using variable' cannot be passed by reference is the compiler restriction, not the CLR one. And in this case, the restriction is removed because it is indeed safe to pass the variable as `in` argument.

4. Default values for `in`-parameters
Here is another difference between `in`-parameters and `ref`/`out`: the `in`-parameter could have a default value:

```csharp
public int ByIn(in string s = "") => s.Length;
```

5. Extension methods that takes

5. You **can** make an overload that differs only by `in` modifier:

```csharp
public int Foo(in string s) => s.Length;
public int Foo(string s) => s.Length;
```

This case is tricky because the behavior is language-version-specific. For instance, in C# 7.2 (the first version that added this feature), it was impossible to call the second overload:

```csharp
string s = string.Empty;
Foo(in s);
// The call is ambiguous between the following methods or properties: 
// 'WeirdOverload.Foo(in string)' and 'WeirdOverload.Foo(string)'
Foo(s);
```

But this behavior was [fixed](https://github.com/dotnet/csharplang/issues/945) in C# 7.3 and now, `Foo(s)` is resolved to `Foo(string s)`.

But why the `in`-modifier is optional on the call-site? As we'll see in a moment, `in`-modifier can be very useful for high-performance scenarios, and this behavior simplifies the adoption of this feature:

```csharp
public int ByIn(in string s) => s.Length;
ByIn(in s); // Works fine
ByIn(s); // Works fine as well!
ByIn("some string"); // works with literals!
```

Even though the behavior looks a bit inconsistent (all other `ref`-like parameters should be passed using `in` or `out` keyword), the decision makes a perfect sense to. Let suppose you've changed a library code to pass some fairly large struct using `in`-modifier. You don't want every client of your library to change the call site of this method in order to benefit from your change.

As you can see the `in`-modifier is a bit tricker than you might think. Semantically it is just another form of "input" parameters, very similar to passing an argument by value. On the other hand the `in`-modifier is implemented as a `ref`-parameter making some scenarios like async methods, impossible. 

But even with the existing restrictions, the new modifier is fairly useful because it helps to express the intent more clearly, and, as we'll see in a moment, it helps in terms of performance.

## Performance characteristics of the `in`-modifier

The `in` parameters of value types are passed by reference, and that means that the cost of passing an argument is constant and doesn't depend on the size of the struct. This is a good news. But I have a bad news as well.

Let's change our original benchmark a little bit:

```csharp
public struct FairlyLargeStruct
{
    private readonly long l1, l2, l3, l4;
    public int N { get; }
    public FairlyLargeStruct(int n) : this() => N = n;
}

[Benchmark]
public int AggregatePassedByValue()
{
    return DoAggregate(new FairlyLargeStruct(42));

    // Passing by value
    int DoAggregate(FairlyLargeStruct largeStruct)
    {
        int result = 0;
        foreach (int n in _data)
            result += n + largeStruct.N;
        return result;
    }
}

[Benchmark]
public int AggregatePassedByIn()
{
    return DoAggregate(new FairlyLargeStruct(42));

    // Passing by "reference"
    int DoAggregate(in FairlyLargeStruct largeStruct)
    {
        int result = 0;
        foreach (int n in _data)
            result += n + largeStruct.N;
        return result;
    }
}
```

Note, that `FairlyLargeStruct` struct is a normal struct, not a readonly one. Here are the results:

```csharp
                 Method |      Mean |     Error |    StdDev |
----------------------- |----------:|----------:|----------:|
 AggregatePassedByValue |  71.24 us | 0.3150 us | 0.2278 us |
    AggregatePassedByIn | 124.02 us | 3.2885 us | 9.6963 us |
```

Remember I've mentioned that the `in` parameters are similar to the readonly fields? To make sure that the parameter's value stays the same the compiler make a defensive copy of the parameter every time a method/property is used. If the struct is readonly then the compiler removes the defensive copy the same way as it does for readonly fields.

It means that **you should never pass a non-readonly struct as `in` parameter**. It almost always will make the performance worse. Yes, the argument passing is cheaper, but once the parameter is used, the defensive copy will nullify the benefits or will make the performance worse. It could make sense if the struct is a C-like struct with a bunch of public fields and everyone in the team is aware that changing fields to properties would have a drastic performance impact on the application. But in this case, I would suggest passing the struct by reference instead.

## Conclusion
* Readonly structs are very useful from the design and the performance points of view.
* If the size of a readonly struct is bigger than `IntPtr.Size` you should pass it as an `in`-parameter for performance reasons.
* You may consider using the `in`-parameters for reference types to express your intent more clearly. 
* You should never use a non-readonly struct as the `in` parameters because it may negatively affect performance and could lead to an obscure behavior if the struct is mutable.