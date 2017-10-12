using System;

namespace PatternMatching
{
    public class Patterns
    {
        public void DiscardPattern(object o)
        {
            //object o = 42L;
            // Just similar to o is int
            if (o is var x)
            {
                Console.WriteLine(x);
            }
        }
    }
}