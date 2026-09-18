namespace System.Buffers;

public ref struct SpanBufferWriter<T> : IBufferWriter<T>
{
	public SpanBufferWriter(Span<T> span)
	{
		_span = span;
		_index = 0;
	}

	private readonly Span<T> _span;
	private int _index;

	public void Write(T value)
	{
		if (_index >= _span.Length)
			throw new InvalidOperationException("Span is full.");

		_span[_index++] = value;
	}

	public void Write(ReadOnlySpan<T> values)
	{
		if (_index + values.Length > _span.Length)
			throw new InvalidOperationException("Span is full.");

		values.CopyTo(_span[_index..]);
		_index += values.Length;
	}

	public readonly Span<T> GetSpan(int minimumLength)
	{
		if (_index + minimumLength > _span.Length)
			throw new InvalidOperationException("Span is full.");

		return _span[_index..];
	}

	public readonly Memory<T> GetMemory(int minimumLength)
	{
		throw new NotSupportedException();
	}

	public readonly ReadOnlySpan<T> WrittenSpan => _span[.._index];

	public readonly int WrittenLength => _index;

	public readonly Span<T> Remaining => _span[_index..];

	public readonly int RemainingLength => _span.Length - _index;

	public void Advance(int count)
	{
		if (_index + count > _span.Length)
			throw new InvalidOperationException("Span is full.");

		_index += count;
	}

	public readonly void Clear()
	{
		_span[.._index].Clear();
	}

	public readonly T[] ToArray()
	{
		var array = new T[_index];
		_span[.._index].CopyTo(array);
		return array;
	}
}
