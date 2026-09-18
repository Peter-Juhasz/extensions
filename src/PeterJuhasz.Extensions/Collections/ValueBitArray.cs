namespace System.Collections;

public readonly ref struct ValueBitArray
{
	public ValueBitArray(Span<ulong> buffer)
	{
		_buffer = buffer;
	}

	private readonly Span<ulong> _buffer;
	private const int NumberOfBitsInBucket = sizeof(ulong) * 8;

	public static int GetRequiredBucketCount(int count)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(count, 0, nameof(count));

		if (count == 0)
		{
			return 0;
		}

		return (count + NumberOfBitsInBucket - 1) / NumberOfBitsInBucket;
	}

	public int Capacity => _buffer.Length * NumberOfBitsInBucket;

	public bool this[int index]
	{
		get
		{
			(int bucket, int position) = Math.DivRem(index, NumberOfBitsInBucket);
			return (_buffer[bucket] & (1ul << position)) > 0ul;
		}
		set
		{
			(int bucket, int position) = Math.DivRem(index, NumberOfBitsInBucket);
			_buffer[bucket] = value switch
			{
				true => _buffer[bucket] | (1ul << position),
				false => _buffer[bucket] & ~(1ul << position),
			};
		}
	}

	public bool Any() => _buffer.ContainsAnyExcept(0UL);

	public bool IsEmpty() => !Any();

	public bool All()
	{
		for (var i = 0; i < _buffer.Length - 1; i++)
		{
			if (_buffer[i] != ulong.MaxValue)
			{
				return false;
			}
		}

		if (_buffer.Length > 0 && _buffer[^1] != (ulong.MaxValue >> (NumberOfBitsInBucket - Capacity % NumberOfBitsInBucket)))
		{
			return false;
		}

		return true;
	}

	public readonly void Clear() => _buffer.Clear();
}