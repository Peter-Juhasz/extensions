using Microsoft.Extensions.ObjectPool;

namespace System.Collections.Generic;

public sealed class SmallSet<T>(IEqualityComparer<T>? comparer = null, int capacity = 1) : ISet<T>, IReadOnlySet<T>
{
	private readonly IEqualityComparer<T> _comparer = comparer ?? EqualityComparer<T>.Default;
	internal T[] _items = new T[capacity];
	internal int _count = 0;

	public int Count => _count;

	public bool IsReadOnly => false;


	public bool Add(T item)
	{
		if (Contains(item))
			return false;

		if (_count == _items.Length)
		{
			Array.Resize(ref _items, _items.Length * 2);
		}

		_items[_count++] = item;
		return true;
	}

	public bool Any() => _count > 0;

	void ICollection<T>.Add(T item) => Add(item);

	public bool TryAdd(T item, out T? existingItem)
	{
		if (Contains(item, out existingItem))
		{
			return false;
		}
		if (_count == _items.Length)
		{
			Array.Resize(ref _items, _items.Length * 2);
		}
		existingItem = default;
		_items[_count++] = item;
		return true;
	}

	public bool Contains(T item) => Contains(item, out _);

	public bool Contains(T item, out T? existing)
	{
		for (int i = 0; i < _count; i++)
		{
			if (_comparer.Equals(_items[i], item))
			{
				existing = _items[i];
				return true;
			}
		}

		existing = default;
		return false;
	}

	public bool TryGet(Func<T, bool> predicate, out T? item)
	{
		for (int i = 0; i < _count; i++)
		{
			if (predicate(_items[i]))
			{
				item = _items[i];
				return true;
			}
		}

		item = default;
		return false;
	}

	public bool Remove(T item)
	{
		for (int i = 0; i < _count; i++)
		{
			if (_comparer.Equals(_items[i], item))
			{
				_items[i] = _items[--_count];
				_items[_count] = default!;
				return true;
			}
		}

		return false;
	}

	public bool RemoveWhere(Func<T, bool> predicate)
	{
		bool removed = false;
		for (int i = 0; i < _count;)
		{
			if (predicate(_items[i]))
			{
				_items[i] = _items[--_count];
				_items[_count] = default!;
				removed = true;
			}
			else
			{
				i++;
			}
		}
		return removed;
	}

	public void Clear()
	{
		_count = 0;
		_items.AsSpan().Clear();
	}


	public void IntersectWith(IEnumerable<T> other)
	{
		throw new NotImplementedException();
	}

	public void ExceptWith(IEnumerable<T> other)
	{
		throw new NotImplementedException();
	}

	public bool IsProperSubsetOf(IEnumerable<T> other)
	{
		throw new NotImplementedException();
	}

	public bool IsProperSupersetOf(IEnumerable<T> other)
	{
		throw new NotImplementedException();
	}

	public bool IsSubsetOf(IEnumerable<T> other)
	{
		throw new NotImplementedException();
	}

	public bool IsSupersetOf(IEnumerable<T> other)
	{
		throw new NotImplementedException();
	}

	public bool Overlaps(IEnumerable<T> other)
	{
		throw new NotImplementedException();
	}

	public bool SetEquals(IEnumerable<T> other)
	{
		throw new NotImplementedException();
	}

	public void SymmetricExceptWith(IEnumerable<T> other)
	{
		throw new NotImplementedException();
	}

	public void UnionWith(IEnumerable<T> other)
	{
		foreach (var item in other)
		{
			Add(item);
		}
	}

	public void Sort(Comparison<T> comparison)
	{
		if (_count < 2)
		{
			return;
		}

		_items.AsSpan(0, _count).Sort(comparison);
	}

	public void Sort(IComparer<T> comparison)
	{
		if (_count < 2)
		{
			return;
		}

		_items.AsSpan(0, _count).Sort(comparison);
	}


	public void CopyTo(T[] array, int arrayIndex)
	{
		_items.AsSpan(0, _count).CopyTo(array.AsSpan(arrayIndex));
	}

	public T[] ToArray()
	{
		var result = new T[_count];
		_items.AsSpan(0, _count).CopyTo(result);
		return result;
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public IEnumerator<T> GetEnumerator()
	{
		for (int i = 0; i < _count; i++)
		{
			yield return _items[i];
		}
	}
}

public static partial class SmallCollectionsMarshal
{
	extension<T>(SmallSet<T> set) where T : notnull
	{
		public Span<T> AsSpan() => new(set._items, 0, set._count);
	}
}

public static partial class SmallSetPool<T> where T : notnull
{
	public static readonly ObjectPool<SmallSet<T>> Default = DefaultPool.Create(Policy.Instance);

	public static ObjectPool<SmallSet<T>> Create(int size = 20)
		=> DefaultPool.Create(Policy.Instance, size);

	public static PooledObject<SmallSet<T>> GetPooledObject()
		=> Default.GetPooledObject();

	public static PooledObject<SmallSet<T>> GetPooledObject(out SmallSet<T> set)
		=> Default.GetPooledObject(out set);

	private sealed class Policy(int? initialCapacity = null) : IPooledObjectPolicy<SmallSet<T>>
	{
		public static readonly Policy Instance = new();

		public SmallSet<T> Create() => initialCapacity is int ic ? new(capacity: ic) : [];

		public bool Return(SmallSet<T> list)
		{
			list.Clear();
			return true;
		}
	}
}
