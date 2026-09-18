namespace System.Buffers;

public sealed class MemoryPoolBufferWriter<T>(MemoryPool<T> pool) : IBufferWriter<T>, IDisposable
{
	private int _position;
	private IMemoryOwner<T>? _owner;

	private const int MinimumBufferSize = 8192;

	public void Advance(int count)
	{
		if (_owner == null)
		{
			throw new InvalidOperationException("No memory has been requested. Call GetMemory or GetSpan before calling Advance.");
		}

		ArgumentOutOfRangeException.ThrowIfNegative(count);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(count, _owner.Memory.Length - _position);
		_position += count;
	}

	public Memory<T> GetMemory(int sizeHint = 0)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(sizeHint);

		if (sizeHint == 0)
		{
			sizeHint = 1;
		}

		if (_owner == null)
		{
			_owner = pool.Rent(Math.Max(sizeHint, MinimumBufferSize));
			return _owner.Memory;
		}

		if (_position + sizeHint > _owner.Memory.Length)
		{
			var newSize = Math.Max(_position + sizeHint, _owner.Memory.Length * 2);
			var newOwner = pool.Rent(newSize);
			_owner.Memory[.._position].CopyTo(newOwner.Memory);
			_owner.Dispose();
			_owner = newOwner;
		}

		return _owner.Memory[_position..];
	}

	public Span<T> GetSpan(int sizeHint = 0) => GetMemory(sizeHint).Span;


	public ReadOnlyMemory<T> WrittenMemory
	{
		get
		{
			if (_owner == null)
			{
				return ReadOnlyMemory<T>.Empty;
			}

			return _owner.Memory[.._position];
		}
	}

	public ReadOnlySpan<T> WrittenSpan => WrittenMemory.Span;

	public void Dispose()
	{
		if (_owner is not null)
		{
			_owner.Dispose();
			_owner = null;
		}
	}
}
