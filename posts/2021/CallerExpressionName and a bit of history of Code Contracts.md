# CallerExpressionNameAttribute in C# 10 and a bit of history of Code Contracts

Today I want to continue covering the new C# 10 features that I've started with the [previous post](https://sergeyteplyakov.github.io/Blog/c%2310/2021/11/08/Dissecing-Interpolated-Strings-Improvements-In-CSharp-10.html). This time I want to talk about a very small but really useful (in narrow cases) feature called `CallerExpressionNameAttribute`. And to really appreciate it as well as the changes in the interpolated string I want to talk about [Code Contracts](https://docs.microsoft.com/en-us/dotnet/framework/debug-trace-profile/code-contracts).

Code Contracts is a .NET implementation of [Design by Contract](https://en.wikipedia.org/wiki/Design_by_contract) approach for software design. The key idea behind DbC is that the software components should clearly define their "contracts" with preconditions, postconditions and invariants. 

For instance, arguments are checked with 'preconditons' and the clients are responsible for making sure the preconditions are met. A method checks the preconditions with `Contract.Requires(predicate)` and failure to met the precondition indicates a bug in the client code. 

When the method accepts the passing arguments it "promises" to fulfil it's job by producing the result. The expected result (*) is checked with a postcondition by calling `Contract.Ensures(predicate)` and failure to met the postcondition indicates a bug in the implementation logic.

(*) The 'result' here may be a value returned from the method or some other changes, like a state change, a callback invocation etc.

Object invariants are checked by `Contract.Invariant(predicate)` call and you can think of them as something that must be true during the entire object's lifetime. For instance, that some field is not null or that the sum of two fields has some special value.

But there is another assertion method in Code Contracts called `Contract.Assert(predicate)`. This is a "regular" assersion that checks that something is true at given point of time. For instance, there maybe a check that a local variable is initialized or has a special value etc. The "regular" assertion violation also indicate an issue in the implementation logic. You can think of it as `Debug.Assert` but with the runtime semantic in release builds as well.

A regular .NET aplication also separates differnet "failure" modes as well, but instead of using different assertion methods it may use different exception types. For instance, `ArgumentException`s are used for modeling precondition violation and `InvalidOperationException` indicate that an object invariant was violated.

I do like the idea behind Design by Contract and I do like the conceptual separation that different assertions give me when I design a system. I was interested in this topic long before I joined Microsoft and I was very glad to discover that the team I joined was using Code Contracts extensively in the new build system called Domino (now [BuildXL](https://github.com/microsoft/BuildXL)). I liked the idea so much that I became a main maintainer of [Code Contracts](https://github.com/microsoft/CodeContracts) and I've added support for C# 5 back in 2015 and was maintaining this project for a year or two after that.

Eventually I realized that maintaining this project takes a lot of time and that using Code Contracts in large and high performant application has it's costs as well. The main issue was that the runtime behavior of Code Contracts is implemented based on IL rewriting. It was a very good design idea when the project started because it allows supporting multiple language but the rewriter has to know about the different IL patterns that different compilers use to represents various language features. But once the C# compiler started adding more and more high level language features it became insanely hard to keep up and recognize all the possible IL patterns.

I'm not going to cover how the rewriter worked and how async methods are represented in the IL. But I want to talk about some very handy features every Code Contracts assertion has. Let's look at a simple check:

```csharp
int x = 42;
Contract.Assert(x == -1, string.Format("x is {0}", x)));
```

The rewriter was smart enough to recognize where the predicate is located in the IL and how exactly the message was constructed. So it was rewriting this into something like this:

```csharp
int x = 42;
bool precidate = x == -1;
if (!predicate)
{
    // Its a seudo code. The actual code was more complicated, for instance, a special event was raised here as well.
    throw new ContractException(predicate: "x == -1", userDefinedMessage: string.Format("x is {0}", x));
}
```

Each contract violation had a predicate in a text form and an optional user provided message. It may seems simple, but reconstructing a string-based predicate from the IL code is not easy. And also notice that the message is constructed only when the assertion is violated! These two thing allowed having a more usable error messages and have 0 allocations when the assertions were not violated.

The Code Contracts rewriter was doing a lot of heavy lifting but with C# 10 these two aspects are supported by the C# compiler! We talked about [the interpolated string improvements](https://sergeyteplyakov.github.io/Blog/c%2310/2021/11/08/Dissecing-Interpolated-Strings-Improvements-In-CSharp-10.html) that allow creating a message only when the predicate is false, and with `CallerExpressionNameAttribute` the compiler can create a string representation of a predicate as well!

Let's have a look at the `Contract.Assert` API:


```csharp
public static void Assert(
    [DoesNotReturnIf(false)]
    bool condition,
    [InterpolatedStringHandlerArgument("condition")]ref ContractMessageInterpolatedStringHandler userMessage,
    [CallerArgumentExpression("condition")] string conditionText = "")
```

The last argument is marked with `CallerArgumentExpression` attribute that "tells" the compiler to reconstruct the string representation of the expression that was used with `condition` argument. I.e. if the we write something like `Contract.Assert(collection.First().X > 0)` then the `conditionText` will be `collection.First().X > 0`. 

But why do you need this?

Assertion violations should not be handled in the code programmatically, but they rather just logged or sent to telemetry. In most cases the line number from the stack trace should help figuring out where the error is comming from. But this is not always true. For instance, pdbs can be missing, the source code could've changed and the line numbers can differ from your main branch. And if you have more then one assertion in the method it can be quite hard to figure out exactly what failed. Storing the expression in logs can simplify the analysis a lot but explicitly saying what went wrong.

This attribute is very helpful for different kinds of validation API. It can be something similar to `Contract.Assert` or `Debug.Assert`. Or for null-checking:

```csharp
public static class NullableChecks
{
    [DebuggerStepThrough]
    public static T NotNull<T>([NotNull]this T? value, [CallerArgumentExpression("value")] string paramName = "") where T : notnull
    {
        if (value is null)
        {
            // Getting a name of an actual expression that was used to produce the 'value' parameter
            throw new ArgumentNullException(paramName);
        }

        return value;
    }
}
```

And now you can use this in your code to enforce that a value is not null:
```csharp
var foo = new Foo();
foo.Bar.NotNull(); // will throw ArgumentNullException with argument name 'foo.Bar'.
```

A very similar helper is available in .NET6 - `ArgumentNullException.ThrowIfNull(expression)`, but I actually do like an API using extension methods.

**Can I use this feature with the lower .net framework versons besides .NET 6?**

Like the interpolated strings improvements, the `CallerArgumentExpression` attribute can be used with C# 10+ targeting any framework versions. You just need to define `CallerArgumentExpressionAttribute` yourself in `System.Runtime.CompilerServices` namespace. That's exactly what I did for `RuntimeContracts` (TODO: Add a link!) that target .netstandard2.0.

## Conclusion
`CallerArgumentExpressionAttribute` is a very small feature with a very limit applicability outside of validation API like `Debug.Assert` or `Guard.NotNull`. But I personally very glad that it was added. I remember multiple cases when the stack trace information was either unavailable, out of date or just plainly wrong, and not having an expressions that violated the assertion was causing a massive headache during postmortem analysis.

 But I think that the main purpose of this feature is actually to be a building block for the upcoming ['simplified parameter null validation'](https://github.com/dotnet/csharplang/issues/2145) feature that we can expect in the upcoming C# version:

```csharp
public void FooBar(string s!!) {}
```

Will be equivalent to:

```csharp
public void FooBar(string s)
{
    ArgumentNullException.ThrowIfNull(s);
}
```