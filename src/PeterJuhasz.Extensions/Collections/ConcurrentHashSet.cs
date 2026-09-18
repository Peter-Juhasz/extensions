namespace System.Collections.Concurrent;

public class ConcurrentHashSet<T> : ISet<T> where T : notnull
{
	private readonly ConcurrentDictionary<T, object?> _dictionary;

	public ConcurrentHashSet(int concurrencyLevel, int capacity)
	{
		_dictionary = new(concurrencyLevel, capacity);
	}

	public ConcurrentHashSet()
		: this(Environment.ProcessorCount, 31)
	{ }

	public int Count => _dictionary.Count;

	public bool IsReadOnly => false;

	public bool Add(T item) => _dictionary.TryAdd(item, null);

	public void Clear() => _dictionary.Clear();

	public bool Contains(T item) => _dictionary.ContainsKey(item);

	public void CopyTo(T[] array, int arrayIndex) => _dictionary.Keys.CopyTo(array, arrayIndex);

	public void ExceptWith(IEnumerable<T> other)
	{
		throw new NotImplementedException();
	}

	public IEnumerator<T> GetEnumerator() => _dictionary.Keys.GetEnumerator();

	public void IntersectWith(IEnumerable<T> other)
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

	public bool Remove(T item) => _dictionary.TryRemove(item, out _);

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
		throw new NotImplementedException();
	}

	void ICollection<T>.Add(T item) => Add(item);

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

