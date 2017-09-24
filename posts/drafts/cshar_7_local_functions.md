# Dissecting the local functions in C# 7


http://mustoverride.com/local_functions/

Find some stuff from:
Jon Skeet (find his book?)
Bill Wagner


## Do we need it?
Different language authors follows different philosophical schools. Some may say that the language should have extremely small core with cohesive and expressive features. But another may argue that this isn't that practical and the language should have different (even somewhat similar) features for slightly different needs.

I think that the C# language is some
There are different philosophical schools 

Lambda expression vs. Local function

## What local function can do that lambda expression can't?



## Code maintainability and Ide friendliness
Refactoring unfriendliness (can't move the function easily out).
It reduces the scope, it can be helpful.
Some ideas: 

**Consider**: declare a local function after the final return statement of the enclosing function if possible.
**Use** local functions for eager argument validation for async methods and iterator blocks.
**Use** local functions for "local" functions with iterator blocks.
**Use** local functions instead of lambda expressions if possible to avoid excessive allocations.
**Avoid** too many local functions in the same enclosing function. Consider extracting the functionality into a separate class.

## Thoughts

Local function is kind of the same declaration as other stuff, like local variable. You can't use anything inside the function that is declared after the function declaration, but you can call the function that was declared at the end of the function. 

Lambda: potentially two allocations - 1 for a closure and one for a delegate. Local function: 0 allocations in the best case.

## Implementation details

Local function like any anonymous method (TODO: check the term) can capture enclosing context, like arguments for the enclosing method, instance or static fields, or local variables declared before the local function declaration.

In case of anonymous methods the C# compiler always creates a "closure": instance of a compiler generated class that "captures" the enclosing state used by the anonymous function.

```csharp
private int m_value = 42;
private static int s_value = 42;
public void Foo(int arg)
{    
    LocalFunction();
    
    void LocalFunction()
    {
        Console.WriteLine(arg + m_value + s_value);
    }
}
```

In case of a local function the compiler is smarter. If a local function simply captures an enclosing context and there is no (implicit or explicit) conversion to delegate and if the method doesn't have any anonymous functions (*) then the compiler can generate a struct-based closure and pass the instance of it to a generated method by reference:

(*) We'll explore different cases later

```csharp
private struct <>c__DisplayClass2_0
{
    public int n;
}

public void Foo(int arg)
{
    C.<>c__DisplayClass2_0 <>c__DisplayClass2_;
    <>c__DisplayClass2_.<>4__this = this;
    <>c__DisplayClass2_.arg = arg;
    C.<Foo>g__LocalFunction2_0(ref <>c__DisplayClass2_);
}

internal static void <Foo>g__LocalFunction2_0(ref C.<>c__DisplayClass2_0 ptr)
{
    Console.WriteLine(ptr.arg + ptr.<>4__this.m_value + C.s_value);
}
```

## Closure allocation #1: conversion to a delegate

The optimization works only when the compiler can prove that the enclosing context won't live longer than the current stack frame. If a local function is converted to a delegate than this assumption is no longer true and the compiler forced to generate a closure in the heap:

```csharp
public void Foo(int arg)
{
    // Closure allocation
    Action a = LocalFunction;
    a();
    
    void LocalFunction()
    {
        Console.WriteLine(arg);
    }
}
```

This is true in other cases as well. For instance, if the local function is captured in the lambda expression, the compiler will generate the class-based closure. This is true even if the capturing happens in unreachable code in another local function:

```csharp
public void Foo(int arg)
{
    Action a = () => LocalFunction();
    return;
    
    void LocalFunction()
    {
        Console.WriteLine(arg);
    }
}
```

In this case a local method is generated inside the generated class:

```csharp
private sealed class <>c__DisplayClass0_0
{
    public int arg;

    internal void <Foo>b__0()
    {
        this.<Foo>g__LocalFunction1();
    }

    internal void <Foo>g__LocalFunction1()
    {
        Console.WriteLine(this.arg);
    }
}

public void Foo(int arg)
{
    // Closure allocation
    C.<>c__DisplayClass0_0 <>c__DisplayClass0_ = new C.<>c__DisplayClass0_0();
    <>c__DisplayClass0_.arg = arg;
    Action action = new Action(<>c__DisplayClass0_.<Foo>b__0);
}
```

The last case is the trickiest. 
Implicitely captured locals!

Here is a list of cases when the local function causes a heap allocation:
* Local function is converted to a delegate.
* Local function is captured in an anonymous method.
* If both Local function and anonymous method captures a local context (local variables or arguments).

The compiler g
 but the same context is not captured by an anonymous method, and the if a local function itself is not converted to a 

### Normal case: struct-based closure
In "normal" case the loc
If a local function 
If a local function is (explicitly or implicitely) converted to a delegate

## Use cases
```csharp
public IEnumerable<string> ReadAllText(string fileName)
{
    if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
    
    return ReadAllText();
    
    IEnumerable<string> ReadAllText()
    {
        foreach(var s in System.IO.File.ReadAllLines(fileName))
        {
            yield return s;
        }
    }
}
```