
using System;
using System.Runtime.CompilerServices;

namespace InterpolatedStrings
{
    public static class Contract
    {
        // "Telling" the compiler to pass the 'predicate' parameter to the handler.
        public static void Requires(bool predicate, [InterpolatedStringHandlerArgument("predicate")]ref ContractMessageInterpolatedStringHandler handler, [CallerArgumentExpression("predicate")] string expression = null!)
        {
            if (!predicate)
            {
                throw new Exception($"Precondition failed! Expression: {expression}. Message:{handler.ToString()}");
            }
        }
    }
}
