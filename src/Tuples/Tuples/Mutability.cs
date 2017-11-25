using System;
using System.Collections.Generic;

namespace Tuples
{
    public class Mutability
    {
        public void MutableEnumerator()
        {
var x = new { Items = new List<int> { 1, 2, 3 }.GetEnumerator() };
while (x.Items.MoveNext())
{
    Console.WriteLine(x.Items.Current);
}
        }
        public void MutabilityTest()
        {
var tpl = (x: 1, y: 2);
var hs = new HashSet<(int x, int y)>();
hs.Add(tpl);

tpl.x++;
Console.WriteLine(hs.Contains(tpl)); // false
        }
    }
}