namespace System.Buffers;

public ref struct MemoryBufferWriter<T>(Memory<T> memory) : IBufferWriter<T>
{
	private int _position;

	public void Advance(int count)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(count);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(count, memory.Length - _position);
		_position += count;
	}

	public readonly Memory<T> GetMemory(int sizeHint = 0)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(sizeHint);

		if (sizeHint == 0)
		{
			sizeHint = 1;
		}

		if (_position + sizeHint > memory.Length)
		{
			throw new InvalidOperationException("Not enough space in the buffer.");
		}

		return memory[_position..];
	}

	public readonly Span<T> GetSpan(int sizeHint = 0) => GetMemory(sizeHint).Span;


	public readonly ReadOnlyMemory<T> WrittenMemory => memory[.._position];

	public readonly ReadOnlySpan<T> WrittenSpan => WrittenMemory.Span;
}
