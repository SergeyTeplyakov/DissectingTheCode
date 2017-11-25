# Dissecting the pattern matching in C# 7

C# 7 finally introduced a long-awaited feature called "pattern matching". If you're familiar with functional languages like F# you may be slightly disappointed with this feature in its current state, but even today it can simplify your code in a variety of different scenarios.

Every new feature is fraught with danger for a developer working on a performance critical application. New levels of abstractions are good but in order to use them effectively, you should know what is happening under the hood. Today we're going to explore pattern matching and look under the covers to understand how it is implemented.

The C# language introduced the notion of a pattern that can be used in `is`-expression and inside a `case` block of a `switch` statement.

There are 3 types of patterns:
* The const pattern
* The type pattern
* The `var` pattern

## Pattern matching in `is`-expressions

```csharp
public void IsExpressions(object o)
{
    // Alternative way checking for null
    if (o is null) Console.WriteLine("o is null");
    
    // Const pattern can refer to a constant value
    const double value = double.NaN;
    if (o is value) Console.WriteLine("o is value");

    // Const pattern can use a string literal
    if (o is "o") Console.WriteLine("o is \"o\"");

    // Type pattern
    if (o is int n) Console.WriteLine(n);

    // Type pattern and compound expressions
    if (o is string s && s.Trim() != string.Empty)
        Console.WriteLine("o is not blank");
}
```

`is`-expression can check if the value is equal to a constant and a type check can optionally specify the **pattern variable**. 

I've found few interesting aspects related to pattern matching in `is`-expressions:

* Variable introduced in an `if` statement is lifted to the outer scope.
* Variable introduced in an `if` statement is definitely assigned only when the pattern is matched.
* Current implementation of the const pattern matching in `is`-expressions is not very efficient.

Let's check the first two cases first:

```csharp
public void ScopeAndDefiniteAssigning(object o)
{
    if (o is string s && s.Length != 0)
    {
        Console.WriteLine("o is not empty string");
    }

    // Can't use 's' any more. 's' is already declared in the current scope.
    if (o is int n || (o is string s2 && int.TryParse(s2, out n)))
    {
        Console.WriteLine(n);
    }
}
```

The first `if` statement introduces a variable `s` and the variable is visible inside the whole method. This is reasonable but will complicate the logic if the other if-statements in the same block will try to reuse the same name once again. In this case, you **have** to use another name to avoid the collision.

The variable introduced in the `is`-expression is definitely assigned only when the predicate is `true`. It means that the `n` variable in the second if-statement is not assigned in the right operand but because the variable is already declared we can use it as the `out` variable in the `int.TryParse` method.

The third aspect mentioned above is the most concerning one. Consider the following code:

```csharp
public void BoxTwice(int n)
{
    if (n is 42) Console.WriteLine("n is 42");
}

```

In most cases the `is`-expression is translated to the `object.Equals(constValue, variable)` (even though the spec says that `operator==` should be used for primitive types):

```csharp
public void BoxTwice(int n)
{
    if (object.Equals(42, n))
    {
        Console.WriteLine("n is 42");
    }
}
```

This code causes 2 boxing allocations that can reasonable affect performance if used in the application's critical path. It used to be the case that `o is null` was causing the boxing allocation if `o` is a nullable value type (see [Suboptimal code for e is null](https://github.com/dotnet/roslyn/issues/13247)) so I really hope that this behavior will be fixed (here is [an issue on github](https://github.com/dotnet/roslyn/issues/20642)).

If the `n` variable is of type `object` the `o is 42` will cause one boxing allocation (for the literal `42`), even though the similar switch-based code would not cause any allocations.

## The `var` patterns in `is`-expressions

The `var` pattern is a special case of the type pattern with one major distinction: the pattern will match any value, even if the value is `null`.

```csharp
public void IsVar(object o)
{
   if (o is var x) Console.WriteLine($"x: {x}");
}
```

`o is object` is `true` when `o` is not `null`, but `o is var x` is always `true`. The compiler knows about that and in the Release mode (*), it removes the if-clause altogether and just leaves the `Console` method call. Unfortunately, the compiler does not warn you that the code is unreachable in the following case: `if (!(o is var x)) Console.WriteLine("Unreachable")`. Hopefully, this will be fixed as well.

