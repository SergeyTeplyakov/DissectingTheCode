using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PatternMatching
{
    public class VarPatterns
    {
public void VarPattern(IEnumerable<string> s)
{
    if (s.FirstOrDefault(o => o != null) is var v 
        && int.TryParse(v, out var n))
    {
        Console.WriteLine(n);
    }
}
    }
}