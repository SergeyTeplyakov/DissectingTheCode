# Dissecting the tuples in C#

[`System.Tuple`](http://referencesource.microsoft.com/#mscorlib/system/tuple.cs,9124c4bea9ab0199) types were introduced in .NET 4.0 with two significant drawbacks: (1) tuple types are classes and (2) there was no language support for constructing/deconstructing them. To solve these issues, C# 7 introduces new language feature as well as a new family of types (*).

Today, if you need to glue together two values to return them from a function or put two values in a hash set you can use `System.ValueTuple` types and construct them using a handy syntax:

```csharp
// Constructing the tuple instance
var tpl = (1, 2);
            
// Using tuples with a dictionary
var d = new Dictionary<(int x, int y), (byte a, short b)>();

// Tuples with different names are compatible
d.Add(tpl, (a: 3, b: 4));

// Tuples have value semantic
if (d.TryGetValue((1, 2), out var r))
{
    // Deconstructing the tuple ignoring the first element
    var (_, b) = r;
                
    // Using named syntax as well as predefined name
    Console.WriteLine($"a: {r.a}, b: {r.Item2}");
}
```

(*) `System.ValueTuple` types are introduced in .NET Framework 4.7. But you still can use the feature and target lower framework versions, in this case, you have to reference a special nuget package: [System.ValueTuple](https://www.nuget.org/packages/System.ValueTuple/).

* Tuple declaration syntax is similar to function parameter declaration: `(Type1 name1, Type2 name2)`.
* Tuple construction syntax is similar to argument construction: `(value1, optionalName: value2)`.
* Two tuples with the same element types but with different names are compatible (**): `(int a, int b) = (1, 2)`.
* Tuples have value semantic: `(1,2).Equals((a: 1, b: 2))` and `(1,2).GetHashCode() == (1,2).GetHashCode()` are both `true`.
* Tuples do not support `==` and `!=`. There is a pending discussion about it on github: ["Support for == and != on tuple types"](https://github.com/dotnet/csharplang/issues/190).
* Tuples can be "deconstructed" but only into "variable declaration" but not into "out var" or in the `case` block: `var (x, y) = (1,2)` - OK, `(var x, int y) = (1,2)` - OK, `dictionary.TryGetValue(key, out var (x, y))` - not OK, `case var (x, y): break;` - not OK.
* Tuples are mutable: `(int a, int b) x (1,2); x.a++;`.
* Tuple elements can be accessed by the name (if provided) or via generic names like `Item1`, `Item2` etc.

(**) We'll see when this is not the case in a moment.

## Tuple element names
Lack of user-defined names makes `System.Tuple` types not very useful. I can use `System.Tuple` as an implementation detail of a small method but if I need to pass it around I prefer a named type with descriptive property names. New tuple feature addresses this issue quite elegantly: you can specify names for tuple elements and unlike anonymous classed these names are available even across different assemblies.

The C# compiler emits a special attribute [`TupleElementNamesAttribute`](http://source.roslyn.io/#System.Runtime/System.Runtime.cs,3832) (***) for each tuple type used in a method signature:

(***) The attribute `TupleElementNamesAttribute` is special and can't be used directly in the user code. The compiler emits an error if you try to use it.

```csharp
public (int a, int b) Foo((int c, int d) a) => a;

// Translated to:
[return: TupleElementNames(new[] { "a", "b" })]
public ValueTuple<int, int> Foo(
    [TupleElementNames(new[] { "c", "d" })] ValueTuple<int, int> a)
{
    return a;
}
```

This helps an IDE and the compiler to "see" what the element names are and warn if they used incorrectly:

```csharp
// Ok: tuple literal can skip element names
(int x, int y) tpl = (1, 2);

// Warning: The tuple element 'a' is ignored because a different name
// or no name is specified by the target type '(int x, int y)'.
tpl = (a:1, b:2);

// Ok: tuple deconstruction ignore element names
var (a, b) = tpl;

// x: 2, y: 1. Tuple names are ignored
(int y, int x) = tpl;
```

The compiler has stronger requirements for inherited members:

```csharp
public abstract class Base
{
    public abstract (int a, int b) Foo();
}

public class Derived : Base
{
    // Error: Cannot change tuple element names when overriding method
    public override (int c, int d) Foo() => (1, 2);
}
```

Regular method arguments can be freely changed in overriden members, tuple element names in overriden members should exactly match ones from a base type.

## Element name inference

C# 7.1 introduces one additional enhancement: element name inference similar to what C# does for anonymous types.

```csharp
public void NameInference(int x, int y)
{
    // (int x, int y)
    var tpl = (x, y);
    Console.WriteLine($"x: {tpl.x}, y: {tpl.y}");

    var a = new {X = x, Y = y};

    // (int X, int y)
    var tpl2 = (a.X, a.Y);
    Console.WriteLine($"X: {tpl2.X}, Y: {tpl2.Y}");
}
```

## Value semantic and mutability

Tuples are mutable value types with elements as public fields. This sounds concerning because we know that mutable value types considered harmful. Here is a small example of their evil nature:

```csharp
var x = new { Items = new List<int> { 1, 2, 3 }.GetEnumerator() };
while (x.Items.MoveNext())
{
    Console.WriteLine(x.Items.Current);
}
```

If you'll run this code you'll get ... an infinite loop. `List<T>.Enumerator` is a mutable value type but `Items` is a property. This means that `x.Items` returns a copy of the original iterator on each loop iteration causing an infinite loop.

But mutable value types are dangerous only when the data is mixed with a behavior: an enumerator holds a state (current element) and has a behavior (an ability to advance an iterator by calling `MoveNext` method). This combination can cause issues because it's so easy to call a method on a copy instead of on an original instance -- causing effectively no-op. Here is a set of examples that can cause an unobvious behavior due to a hidden copy of a value type: [gist](https://gist.github.com/SergeyTeplyakov/8841519120c9858324314e25ddccfc52).

But one issue with mutability still remains:

```csharp
var tpl = (x: 1, y: 2);
var hs = new HashSet<(int x, int y)>();
hs.Add(tpl);

tpl.x++;
Console.WriteLine(hs.Contains(tpl)); // false
```

Tuples are very useful as keys in dictionaries and can be stored in hash sets due to a proper value semantics. But you should not mutate the state of a tuple variable between different operations with the collection.

## Deconstruction

Even though the tuple construction is special to the tuples, deconstruction is generic and can be used with any type.

```csharp
public static class VersionDeconstrucion
{
    public static void Deconstruct(this Version v, out int major, out int minor, out int build, out int revision)
    {
        major = v.Major;
        minor = v.Minor;
        build = v.Build;
        revision = v.Revision;
    }
}

var version = Version.Parse("1.2.3.4");
var (major, minor, build, _) = version;

// Prints: 1.2.3
Console.WriteLine($"{major}.{minor}.{build}");
```

Deconstruction uses "duck-typing" approach: if the compiler can find a method called `Deconstruct` for a given type - instance method or an extension method - the type is deconstructable.

## Aliasing tuples
Once you start using the tuples, you'll quickly realize that you want to "reuse" a tuple type with named elements in multiple places in your source code. But there are few issues with that. First, C# does not support global aliases for a given type. You can use 'using' alias directive, but it creates an alias visible in one file. And second, you can't even alias the tuple:

```csharp
// You can't do this: compilation error
using Point = (int x, int y);

// But you *can* do this
using SetOfPoints = System.Collections.Generics.HashSet<(int x, int y)>;
```

There is a pending discussion on github at ["Tuple types in using directives"](https://github.com/dotnet/csharplang/issues/423). So if you'll find yourself using one tuple type in multiple places you have two options: keep copy-pasting or create a named type.

## What casing for elements should I use?
Here is an interesting question: what casing rule we should follow for tuple elements? Pascal case like `ElementName` or camel case like `elementName`? On one hand, tuple elements should follow the naming rule for public members (i.e. PascalCase), but on the other hand, tuples are just bags with variables and variables are camel cased.

You may consider using a different naming scheme based on the usage and use `PascalCase` if a tuple is used as an argument or return type of a method and use `camelCase` if a tuple is created locally in the function. But I prefer to use `camelCase` all the time.

## Conclusion

I've found tuples very useful in my day to day job. I need more than one return value from a function, or I need to put a pair of values into a hashset, or I need to change a dictionary and keep not the one value but two, or the key becomes more complicated and I need to extend it with another "field".

I even use them to avoid closure allocation with methods such a [`ConcurrentDictionary.TryGetOrAdd`](https://github.com/dotnet/corefx/blob/afcbd9a5b9e4ecde2db3a69afd093631f1db91c5/src/System.Collections.Concurrent/src/System/Collections/Concurrent/ConcurrentDictionary.cs#L1010) that now takes an extra argument. And in many cases the state is a tuple as well.

The feature is very useful but I really want to see a few enhancements:

1. Global aliases: an ability to "name" a tuple and use them in the whole assembly (****).
2. Deconstruct a tuple in the pattern matching: in `out var` and in `case var`.
3. Use operator `==` for equality comparison.

(****) I know that this feature is debatable, but I think it'll be very useful. We can wait for record types, but I'm not sure if the records will be value types or reference types.