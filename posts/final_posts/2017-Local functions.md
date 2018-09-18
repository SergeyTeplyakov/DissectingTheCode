# Dissecting the local functions in C# 7

Local functions is a new feature in C# 7 that allows defining a function inside another function. 

## When to use a local function?

The main idea of local functions is very similar to anonymous methods: in some cases creating a named function is too expensive in terms of cognitive load to a reader. Sometimes the functionality is inherently local to another function and it makes no sense to pollute the "outer" scope with a separate named entity.

You may think that this feature is redundant because the same behavior can be achieved with anonymous delegates or lambda expression. But this is not always the case. Anonymous functions have certain restrictions and their performance characteristics can be unsuitable for your scenarios.

## Use Case 1: eager preconditions in iterator blocks
Here is a simple function that reads file line by line. Do you know when the `ArgumentNullException` will be thrown?

```csharp
public static IEnumerable<string> ReadLineByLine(string fileName)
{
    if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
    foreach(var line in File.ReadAllInes(fileName))
    {
        yield return line;
    }
}

// When the error will happen?
string fileName = null;
// Here?
var query = ReadLineByLine(fileName).Select(x => $"\t{x}").Where(l => l.Length > 10);
// Or here?
ProcessQuery(query);
```

Methods with `yield return` in their body are special. They called [Iterator Blocks](https://msdn.microsoft.com/en-us/library/65zzykke%28v=vs.100%29.aspx?f=255&MSPPError=-2147217396) and they're lazy. This means that the execution of those methods is happening "by demand" and the first block of code in it will be executed only when the client of the method will call `MoveNext` method on the iterator. In our case, it means the error will happen only in the `ProcessQuery` method because all the LINQ-operators are lazy as well.

Obviously, this is not a good design because `ProcessQuery` method will not have enough information about the context of the `ArgumentNullException`. So it would be good to throw the exception eagerly - when a client calls `ReadLineByLine` but not when a client processes the result.

To solve this issue we need to extract the validation logic into a separate method. This is a good candidate for anonymous function but anonymous delegates and lambda expressions do not support iterator blocks (*)::

(*) Lambda expressions in VB.NET can have an iterator block.

```csharp
public static IEnumerable<string> ReadLineByLine(string fileName)
{
    if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));

    return ReadLineByLineImpl();

    IEnumerable<string> ReadLineByLineImpl()
    {
        foreach(var line in File.ReadAllInes(fileName))
        {
            yield return line;
        }
    }
}

``` 

## Use Case 2: eager preconditions in async methods
Async methods have the similar issue with exception handling: any exception thrown in a method marked with `async` keyword (**) manifests itself in a faulted task:

```csharp
public static async Task<string> GetAllTextAsync(string fileName)
{
    if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
    var result = await File.ReadAllTextAsync(fileName);
    Log($"Read {result.Length} lines from '{fileName}'");
    return result;
}

string fileName = null;
// No exceptions
var task = GetAllTextAsync(fileName);
// The following line will throw
var lines = await tsk;
```

(**) Technically, `async` is a contextual keyword, but this doesn't change my point.

You may think that there is not much of a difference when the error is happening. But this is far from the truth. 
Faulted task means that the method itself failed to do what it was supposed to do. The failed task means that the problem is in the method itself or in one of the building blocks that the method relies on. 

Eager preconditions validation is especially important when the resulting task is passed around the system. In this case, it would be extremely hard to understand when and what went wrong. A local function can solve this issue:

```csharp
public static Task<string> GetAllTextAsync(string fileName)
{
    // Eager argument validation
    if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
    rerturn GetAllTextAsync();

    async Task<string> GetAllTextAsync()
    {
        var result = await File.ReadAllTextAsync(fileName);
        Log($"Read {result.Length} lines from '{fileName}'");
        return result;
    }
}
```

## Use Case 3: local function with iterator blocks
I found very annoying that you can't use iterators inside a lambda expression. Here is a simple example: if you want to get all the fields in the type hierarchy (including the private once) you have to traverse the inheritance hierarchy manually. But the traversal logic is method-specific and should be kept as local as possible:

```csharp
public static FieldInfo[] GetAllDeclaredFields(Type type)
{
    var flags = BingingFlags.Instance | BindingFlas.Public |
                BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
    return TraverseBaseTypeAndSelf(type)
            .Select(t => t.GetFields(flags));

    IEnumerable<Type> TraverseBaseTypesAndSelf(Type t)
    {
        while(t != null)
        {
            yield return t;
            t = t.BaseType;
        }
    }
}
```

## Use Case 4: recursive anonymous method
Anonymous functions can't reference itself by default. To work around this restriction you should declare a local variable of a delegate type and then capture that local variable inside the lambda expression or anonymous delegate:

```csharp
public static List<Type> BaseTypesAndSelf(Type type)
{
    Action<List<Type>, Type> addBaseType = null;
    addBaseType = (lst, t) =>
    {
        lst.Add(t);
        if (t.BaseType != null)
        {
            addBaseType(lst, t.BaseType);
        }
    };

    var result = new List<Type>();
    addBaseType(result, type);
    return result;
}
```

This approach is not very readable and similar solution with local function feels way more natural:

```csharp
public static List<Type> BaseTypesAndSelf(Type type)
{
    return AddBaseType(new List<Type>(), type);

    List<Type> AddBaseType(List<Type> lst, Type t)
    {
        lst.Add(t);
        if (t.BaseType != null)
        {
            AddBaseType(lst, t.BaseType);
        }
        return lst;
    }
}
```

## Use Case 5: when allocations matters

If you ever work on a performance critical application, then you know that anonymous methods are not cheap:

* Overhead of a delegate invocation (very very small, but it does exist).
* **2 heap allocations** if a lambda captures local variable or argument of enclosing method (one for closure instance and another one for a delegate itself).
* **1 heap allocation** if a lambda captures an enclosing instance state (just a delegate allocation).
* **0 heap allocations** only if a lambda does not capture anything or captures a static state.

But this is not the case for local functions.

```csharp
public void Foo(int arg)
{
    PrintTheArg();
    return;
    void PrintTheArg()
    {
        Console.WriteLine(arg);
    }
}
```

If a local function captures a local variable or an argument then the C# compiler generates a special closure struct, instantiates it and passes it by reference to a generated static method:

```csharp
private struct c__DisplayClass0_0
{
    public int arg;
}

public void Foo(int arg)
{
    // Closure instantiation
    var c__DisplayClass0_ = new c__DisplayClass0_0() { arg = arg };
    // Method invocation with a closure passed by ref
    Foo_g__PrintTheArg0_0(ref c__DisplayClass0_);
}

internal static void Foo_g__PrintTheArg0_0(ref c__DisplayClass0_0 ptr)
{
    Console.WriteLine(ptr.arg);
}
```

(The compiler generates names with invalid characters like `<` and `>`. To improve readability I've changed the names and simplified the code a little bit.)

A local function can capture instance state, local variables (***) or arguments. No heap allocation will happen.

(***) Local variables used in a local function should be definitely assigned at the local function declaration site.

There are a few cases when a heap allocation will occur:

1. A local function is explicitly or implicitly converted to a delegate.

**Only a delegate allocation** will occur if a local function captures static/instance fields but does not capture locals/arguments.

```csharp
public void Bar()
{
    // Just a delegate allocation
    Action a = EmptyFunction;
    return;
    void EmptyFunction() {}
}
```

**Closure allocation and a delegate allocation** will occur if a local function captures locals/arguments:

```csharp
public void Baz(int arg)
{
    // Local function captures an enclosing variable.
    // The compiler will instantiate a closure and a delegate
    Action a = EmptyFunction;
    return;
    void EmptyFunction() {Console.WriteLine(arg);}
}
```

2. A local function captures a local variable/argument and anonymous function captures variable/argument from the same scope.

This case is way more subtle.

The C# compiler generates a different closure type per lexical scope (method arguments and top-level locals resides in the same scope). In the following case the compiler will generate two closure types:

```csharp
public void DifferentScopes(int arg)
{
    {
        int local = 42;
        Func<int> a = () => local;
        Func<int> b = () => local;
    }

    Func<int> c = () => arg;
}
```

Two different lambda expressions will use the same closure type if they capture locals from the same scope. Lambdas `a` and `b` reside in the same closure:

```csharp
private sealed class c__DisplayClass0_0
{
    public int local;

    internal int DifferentScopes_b__0()
    {
        // Body of the lambda 'a'
        return this.local;
    }

    internal int DifferentScopes_b__1()
    {
        // Body of the lambda 'a'
        return this.local;
    }
}

private sealed class c__DisplayClass0_1
{
    public int arg;

    internal int DifferentScopes_b__2()
    {
        // Body of the lambda 'c'
        return this.arg;
    }
}

public void DifferentScopes(int arg)
{
    var closure1 = new c__DisplayClass0_1 { arg = arg };
    var closure2 = new c__DisplayClass0_0() { local = 42 };
    Func<int> a = closure1.DifferentScopes_b__0;
    Func<int> b = closure1.DifferentScopes_b__1;
    Func<int> c = closure2.DifferentScopes_b__2;
}
```

In some cases, this behavior can cause some very serious memory-related issues. Here is an example:

```csharp
private Func<int> func;
public void ImplicitCapture(int arg)
{
    var o = new VeryExpensiveObject();
    Func<int> a = () => o.GetHashCode();
    Console.WriteLine(a());

    Func<int> b = () => arg;
    func = b;
}
```

It seems that the `o` variable should be eligible for garbage collection right after the delegate invocation `a()`. But this is not the case. Two lambda expressions share the same closure type:

```csharp
private sealed class c__DisplayClass1_0
{
    public VeryExpensiveObject o;
    public int arg;

    internal int ImplicitCapture_b__0()
        => this.o.GetHashCode();

    internal int ImplicitCapture_b__1()
        => this.arg;
}

private Func<int> func;

public void ImplicitCapture(int arg)
{
    var c__DisplayClass1_ = new c__DisplayClass1_0()
    {
        arg = arg,
        o = new VeryExpensiveObject()
    };
    Func<int> a = c__DisplayClass1_.ImplicitCapture>b__0;
    Console.WriteLine(func());
    Func<int> b = c__DisplayClass1_.ImplicitCapture>b__1;
    this.func = b;
}
```

This means that **the lifetime of the closure is bound to the lifetime of the `func` field**. And this can prolong the lifetime of the `VeryExpensiveObject` drastically causing, basically, a memory leak.

A similar issue happens when a local function an lambda expression captures variables from the same scope. Even if they capture different variables the closure type will be shared causing a heap allocation:

```csharp
private sealed class c__DisplayClass0_0
{
    public int arg;
    public int local;

    internal int ImplicitAllocation_b__0()
        => this.arg;

    internal int ImplicitAllocation_g__Local1()
        => this.local;
}

public int ImplicitAllocation(int arg)
{
    var c__DisplayClass0_ = new c__DisplayClass0_0 { arg = arg };
    if (c__DisplayClass0_.arg == int.MaxValue)
    {
        Func<int> func = c__DisplayClass0_.ImplicitAllocation_b__0;
    }
    c__DisplayClass0_.local = 42;
    return c__DisplayClass0_.ImplicitAllocation>g__Local1();
}
```

As you can see all the locals from the top-level scope now become part of the closure class causing the closure allocation even when a local function and lambda expression capture different variables.

## Local functions 101

Here is a list of the most important aspects of local functions in C#:

1. Local functions can define iterators.
2. Local functions useful for eager validation for async methods and iterator blocks.
3. Local functions can be recursive.
4. Local functions are allocation-free if no conversion to delegates is happening.
5. Local functions are slightly more performant than anonymous functions due to a lack of delegate invocation overhead.
6. Local functions can be declared after return statement separating main logic from the helpers.
7. Local functions can "hide" a function with the same name declared in the outer scope.
8. Local functions can be `async` and/or `unsafe` no other modifiers are allowed.
9. Local functions can't have attributes.
10. Local functions are not very IDE friendly: there is no "extract local function refactoring" (yet) and if a code with a local function is partially broken you'll get a lot of "squiggles" in the IDE.