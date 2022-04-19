
using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace InterpolatedStrings
{
    [InterpolatedStringHandler]
    public struct ContractMessageInterpolatedStringHandler
    {
        // Will delegate all the work here!
        private readonly StringBuilder _builder;

        public ContractMessageInterpolatedStringHandler(int literalLength, int formattedCount, bool predicate, out bool handlerIsValid)
        {
            _builder = null;

            if (predicate)
            {
                // If the predicate is evaluated to 'true', then we don't have to construct a message!
                handlerIsValid = false;
                return;
            }

            handlerIsValid = true;
            _builder = new StringBuilder();
        }

        public void AppendLiteral(string s) => _builder.Append(s);

        public void AppendFormatted<T>(T t) => _builder.Append(t.ToString());

        public void AppendFormatted(ReadOnlySpan<char> value) => _builder.Append(value.ToString());

        public override string ToString() => _builder.ToString();
    }
}