(*) It is not clear why the behavior is different in the Release mode only. But I think all the issues falls into the same bucker: the initial implementation of the feature is suboptimal. But based on [this comment](https://github.com/dotnet/roslyn/issues/22654#issuecomment-336329881) by Neal Gafter, this is going to change: "The pattern-matching lowering code is being rewritten from scratch (to support recursive patterns, too). I expect most of the improvements you seek here will come for "free" in the new code. But it will be some time before that rewrite is ready for prime time.".

The lack of `null` check makes this case very special and potentially dangerous. But if you know what exactly is going on you may find this pattern useful. It can be used for introducing a temporary variable inside the expression:

```csharp
public void VarPattern(IEnumerable<string> s)
{
    if (s.FirstOrDefault(o => o != null) is var v 
        && int.TryParse(v, out var n))
    {
        Console.WriteLine(n);
    }
}
```

## `Is`-expression meets "Elvis" operator

There is another use case that I've found very useful. The type pattern matches the value only when the value is not `null`. We can use this "filtering" logic with the null-propagating operator to make a code easier to read:

```csharp
public void WithNullPropagation(IEnumerable<string> s)
{
    if (s?.FirstOrDefault(str => str.Length > 10)?.Length is int length)
    {
        Console.WriteLine(length);
    }
    
    // Similar to
    if (s?.FirstOrDefault(str => str.Length > 10)?.Length is var length2 && length2 != null)
    {
        Console.WriteLine(length2);
    }
    
    // And similar to
    var length3 = s?.FirstOrDefault(str => str.Length > 10)?.Length;
    if (length3 != null)
    {
        Console.WriteLine(length3);
    }
}
```

Note, that the same pattern can be used for both - value types and reference types.

## Pattern matching in the `case` blocks

C# 7 extends the switch statement to use patterns in the case clauses:

```csharp
public static int Count<T>(this IEnumerable<T> e)
{
    switch (e)
    {
        case ICollection<T> c: return c.Count;
        case IReadOnlyCollection<T> c: return c.Count;
        // Matches concurrent collections
        case IProducerConsumerCollection<T> pc: return pc.Count;
        // Matches if e is not null
        case IEnumerable<T> _: return e.Count();
        // Default case is handled when e is null
        default: return 0;
    }
}
```

The example shows the first set of changes to the switch statement.

1. A variable of any type may be used in a switch statement.
2. A case clause can specify a pattern.
3. The order of the case clauses matters. The compiler emits an error if the previous clause matches a base type and the next clause matches a derived type.
4. Non default clauses have an implicit null check (**). In the example before the very last case clause is valid because it matches only when the argument is not `null`.

(**) The very last case clause shows another feature added to C# 7 called "discard" pattern. The name `_` is special and tells the compiler that the variable is not needed. The type pattern in a case clause requires an alias and if you don't need it you can ignore it using `_`.

The next snippet shows another feature of the switch-based pattern matching - an ability to use predicates:

```csharp
public static void FizzBuzz(object o)
{
    switch (o)
    {
        case string s when s.Contains("Fizz") || s.Contains("Buzz"):
            Console.WriteLine(s);
            break;
        case int n when n % 5 == 0 && n % 3 == 0:
            Console.WriteLine("FizzBuzz");
            break;
        case int n when n % 5 == 0:
            Console.WriteLine("Fizz");
            break;
        case int n when n % 3 == 0:
            Console.WriteLine("Buzz");
            break;
        case int n:
            Console.WriteLine(n);
            break;
    }
}
```

This is a weird version of the [FizzBuzz](http://wiki.c2.com/?FizzBuzzTest) problem that processes an `object` instead of just a number.

A switch can have more than one case clause with the same type. If this happens the compiler groups together all type checks to avoid redundant computations:

```csharp
public static void FizzBuzz2(object o)
{
    // All cases can match only if the value is not null
    if (o != null)
    {
        if (o is string s &&
            (s.Contains("Fizz") || s.Contains("Buzz")))
        {
            Console.WriteLine(s);
            return;
        }

        bool isInt = o is int;
        int num = isInt ? ((int)o) : 0;
        if (isInt)
        {
            // The type check and unboxing happens only once per group
            if (num % 5 == 0 && num % 3 == 0)
            {
                Console.WriteLine("FizzBuzz");
                return;
            }
            if (num % 5 == 0)
            {
                Console.WriteLine("Fizz");
                return;
            }
            if (num % 3 == 0)
            {
                Console.WriteLine("Buzz");
                return;
            }

            Console.WriteLine(num);
        }
    }
}
```

But there are two things to keep in mind:
1. The compiler will group together only consecutive type checks and if you'll intermix cases for different types the compiler will generate less optimal code:

```csharp
switch(o)
{
    // The generated code is less optimal:
    // If o is int, then more than one type check and unboxing operation
    // may happen.
    case int n when n == 1: return 1;
    case string s when s == "": return 2;
    case int n when n == 2: return 3;
}
```

The compiler will translate it effectively to the following:

```csharp
if (o is int n && n == 1) return 1;
if (o is string s && s == "") return 2;
if (o is int n2 && n2 == 2) return 3;
```

2. The compiler tries it best to prevent common ordering issues.

```csharp
switch (o)
{
    case int n: return 1;
    // Error: The switch case has already been handled by a previous case.
    case int n when n == 1: return 2;
}
```

But compiler doesn't know that one predicate is stronger than the other and effectively supersedes the next cases:

```csharp
switch (o)
{
    case int n when n > 0: return 1;
    // Will never match, but the compiler won't warn you about it
    case int n when n > 1: return 2;
}
```

## Pattern matching 101
* C# 7 introduced the following patterns: the const pattern, the type pattern, the var pattern and the discard pattern.
* Patterns can be used in `is`-expressions and in case blocks.
* The implementation of the const pattern in `is`-expression for value types is far from perfect from the performance point of view.
* The `var`-pattern always match and you should be careful with them.
* A switch statement can be used for a set of type checks with additional predicates in `when` clauses.