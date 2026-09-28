using System.Runtime.CompilerServices;

namespace System.Collections.Generic;

public ref struct SpanList<T>(Span<T> span, bool clear = false)
{
	private readonly Span<T> span = span;
	private readonly bool clear = clear;

	public readonly int Capacity => span.Length;

	private int count = 0;

	public readonly int Count => count;

	public readonly ReadOnlySpan<T> WrittenSpan => span[..count];

	public readonly Span<T> AsSpan() => span[..count];

	public readonly ref T this[int index]
	{
		get
		{
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, count);
			return ref span[index];
		}
	}

	public readonly ref T this[Index index] => ref this[index.GetOffset(count)];

	public readonly ReadOnlySpan<T> this[Range range] => WrittenSpan[range];


	public void Add(T item)
	{
		ThrowIfFull();

		span[count++] = item;
	}

	public bool TryAdd(T item)
	{
		if (count >= span.Length)
		{
			return false;
		}
		span[count++] = item;
		return true;
	}

	public void AddRange(ReadOnlySpan<T> items)
	{
		if (items.Length == 0)
		{
			return;
		}
		if (count + items.Length > span.Length)
		{
			throw new InvalidOperationException("SpanList is full.");
		}
		items.CopyTo(span[count..]);
		count += items.Length;
	}

	public void RemoveAt(int index)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, count);
		if (index < count - 1)
		{
			span[(index + 1)..count].CopyTo(span[index..]);
		}
		if (clear)
		{
			span[count - 1] = default!;
		}
		count--;
	}

	public void RemoveAt(Index index) => RemoveAt(index.GetOffset(count));

	public void RemoveRange(Range range)
	{
		var (start, length) = range.GetOffsetAndLength(count);
		if (length == 0) return;

		int end = start + length;
		if (end < count)
		{
			span[end..count].CopyTo(span[start..]);
		}

		if (clear)
		{
			span[(count - length)..count].Clear();
		}

		count -= length;
	}

	public int RemoveAll(Predicate<T> predicate)
	{
		int newCount = 0;
		for (int i = 0; i < count; i++)
		{
			if (!predicate(span[i]))
			{
				if (newCount != i)
				{
					span[newCount] = span[i];
				}
				newCount++;
			}
		}

		if (clear)
		{
			span[newCount..count].Clear();
		}

		var removedCount = count - newCount;
		count = newCount;
		return removedCount;
	}

	public bool Remove(T item, IEqualityComparer<T>? comparer = null)
	{
		int index = IndexOf(item, comparer);
		if (index < 0)
		{
			return false;
		}

		RemoveAt(index);
		return true;
	}

	public void Insert(int index, T item)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThan(index, count);
		ThrowIfFull();
		if (index < count)
		{
			span[index..count].CopyTo(span[(index + 1)..]);
		}
		span[index] = item;
		count++;
	}

	public void InsertRange(int index, ReadOnlySpan<T> items)
	{
		if (items.Overlaps(span))
			throw new ArgumentException("Cannot insert from the list's own buffer.", nameof(items));

		ArgumentOutOfRangeException.ThrowIfGreaterThan(index, count);
		if (items.Length == 0)
		{
			return;
		}
		if (count + items.Length > span.Length)
		{
			throw new InvalidOperationException("SpanList is full.");
		}
		if (index < count)
		{
			span[index..count].CopyTo(span[(index + items.Length)..]);
		}
		items.CopyTo(span[index..]);
		count += items.Length;
	}

	public void Clear()
	{
		if (clear)
		{
			span[..count].Clear();
		}

		count = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private readonly void ThrowIfFull()
	{
		if (count >= span.Length)
		{
			throw new InvalidOperationException("SpanList is full.");
		}
	}


	public readonly ReadOnlySpan<T>.Enumerator GetEnumerator() => WrittenSpan.GetEnumerator();


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool Any() => count > 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool Any(Func<T, bool> predicate) => WrittenSpan.Any(predicate);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool All(Func<T, bool> predicate) => WrittenSpan.All(predicate);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly int IndexOf(T item, IEqualityComparer<T>? comparer = null) => WrittenSpan.IndexOf(item, comparer);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool Contains(T item, IEqualityComparer<T>? comparer = null) => WrittenSpan.Contains(item, comparer);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly void CopyTo(Span<T> destination) => WrittenSpan.CopyTo(destination);
}
