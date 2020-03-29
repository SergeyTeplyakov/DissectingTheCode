using System;
using System.Collections.Generic;

namespace Tuples
{
    public class BasicSamples
    {
        public static void Foo(System.ValueTuple<int, int> x)
        {
            
        }

        public static void Sample()
        {
// Constructing the tuple instance
var tpl = (1, 2);
            
// Using tuples with a dictionary
var d = new Dictionary<(int x, int y), (byte a, short b)>();

// Tuples with different names are compatible
d.Add(tpl, (a: 3, b: 4));

// Tuples have value semantic
if (d.TryGetValue((1, 2), out var r))
{
    // Deconstructing the tuple ignoring the first element
    var (_, b) = r;
                
    // Using named syntax as well as predefined name
    Console.WriteLine($"a: {r.a}, b: {r.Item2}");
}

Foo((x: 1, y: 2));
            tpl = (a: 1, b: 2);

            (int x, int y) x = (Y: 1, 2);

            //bool bbb = tpl == (1, 2);   
            var r33 = (a: 1, b: 2).Equals((a: 1, b: 2));
        }
    }
}