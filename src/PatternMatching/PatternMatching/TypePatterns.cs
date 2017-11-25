using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PatternMatching
{
    public class TypePatterns1
    {
public void TypePatterns(object o)
{
    if (!(o is int n)) Console.WriteLine(n);

            //Console.WriteLine(n);

    if (o is string s && s.Trim() != string.Empty)
        Console.WriteLine("o is not blank");
}

public static bool IsEmpty<T>(IEnumerable<T> e)
{
    return
        (e is ICollection c && c.Count == 0) ||
        (e is IReadOnlyCollection<T> rc && rc.Count == 0) ||
        (e != null && e.Any());
}

        public void ScopeAndDefiniteAssigning(object o)
        {
            if (o is string s && s.Length != 0)
            {
                Console.WriteLine("o is not empty string");
            }

            // Can't use 's' any more. 's' is already declared in the current scope.
            if (o is int n || (o is string s2 && int.TryParse(s2, out n)))
            {
                // can't s
            }
        }
    }
}