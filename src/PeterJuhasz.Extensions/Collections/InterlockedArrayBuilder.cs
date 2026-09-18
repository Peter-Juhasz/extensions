namespace System.Collections.Concurrent;

public sealed class MultiPageInterlockedArrayBuilder<T>(int pageSize = 16, int maxPages = 10) : IWriteOnlyCollection<T>
{
	private int _lastInsertionIndex = -1;
	private readonly T?[]?[] _pages = new T[maxPages][];

	public int Count => _lastInsertionIndex + 1;

	/// <summary>
	///	Gets the total number of elements the internal data structure can hold.
	/// </summary>
	public int Capacity
	{
		get
		{
			var count = Count;
			if (count == 0)
			{
				return 0;
			}

			return (count / pageSize + 1) * pageSize;
		}
	}

	public int MaximumCapacity => maxPages * pageSize;

	public void Add(T item)
	{
		// move insertion position
		var insertionIndex = Interlocked.Increment(ref _lastInsertionIndex);

		// find insertion position
		var (pageIndex, itemIndexOnPage) = Math.DivRem(insertionIndex, pageSize);
		if (pageIndex >= _pages.Length)
		{
			throw new InvalidOperationException("Cannot add more items than the fixed capacity of the builder.");
		}
		ref var page = ref _pages[pageIndex];
		if (page == null)
		{
			// allocate new page
			var newBuffer = new T[pageSize];
			InterlockedExtensions.Initialize(ref page, newBuffer);
		}

		// add item
		page[itemIndexOnPage] = item;
	}

	public T[] ToArray()
	{
		// check for empty
		var count = Count;
		if (count == 0)
		{
			return [];
		}

		// collect items from all pages
		var result = new T[count];
		var (lastPageIndex, lastItemIndexOnLastPage) = Math.DivRem(_lastInsertionIndex, pageSize);
		var written = 0;

		// copy intermediate pages
		for (var i = 0; i < lastPageIndex; i++)
		{
			var page = _pages[i]!;
			page.AsSpan().CopyTo(result.AsSpan(written)!);
			written += page.Length;
		}

		// copy last page
		var lastPage = _pages[lastPageIndex]!;
		lastPage.AsSpan(0, lastItemIndexOnLastPage + 1).CopyTo(result.AsSpan(written)!);

		return result;
	}

	public void Clear()
	{
		if (_lastInsertionIndex == -1)
		{
			return;
		}

		var (lastPageIndex, lastItemIndexOnLastPage) = Math.DivRem(_lastInsertionIndex, pageSize);

		// copy intermediate pages
		for (var i = 0; i < lastPageIndex; i++)
		{
			var page = _pages[i]!;
			page.AsSpan().Clear();
		}

		// copy last page
		var lastPage = _pages[lastPageIndex]!;
		lastPage.AsSpan(0, lastItemIndexOnLastPage + 1).Clear();

		_lastInsertionIndex = -1;
	}

	public void TrimExcess()
	{
		if (_lastInsertionIndex == -1)
		{
			_pages.AsSpan().Clear();
			return;
		}

		var lastPageIndexInUse = _lastInsertionIndex / pageSize;
		_pages.AsSpan(lastPageIndexInUse + 1).Clear();
	}
}

public sealed class FixedSizeInterlockedArrayBuilder<T> : IWriteOnlyCollection<T>
{
	public FixedSizeInterlockedArrayBuilder(T?[] buffer)
	{
		_buffer = buffer;
	}
	public FixedSizeInterlockedArrayBuilder(int capacity) : this(new T?[capacity])
	{ }

	private int _previousInsertionIndex = -1;
	private readonly T?[] _buffer;

	public int Count => _previousInsertionIndex + 1;

	public int Capacity => _buffer.Length;

	public void Add(T item)
	{
		var insertionIndex = Interlocked.Increment(ref _previousInsertionIndex);
		var buffer = _buffer;
		if (insertionIndex >= buffer.Length)
		{
			throw new InvalidOperationException("Cannot add more items than the fixed capacity of the builder.");
		}

		buffer[insertionIndex] = item;
	}

	public T[] ToArray()
	{
		var count = Count;
		if (count == 0)
		{
			return [];
		}

		var result = new T[count];
		_buffer.AsSpan(0, count).CopyTo(result!);
		return result;
	}

	public void Clear()
	{
		_buffer.AsSpan().Clear();
		_previousInsertionIndex = -1;
	}
}