using System.Numerics;
using System.Runtime.CompilerServices;

namespace System.Collections;

public readonly ref struct ValueBitArray<T>
	where T : unmanaged, IBinaryInteger<T>, 
		IShiftOperators<T, int, T>, IBitwiseOperators<T, T, T>,
		IComparisonOperators<T, T, bool>
{
	public ValueBitArray(Span<T> buffer)
	{
		_buffer = buffer;
	}

	private readonly Span<T> _buffer;

	private static readonly int NumberOfBitsInBucket = Unsafe.SizeOf<T>() * 8;

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
			return (_buffer[bucket] & (T.One << position)) != T.Zero;
		}
		set
		{
			(int bucket, int position) = Math.DivRem(index, NumberOfBitsInBucket);
			_buffer[bucket] = value switch
			{
				true => _buffer[bucket] | (T.One << position),
				false => _buffer[bucket] & ~(T.One << position),
			};
		}
	}

	public bool Any() => _buffer.ContainsAnyExcept(T.Zero);

	public bool IsEmpty() => !Any();

	public bool All() => !_buffer.ContainsAnyExcept(T.AllBitsSet);

	public readonly void Clear() => _buffer.Clear();
}
