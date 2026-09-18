namespace System.Collections;

public sealed class RangeMarkerMap(int fullLength, int capacity = 1)
{
	private Range[] _ranges = new Range[capacity];
	private int _count = 0;

	public void Add(Range range)
	{
		var (offset, _) = range.GetOffsetAndLength(fullLength);
		var insertionIndex = -1;

		for (var i = 0; i < _count; i++)
		{
			var item = _ranges[i];

			// range is already included in the map, no need to add it
			if (Includes(item, range))
			{
				return;
			}

			// merge with existing range if it overlaps
			if (Overlaps(range, item))
			{
				_ranges[i] = Merge(range, item);
				MergeWhileOverlaps(i);
				SwapToOrder(i);
				return;
			}

			// find the insertion index for the new range
			var (itemOffset, itemLength) = item.GetOffsetAndLength(fullLength);
			if (offset < itemOffset)
			{
				insertionIndex = i;
				break;
			}
		}

		if (insertionIndex == -1)
		{
			insertionIndex = _count;
		}

		// backing storage is full, it needs to grow
		if (_count == _ranges.Length)
		{
			var newRanges = new Range[_ranges.Length * 2];
			_ranges.AsSpan().CopyTo(newRanges);
			_ranges = newRanges;
		}

		// shift existing ranges to make space for the new range
		if (insertionIndex < _count)
		{
			_ranges.AsSpan(insertionIndex, _count - insertionIndex).CopyTo(_ranges.AsSpan(insertionIndex + 1));
		}

		// insert the new range
		_ranges[insertionIndex] = range;
		_count++;
	}

	private void SwapToOrder(int index)
	{
		for (int i = index; i > 0; i--)
		{
			var item = _ranges[i];
			var previous = _ranges[i - 1];

			var (itemOffset, _) = item.GetOffsetAndLength(fullLength);
			var (previousOffset, _) = previous.GetOffsetAndLength(fullLength);

			if (itemOffset < previousOffset)
			{
				(_ranges[i], _ranges[i - 1]) = (_ranges[i - 1], _ranges[i]);
			}
			else
			{
				break;
			}
		}
	}

	private void MergeWhileOverlaps(int index)
	{
		while (index < _count - 1)
		{
			var current = _ranges[index];
			var next = _ranges[index + 1];
			if (Overlaps(current, next))
			{
				_ranges.AsSpan(index + 1, _count - index - 1).CopyTo(_ranges.AsSpan(index));
				_ranges[index] = Merge(current, next);
				_count--;
			}
			else
			{
				break;
			}
		}
	}

	private bool Includes(Range wider, Range narrower)
	{
		var (offset1, length1) = wider.GetOffsetAndLength(fullLength);
		var (offset2, length2) = narrower.GetOffsetAndLength(fullLength);
		return offset1 <= offset2 && offset1 + length1 >= offset2 + length2;
	}

	private bool Overlaps(Range range1, Range range2)
	{
		var (offset1, length1) = range1.GetOffsetAndLength(fullLength);
		var (offset2, length2) = range2.GetOffsetAndLength(fullLength);
		return (offset1 <= offset2 && offset1 + length1 >= offset2) || (offset2 <= offset1 && offset2 + length2 >= offset1);
	}

	private Range Merge(Range range1, Range range2)
	{
		var (offset1, length1) = range1.GetOffsetAndLength(fullLength);
		var (offset2, length2) = range2.GetOffsetAndLength(fullLength);
		var start = Math.Min(offset1, offset2);
		var end = Math.Max(offset1 + length1, offset2 + length2);
		return new Range(Index.FromStart(start), Index.FromStart(end));
	}

	public int Count => _count;

	public int FullLength => fullLength;

	public int MarkedLength
	{
		get
		{
			if (_count == 0)
				return 0;
			int total = 0;
			for (int i = 0; i < _count; i++)
			{
				var (_, rangeLength) = _ranges[i].GetOffsetAndLength(fullLength);
				total += rangeLength;
			}
			return total;
		}
	}

	public Range this[int index]
	{
		get
		{
			if (index < 0 || index >= _count)
				throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");

			return _ranges[index];
		}
	}

	public void Clear()
	{
		_count = 0;
	}

	public Span<Range>.Enumerator GetEnumerator() => _ranges.AsSpan(0, _count).GetEnumerator();
}

