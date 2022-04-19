
using System;
using System.Runtime.CompilerServices;

namespace InterpolatedStrings
{
    [InterpolatedStringHandler]
    public ref struct ContractMessageInterpolatedStringHandler
    {
        // Will delegate all the work here!
        private readonly DefaultInterpolatedStringHandler _handler;

        public ContractMessageInterpolatedStringHandler(int literalLength, int formattedCount, bool predicate, out bool handlerIsValid)
        {
            _handler = default;

            if (predicate)
            {
                // If the predicate is evaluated to 'true', then we don't have to construct a message!
                handlerIsValid = false;
                return;
            }

            handlerIsValid = true;
            _handler = new DefaultInterpolatedStringHandler(literalLength, formattedCount);
        }

        public void AppendLiteral(string s) => _handler.AppendLiteral(s);

        public void AppendFormatted<T>(T t) => _handler.AppendFormatted(t);

        //public void AppendFormat<T>(Span<T> span) =

        public void AppendFormatted(MyStruct ms)
        {
            // Special casing a custom struct
            Console.WriteLine("AppendFOrmated with MyStruct");
        }

        public override string ToString() => _handler.ToStringAndClear();
    }
}
