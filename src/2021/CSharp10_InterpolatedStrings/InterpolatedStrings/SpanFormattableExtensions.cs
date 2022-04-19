
using System;

namespace InterpolatedStrings
{
    public static class SpanFormattableExtensions
    {
        public static bool TryFormat<T>(this T spanFormattable, Span<char> destination, out int charsWritten) where T : ISpanFormattable
        {
            return spanFormattable.TryFormat(destination, out charsWritten, format: default, provider: null);
        }
    }
}
