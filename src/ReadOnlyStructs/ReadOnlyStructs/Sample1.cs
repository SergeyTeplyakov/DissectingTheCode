using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ReadOnlyStructs
{
internal class ReadOnlyEnumerator
{
    private readonly List<int>.Enumerator _enumerator;

    public ReadOnlyEnumerator(List<int> list)
    {
        Contract.Requires(list.Count >= 1);
        _enumerator = list.GetEnumerator();
    }

    public void PrintTheFirstElement()
    {
        _enumerator.MoveNext();
        Console.WriteLine(_enumerator.Current);
    }

    public void PrintTheFirstElement_Decompiled()
    {
        // Defensive copy
        var localEnumerator = _enumerator;
        localEnumerator.MoveNext();

        // Defensive copy (2)
        localEnumerator = _enumerator;
        Console.WriteLine(localEnumerator.Current);
    }
}

    public class TestReadOnlyEnumerator
    {
        [Fact]
        public void Test()
        {
var roe = new ReadOnlyEnumerator(new List<int>{1,2});
roe.PrintTheFirstElement();
        }

//// Async methods cannot have ref or out parameters
//async Task ByInAsync(in string s) => await Task.Yield();
    }
}
