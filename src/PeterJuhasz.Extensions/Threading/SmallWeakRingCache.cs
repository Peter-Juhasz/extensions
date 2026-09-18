using System.Diagnostics.CodeAnalysis;

namespace System.Collections.Concurrent;

public sealed class SmallWeakRingCache<TKey, TValue>(int capacity, IEqualityComparer<TKey>? comparer = null)
	where TKey : notnull
	where TValue : class
{
	private readonly CacheItem?[] _items = new CacheItem?[capacity];
	private readonly IEqualityComparer<TKey> _comparer = comparer ?? EqualityComparer<TKey>.Default;
	private int _lastInsertionIndex = -1;

	public int Capacity => _items.Length;

	public void AddOrUpdate(TKey key, TValue value)
	{
		// try to find the existing item
		for (var i = 0; i < _items.Length; i++)
		{
			var item = _items[i];
			if (item is { Key: var itemKey } && _comparer.Equals(itemKey, key))
			{
				item.Value.SetTarget(value);
				return;
			}
		}

		// find next slot
		var nextInsertionIndex = Interlocked.Increment(ref _lastInsertionIndex);

		// handle overflow
		while (nextInsertionIndex == _items.Length)
		{
			if (Interlocked.CompareExchange(ref _lastInsertionIndex, 0, nextInsertionIndex) == nextInsertionIndex)
			{
				nextInsertionIndex = Interlocked.Increment(ref _lastInsertionIndex);
			}
		}

		// replace item
		CacheItem? existingItem;
		CacheItem? newItem = null;
		do
		{
			existingItem = _items[nextInsertionIndex];
			if (existingItem is { Key: var existingItemKey } && _comparer.Equals(existingItemKey, key))
			{
				existingItem.Value.SetTarget(value);
				return;
			}

			newItem ??= new CacheItem(key, new WeakReference<TValue>(value));
		} while (Interlocked.CompareExchange(ref _items[nextInsertionIndex], newItem, existingItem) != existingItem);
	}

	public TValue AddOrUpdate(TKey key, Func<TKey, TValue> factory)
	{
		// try to find the existing item
		for (var i = 0; i < _items.Length; i++)
		{
			var item = _items[i];
			if (item is { Key: var itemKey } && _comparer.Equals(itemKey, key))
			{
				var value = factory(key);
				item.Value.SetTarget(value);
				return value;
			}
		}

		// find next slot
		var nextInsertionIndex = Interlocked.Increment(ref _lastInsertionIndex);

		// handle overflow
		while (nextInsertionIndex == _items.Length)
		{
			if (Interlocked.CompareExchange(ref _lastInsertionIndex, 0, nextInsertionIndex) == nextInsertionIndex)
			{
				nextInsertionIndex = Interlocked.Increment(ref _lastInsertionIndex);
			}
		}

		// replace item
		CacheItem? existingItem;
		CacheItem? newItem = null;
		TValue? newValue = default;
		do
		{
			existingItem = _items[nextInsertionIndex];
			if (existingItem is { Key: var existingItemKey } && _comparer.Equals(existingItemKey, key))
			{
				var value = factory(key);
				existingItem.Value.SetTarget(value);
				return value;
			}

			if (newItem == null)
			{
				newValue = factory(key);
				newItem = new CacheItem(key, new WeakReference<TValue>(newValue));
			}
		} while (Interlocked.CompareExchange(ref _items[nextInsertionIndex], newItem, existingItem) != existingItem);

		return newValue!;
	}

	public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
	{
		for (var i = 0; i < _items.Length; i++)
		{
			if (_items[i] is { Key: var itemKey, Value: var weakReference } && _comparer.Equals(itemKey, key))
			{
				if (weakReference.TryGetTarget(out var target))
				{
					value = target;
					return true;
				}
				else
				{
					value = null;
					return false;
				}
			}
		}

		value = null;
		return false;
	}

	public bool ContainsKey(TKey key, [NotNullWhen(true)] out TKey? existingKey)
	{
		for (var i = 0; i < _items.Length; i++)
		{
			if (_items[i] is { Key: var itemKey } && _comparer.Equals(itemKey, key))
			{
				existingKey = itemKey;
				return true;
			}
		}

		existingKey = default;
		return false;
	}

	public bool ContainsKey(TKey key) => ContainsKey(key, out _);

	public int Count
	{
		get
		{
			var count = 0;

			for (var i = 0; i < _items.Length; i++)
			{
				if (_items[i] is { Value: var weakReference } && weakReference.TryGetTarget(out _))
				{
					count++;
				}
			}

			return count;
		}
	}

	public bool Remove(TKey key)
	{
		for (var i = 0; i < _items.Length; i++)
		{
			while (_items[i] is { Key: var itemKey } item && _comparer.Equals(itemKey, key))
			{
				if (Interlocked.CompareExchange(ref _items[i], null, item) == item)
				{
					return true;
				}
			}
		}

		return false;
	}

	public void RemoveCollected()
	{
		for (var i = 0; i < _items.Length; i++)
		{
			while (_items[i] is { Value: var weakReference } item && !weakReference.TryGetTarget(out _))
			{
				// race condition may happen if another thread sets the Value of the item to a fresh WeakReference

				Interlocked.CompareExchange(ref _items[i], null, item);
			}
		}
	}

	public void Clear()
	{
		_items.AsSpan().Clear();
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		for (var i = 0; i < _items.Length; i++)
		{
			if (_items[i] is { Key: var key, Value: var weakReference } && weakReference.TryGetTarget(out var value))
			{
				yield return new KeyValuePair<TKey, TValue>(key, value);
			}
		}
	}

	private sealed class CacheItem(TKey key, WeakReference<TValue> value)
	{
		public TKey Key => key;

		public WeakReference<TValue> Value { get; set; } = value;
	}
}
