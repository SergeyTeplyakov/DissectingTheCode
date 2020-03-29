using System;

namespace PatternMatching
{
    public class DiscardPattern1
    {
public void DiscardPattern(object o)
{
    //if (o is string _)
    //    // The name '_' does not exists in the current context
    //    Console.WriteLine(_);
}

public void VarPattern(object o)
{
    if (o?.ToString().Length is var length)
        // length is int?
        Console.WriteLine(length);
}
    }
}