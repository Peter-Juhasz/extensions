using System.Diagnostics.CodeAnalysis;

namespace System.Collections.Generic;

public sealed class SmallDictionary<TKey, TValue>(IEqualityComparer<TKey>? comparer = null, int capacity = 1) : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
{
	private readonly IEqualityComparer<TKey> _comparer = comparer ?? EqualityComparer<TKey>.Default;
	private KeyValuePair<TKey, TValue>[] _items = new KeyValuePair<TKey, TValue>[capacity];
	private int _count = 0;

	public int Count => _count;

	public bool IsReadOnly => false;

	public void Add(TKey key, TValue value)
	{
		if (ContainsKey(key))
			throw new ArgumentException("An item with the same key has already been added.");

		if (_count == _items.Length)
		{
			Array.Resize(ref _items, _items.Length * 2);
		}

		_items[_count++] = new(key, value);
	}

	public bool ContainsKey(TKey key)
	{
		for (int i = 0; i < _count; i++)
		{
			if (_comparer.Equals(_items[i].Key, key))
				return true;
		}

		return false;
	}

	public bool Remove(TKey key)
	{
		for (int i = 0; i < _count; i++)
		{
			if (_comparer.Equals(_items[i].Key, key))
			{
				_items[i] = _items[_count - 1];
				_count--;
				_items[_count] = default!;
				return true;
			}
		}

		return false;
	}

	public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
	{
		for (int i = 0; i < _count; i++)
		{
			var item = _items[i];
			if (_comparer.Equals(item.Key, key))
			{
				value = item.Value;
				return true;
			}
		}
		value = default;
		return false;
	}

	public TValue this[TKey key]
	{
		get
		{
			if (!TryGetValue(key, out var value))
				throw new KeyNotFoundException();

			return value;
		}
		set
		{
			for (int i = 0; i < _count; i++)
			{
				if (_comparer.Equals(_items[i].Key, key))
				{
					_items[i] = new(key, value);
					return;
				}
			}

			Add(key, value);
		}
	}

	public ICollection<TKey> Keys => Array.ConvertAll(_items[.._count], item => item.Key);

	public ICollection<TValue> Values => Array.ConvertAll(_items[.._count], item => item.Value);

	IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
	IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

	public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

	public void Clear()
	{
		_items.AsSpan(0, _count).Clear();
		_count = 0;
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		for (int i = 0; i < _count; i++)
		{
			if (_comparer.Equals(_items[i].Key, item.Key) && Equals(_items[i].Value, item.Value))
				return true;
		}
		return false;
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		Array.Copy(_items, 0, array, arrayIndex, _count);
	}

	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		for (int i = 0; i < _count; i++)
		{
			if (_comparer.Equals(_items[i].Key, item.Key) && Equals(_items[i].Value, item.Value))
			{
				_items[i] = _items[_count - 1];
				_count--;
				_items[_count] = default!;
				return true;
			}
		}
		return false;
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		for (int i = 0; i < _count; i++)
		{
			yield return _items[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
