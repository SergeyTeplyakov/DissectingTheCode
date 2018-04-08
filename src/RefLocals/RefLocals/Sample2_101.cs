using System;
using NUnit.Framework;

namespace RefLocals
{
    [TestFixture]
    public class Sample2_101
    {
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

class EncapsulationWentWrong
{
    private readonly Guid _guid;
    private int _x;

    public EncapsulationWentWrong(int x) => _x = x;

    // Return an alias to the private field. No encapsulation any more.
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
    }
}