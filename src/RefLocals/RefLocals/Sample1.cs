﻿using System.Collections.Generic;
using NUnit.Framework;

namespace RefLocals.Sample1
{
    public struct Mutable
    {
        private int _x;
        public Mutable(int x) => _x = x;

        public int X => _x;

        public void IncrementX() { _x++; }
    }

    [TestFixture]
    public class SampleTestCases
    {
[Test]
public void CheckMutability()
{
    var ma = new[] {new Mutable(1)};
    ma[0].IncrementX();
    // X has been changed!
    Assert.That(ma[0].X, Is.EqualTo(2));

    var ml = new List<Mutable> {new Mutable(1)};
    ml[0].IncrementX();
    // X hasn't been changed!
    Assert.That(ml[0].X, Is.EqualTo(1));
}

[Test]
public void CheckMutabilityForNaiveImmutableList()
{
    var ml = new NaiveImmutableList<Mutable>(new Mutable(1));
    ml[0].IncrementX();
    // X has been changed, right?
    Assert.That(ml[0].X, Is.EqualTo(2));
}
    }
}