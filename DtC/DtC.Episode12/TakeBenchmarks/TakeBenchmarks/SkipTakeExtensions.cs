using System.Collections;

namespace TakeBenchmarks;

/// <summary>
/// Combined Skip/Take enumerable to avoid creating multiple wrapper enumerables
/// when chaining Skip().Take(). Mutates state when additional SkipFast/TakeFast
/// are applied prior to enumeration.
/// </summary>
internal sealed class SkipTakeEnumerable<T> : IEnumerable<T>
{
    internal IEnumerable<T> _source; // original source
    internal int _skip;              // total items to skip
    internal int? _take;             // max items to take (null == unlimited)

    internal SkipTakeEnumerable(IEnumerable<T> source, int skip, int? take)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _skip = Math.Max(0, skip);
        _take = take is null ? null : Math.Max(0, take.Value);
    }

    public IEnumerator<T> GetEnumerator() => new Enumerator(this);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal struct Enumerator : IEnumerator<T>
    {
        private readonly IEnumerable<T> _source;
        private readonly int _skip;
        private readonly int? _take;
        private IEnumerator<T>? _inner;
        private int _taken;
        private T? _current;

        public Enumerator(SkipTakeEnumerable<T> parent)
        {
            _source = parent._source;
            _skip = parent._skip;
            _take = parent._take;
            _inner = null;
            _taken = 0;
            _current = default;
        }

        public T Current => _current!;
        object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (_take.HasValue && _taken >= _take.Value)
                return false;

            if (_inner == null)
            {
                _inner = _source.GetEnumerator();
                // Skip phase
                int skipped = 0;
                while (skipped < _skip && _inner.MoveNext())
                {
                    skipped++;
                }
                if (skipped < _skip)
                {
                    // Source shorter than skip count
                    return false;
                }
            }

            if (!_inner.MoveNext())
                return false;

            _current = _inner.Current;
            _taken++;
            return true;
        }

        public void Reset() => throw new NotSupportedException();
        public void Dispose() => _inner?.Dispose();
    }
}

public static class SkipTakeExtensions
{
    /// <summary>
    /// Combines with TakeFast to reuse a single wrapper. Additional SkipFast calls accumulate.
    /// </summary>
    public static IEnumerable<T> SkipFast<T>(this IEnumerable<T> source, int count)
    {
        if (source is SkipTakeEnumerable<T> existing)
        {
            existing._skip += Math.Max(0, count);
            return existing;
        }
        return new SkipTakeEnumerable<T>(source, count, null);
    }

    /// <summary>
    /// If applied to a SkipFast result, adjusts internal take limit instead of allocating new wrapper.
    /// When multiple TakeFast calls are chained, we keep the smallest window (like standard LINQ semantics).
    /// </summary>
    public static IEnumerable<T> TakeFast<T>(this IEnumerable<T> source, int count)
    {
        count = Math.Max(0, count);
        if (source is SkipTakeEnumerable<T> existing)
        {
            if (existing._take.HasValue)
            {
                existing._take = Math.Min(existing._take.Value, count);
            }
            else
            {
                existing._take = count;
            }
            return existing;
        }
        return new SkipTakeEnumerable<T>(source, 0, count);
    }
}
