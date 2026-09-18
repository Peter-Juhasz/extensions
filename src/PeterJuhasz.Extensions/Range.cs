using System.Numerics;

namespace System;

public readonly record struct Range<T>(T Minimum, T Maximum)
	where T : INumber<T>
{
	public T Length => Maximum - Minimum;

	public bool Contains(T value) => value >= Minimum && value <= Maximum;

	public bool Contains(Range<T> other) => other.Minimum >= Minimum && other.Maximum <= Maximum;

	public bool Intersects(Range<T> other) => Minimum <= other.Maximum && Maximum >= other.Minimum;

	public Range<T> Offset(T offset) => new(Minimum + offset, Maximum + offset);

	public Range<T> ScaleLength(T factor) => new(Minimum, Minimum + Length * factor);
}
