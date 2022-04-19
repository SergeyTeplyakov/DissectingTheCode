
using System;

namespace InterpolatedStrings
{
    public readonly struct Point : ISpanFormattable
    {
        public int X { get; }
        public int Y { get; }
        public Point(int x, int y) => (X, Y) = (x, y);

        public override string ToString()
        {
            return ToString(format: null, formatProvider: null);
        }

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
        {
            return destination.TryWrite($"X={X}, Y={Y}", out charsWritten);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Create(formatProvider, $"X={X}, Y={Y}");
        }
    }


    public struct MySpanFormattable : ISpanFormattable
    {
        public override string ToString()
        {
            return "My result";
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return "My result";
        }

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
        {
            Console.WriteLine("TryFormat");
            var source = "My result".AsSpan();
            if (destination.Length < source.Length)
            {
                charsWritten = 0;
                return false;
            }

            source.CopyTo(destination);
            charsWritten = source.Length;
            return true;
        }
    }
}
