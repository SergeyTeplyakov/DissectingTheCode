using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PatternMatching
{
    public class SwitchPatternns
    {
public int OtherPatterns(object o)
{
switch (o)
{
    case int n when n > 0: return 1;
    // Will never match, but the compiler won't warn you about it
    case int n when n > 1: return 2;
}
}
public void SwitchBasedPatternMatching(object o)
{
    if (o != null)
    {
        bool isInt1 = o is int;
        int num = isInt1 ? ((int)o) : 0;
        if (isInt1 && num == 1)
        {
            Console.WriteLine("1");
            return;
        }
        string text;
        if ((text = (o as string)) != null)
        {
            Console.WriteLine("s");
            return;
        }
        bool isInt2 = o is int;
        num = (isInt2 ? ((int)o) : 0);
        if (isInt2 && num == 2)
        {
            Console.WriteLine("2");
        }
    }
}
        public void S1witchBasedPatternMatching2(object o)
{
    if ( o != null)
    {
        bool isInt = o is int;
        int num = isInt ? ((int)o) : 0;
        if (isInt)
        {
            if (num == 1)
            {
                Console.WriteLine("1");
                return;
            }
            if (num == 2)
            {
                Console.WriteLine("2");
                return;
            }
        }
        string text;
        if ((text = (o as string)) != null)
        {
            Console.WriteLine("s");
        }
    }
}
        public static void FizzBuzz(object o)
{
    switch (o)
    {
        case string s when s.Contains("Fizz") || s.Contains("Buzz"):
            Console.WriteLine(s);
            break;
        case int n when n % 5 == 0 && n % 3 == 0:
            Console.WriteLine("FizzBuzz");
            break;
        case int n when n % 5 == 0:
            Console.WriteLine("Fizz");
            break;
        case int n when n % 3 == 0:
            Console.WriteLine("Buzz");
            break;
        case int n:
            Console.WriteLine(n);
            break;
    }
}

public static void FizzBuzz2(object o)
{
    if (o != null)
    {
        if (o is string s &&
            (s.Contains("Fizz") || s.Contains("Buzz")))
        {
            Console.WriteLine(s);
            return;
        }

        bool isInt = o is int;
        int num = isInt ? ((int)o) : 0;
        if (isInt)
        {
            if (num % 5 == 0 && num % 3 == 0)
            {
                Console.WriteLine("FizzBuzz");
                return;
            }
            if (num % 5 == 0)
            {
                Console.WriteLine("Fizz");
                return;
            }
            if (num % 3 == 0)
            {
                Console.WriteLine("Buzz");
                return;
            }

            Console.WriteLine(num);
        }
    }
}


        public static int Count<T>(IEnumerable<T> e)
{
    switch (e)
    {
        case ICollection<T> c: return c.Count;
        case IReadOnlyCollection<T> c: return c.Count;
        // Matches concurrent collections
        case IProducerConsumerCollection<T> pc: return pc.Count;
        // Matches if e is not null
        case IEnumerable<T> _: return e.Count();
        // Default case is handled when the e is null
        default: return 0;
    }
}
    }
}