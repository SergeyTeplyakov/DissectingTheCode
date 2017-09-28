using System;

namespace LocalFunctions
{
    public class ImplicitClosure
    {
        public int ImplicitAllocation(int arg)
        {
            if (arg == int.MaxValue)
            {
                // This code is effectively unreachable
                Func<int> a = () => arg;
            }

            int local = 42;
            return Local();

            int Local() => local;
        }
    }
}