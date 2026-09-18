namespace System.Collections.Generic;

public sealed class SmallRingSet<TValue>(int capacity, IEqualityComparer<TValue>? comparer = null)
{
	private readonly IEqualityComparer<TValue> _comparer = comparer ?? EqualityComparer<TValue>.Default;
	internal readonly TValue[] _items = new TValue[capacity];
	private int _writeIndex = 0;

	public void Add(TValue value)
	{
		for (int i = 0; i < _items.Length; i++)
		{
			if (_comparer.Equals(_items[i], value))
			{
				return;
			}
		}

		_items[_writeIndex] = value;
		if (++_writeIndex == capacity)
		{
			_writeIndex = 0;
		}
		else
		{
			_writeIndex++;
		}
	}

	public bool Contains(TValue value, out TValue stored)
	{
		for (int i = 0; i < _items.Length; i++)
		{
			var item = _items[i];
			if (_comparer.Equals(item, value))
			{
				stored = item;
				return true;
			}
		}

		stored = default!;
		return false;
	}

	public bool Contains(TValue value) => Contains(value, out _);

	public void Remove(TValue value)
	{
		for (int i = 0; i < _items.Length; i++)
		{
			if (_comparer.Equals(_items[i], value))
			{
				_items[i] = default!;
				return;
			}
		}
	}

	public Enumerator GetEnumerator() => new(this, _comparer);

	public void Clear()
	{
		_items.AsSpan().Clear();
		_writeIndex = 0;
	}

	public ref struct Enumerator
	{
		internal Enumerator(SmallRingSet<TValue> set, IEqualityComparer<TValue> comparer)
		{
			_set = set;
			_comparer = comparer;
			_index = -1;
		}

		private readonly SmallRingSet<TValue> _set;
		private readonly IEqualityComparer<TValue> _comparer;
		private int _index = -1;
		private TValue _current = default!;

		public readonly TValue Current => _current;

		public bool MoveNext()
		{
			do
			{
				if (++_index >= _set._items.Length)
				{
					return false;
				}

				_current = _set._items[_index];
			} while (_comparer.Equals(_current, default));

			return true;
		}

		public void Reset() => _index = -1;
	}
}

public static partial class SmallCollectionsMarshal
{
	extension<T>(SmallRingSet<T> list) where T : notnull
	{
		public Span<T> AsSpan() => new(list._items);
	}
}
