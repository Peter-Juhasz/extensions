namespace System.Collections.Generic;

public sealed class SmallRingDictionary<TKey, TValue>(int capacity, IEqualityComparer<TKey>? comparer = null)
{
	private readonly IEqualityComparer<TKey> _comparer = comparer ?? EqualityComparer<TKey>.Default;
	internal readonly KeyValuePair<TKey, TValue>[] _items = new KeyValuePair<TKey, TValue>[capacity];
	private int _writeIndex = 0;

	public void Add(TKey key, TValue value)
	{
		_items[_writeIndex] = new(key, value);
		if (++_writeIndex == capacity)
		{
			_writeIndex = 0;
		}
		else
		{
			_writeIndex++;
		}
	}

	public void AddOrUpdate(TKey key, TValue value)
	{
		for (int i = 0; i < _items.Length; i++)
		{
			ref var item = ref _items[i];
			if (_comparer.Equals(item.Key, key))
			{
				item = new(key, value);
				return;
			}
		}

		Add(key, value);
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		for (int i = 0; i < _items.Length; i++)
		{
			var item = _items[i];
			if (_comparer.Equals(item.Key, key))
			{
				value = item.Value!;
				return true;
			}
		}

		value = default!;
		return false;
	}

	public void Clear()
	{
		_items.AsSpan().Clear();
	}
}

public static partial class SmallCollectionsMarshal
{
	extension<TKey, TValue>(SmallRingDictionary<TKey, TValue> dictionary) where TKey : notnull
	{
		public Span<KeyValuePair<TKey, TValue>> AsSpan() => new(dictionary._items);
	}
}
