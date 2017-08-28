# Dissecting the managed arrays

Short introduction, like why it matters.

Arrays considered one of the main and crucial building blocks for any language. 

## Array covariance. WHY?!?!?
TODO: add a picture, like Jacky

Mention, that the reason is the silliest possible you can imagine: Java had it!

## Array's internal structure
Two cases: for reference types and for value types.

## Having fun with C-like union types
As you may already know, you may change the content of an array but you can't change a length or an element type of an array. You can't do that normally, but you can if you really want to.

I wanted to show this technique for a while but didn't have a chance to show it in my previous posts. So I've decided, why not to demonstrate it here and learn some stuff about the array internals.

It could be a bit surprising for some of you, but developers can actually control the layout of the object using [`StructLayoutAttribute`](https://msdn.microsoft.com/en-us/library/system.runtime.interopservices.structlayoutattribute%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396). This feature is very useful for structs whe they're used in interop scenarios. But nothing prevents from using the same attribute for classes as well.

Here is a basic example: let suppose we really want to break an encapsulation and change the private field of an object but without using reflection. To do that we need to have a new type with the same layout as the target one but with accessible modifiers:

```csharp
// Class that we would like to hack
public sealed class FooBar
{
    private int _state = 42;
    public int State => _state;
}

internal sealed class FooBarOpen
{
    // Name could be different, it doesn't matter
    public int State;
}

[StructLayout(LayoutKind.Explicit)]
internal sealed class ForBarHack
{
    [FieldOffset(0)]
    public readonly FooBar FooBar;
    [FieldOffset(0)]
    public readonly FooBarOpen FooBarOpen;

    public FooBarHack(FooBar fooBar)
    {
        FooBar = fooBar;
    }
}
```

Now, `FooBarHack` has two fields that stored at the same offset. Basically, we just crated a c-style union type when the same memory location is shared by two different types.

And now, if we'll create an instance of the `FooBarHack` type we would be able to change purely immutable instance of the type `FooBar`:

```csharp
var fooBar = new FooBar();
Console.WriteLine(fooBar.State); // 42
var hack = new FooBarHack(fooBar);
hack.FooBarOpen.State = -1;

Console.WriteLine(fooBar.State); // -1! We've changed it!
```

## Performance implications of the old and unfortunate decision
Show the difference.
Show the trick with the simple wrapper.