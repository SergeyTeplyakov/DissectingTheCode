using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace PatternMatching
{
    public static class ConstPattern
    {
        public static int Foo(object o)
        {
switch (o)
{
    case int n when n > 0: return 1;
    // Will never match, but the compiler won't warn you about it
    case int n when n > 1: return 2;
}


        }

    }
}