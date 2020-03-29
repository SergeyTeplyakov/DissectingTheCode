using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Tuples
{
    public class TupleElementNamesAttribute : Attribute
    {
        public TupleElementNamesAttribute(string[] names)
        {
        }
    }
    public class TupleNames
    {
public (int a, int b) Foo1((int c, int d) a) => a;

[return: TupleElementNames(new[] { "a", "b" })]
public ValueTuple<int, int> Foo(
    [TupleElementNames(new[] { "c", "d" })] ValueTuple<int, int> a)
{
    return a;
}

public void NameInference(int x, int y)
{
    // (int x, int y)
    var tpl = (x, y);

    var a = new {X = x, Y = y};

    // (int X, int y)
    var tpl2 = (a.X, a.Y);
}

        public void Warnings()
        {
// Ok: tuple literal can skip element names
(int x, int y) tpl = (1, 2);

// Warning: The tuple element 'a' is ignored because a different name
// or no name is specified by the target type '(int x, int y)'.
tpl = (a:1, b:2);

// Ok: tuple deconstruction ignore element names
var (a, b) = tpl;

// x: 2, y: 1. Tuple names are ignored
var (y, x) = tpl;

(int x, int y) Foo() => (1,2);
        }

public abstract class Base
{
    public abstract (int a, int b) Foo();
    public abstract (int, int) Bar();
}

//public class Derived : Base
//{
//    // Error: Cannot change tuple element names when overriding method
//    public override (int c, int d) Foo() => (1, 2);
//    // Error: Cannot change tuple element names when overriding method
//    public override (int a, int b) Bar() => (1, 2);
//}
    }
}