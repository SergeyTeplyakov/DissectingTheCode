# Nullable types arithmetic and null-coalescing operator precedence

Here is a simple question for you: which version of a `GetHashCode()` is correct and what the performance impact incorrect version would have?

```csharp
public struct Struct1
{
    public int N { get; }
    public string S { get; }
    public Struct1(int n, string s = null) { N = n; S = s; }

    public override int GetHashCode() => 
        N ^ 
        S?.GetHashCode() ?? 0;

    public override bool Equals(object obj) => 
        obj is Struct1 other && N == other.N && string.Equals(S, other.S);
}

public struct Struct2
{
    public int N { get; }
    public string S { get; }
    public Struct2(int n, string s = null) { N = n; S = s; }

    public override int GetHashCode() => 
        S?.GetHashCode() ?? 0 ^
        N;

    public override bool Equals(object obj) => 
        obj is Struct1 other && N == other.N && string.Equals(S, other.S);
}
```

The structs are not perfect (they don't implement `IEquatable<T>`) but this is not the point. The only difference between the two is the `GetHashCode()` implementation:

```csharp
// Struct 1
public override int GetHashCode() => 
        N ^ 
        S?.GetHashCode() ?? 0;

// Struct 2
public override int GetHashCode() => 
        S?.GetHashCode() ?? 0 ^
        N;

```

Let's check the behavior using the following simple benchmark:

```csharp
private const int count = 10000;
private static Struct1[] _arrayStruct1 =
    Enumerable.Range(1, count).Select(n => new Struct1(n)).ToArray();
private static Struct2[] _arrayStruct2 =
    Enumerable.Range(1, count).Select(n => new Struct2(n)).ToArray();

[Benchmark]
public int Struct1() => new HashSet<Struct1>(_arrayStruct1).Count;

[Benchmark]
public int Struct2() => new HashSet<Struct2>(_arrayStruct2).Count;
```

The results are:

```
  Method |         Mean |        Error |       StdDev |
-------- |-------------:|-------------:|-------------:|
 Struct1 | 736,298.4 us | 4,224.637 us | 3,745.030 us |
 Struct2 |     353.8 us |     2.382 us |     1.989 us |
```

Wow! The `Struct2` is 2000 times faster! This definitely means that the second implementation is correct and the first one is not! Right? Actually, not.

Both implementations are incorrect and just by an accident the second one "works better" in this particular case. Let's take closer look at the `GetHashCode` method for `Struct1`:

```csharp
public override int GetHashCode() => N ^ S?.GetHashCode() ?? 0;
```

You may think that this statement is equivalent to `N ^ (S?.GetHashCode() ?? 0)` but it is actually equivalent to `(N ^ S?.GetHashCode()) ?? 0`:

```csharp
public override int GetHashCode()
{
    int? num = N ^ ((S != null) ? new int?(S.GetHashCode()) : null);
    
    if (num == null)
        return 0;

    return num.GetValueOrDefault();
}
```

Now it is way more obvious why the `Struct1` is so slow: when `S` property is `null` (which is always the case in this example), the hash code is `0` regardless of the `N` because `N ^ (int?)null` is `null`. And trying to add 10000 values with the same hash code effectively converts the hash set into a linked list drastically affecting the performance.

But the second implementation is also wrong: 

```csharp
public override int GetHashCode() => S?.GetHashCode() ?? 0 ^ N;
```

Is equivalent to:

```csharp
public override int GetHashCode()
{
	if (S == null)
	{
		return 0 ^ N;
	}

	return S.GetHashCode();
}
```

In this particular case this implementation gives us way better distribution, but just because the `S` is always null. In other scenario this hash function could be terrible and could give the same value for a large set of instances as well.

## Conclusion

There are two reasons why the expression `N ^ S?.GetHashCode()??0` gives us not what we could expect. C# supports the notion of lifted operators that allows to mix nullable and non-nullable values together in one expression: `42 ^ (int?)null` is `null`. Second, the priority of null-coalescing operator (`??`) is lower then the priority of `^`.

Operator precedence for some operators is so obvious that we can omit explicit parens around them. In case of null-coalescing operator the precedence could be tricky so use parenthesis to clarify your meaning.

## Additional references
* [What exactly does 'lifted' mean?](https://blogs.msdn.microsoft.com/ericlippert/2007/06/27/what-exactly-does-lifted-mean/) by Eric Lippert
* [What are lifted operators?](https://stackoverflow.com/questions/3370110/what-are-lifted-operators)