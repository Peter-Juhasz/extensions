using System.Diagnostics.CodeAnalysis;

namespace System.Collections.Frozen;

public static class FrugalDictionary
{
	public static IReadOnlyDictionary<TKey, TValue> Empty<TKey, TValue>() where TKey : notnull => FrozenDictionary<TKey, TValue>.Empty;

	public static IReadOnlyDictionary<TKey, TValue> Create<TKey, TValue>(TKey key, TValue value, IEqualityComparer<TKey>? comparer = null) where TKey : notnull =>
		new FrugalDictionary1<TKey, TValue>(key, value, comparer ?? EqualityComparer<TKey>.Default);

	public static IReadOnlyDictionary<TKey, TValue> Create<TKey, TValue>(TKey key1, TValue value1, TKey key2, TValue value2, IEqualityComparer<TKey>? comparer = null) where TKey : notnull =>
		new FrugalDictionary2<TKey, TValue>(key1, value1, key2, value2, comparer ?? EqualityComparer<TKey>.Default);


	private sealed class FrugalDictionary1<TKey, TValue>(TKey key, TValue value, IEqualityComparer<TKey> comparer) : IReadOnlyDictionary<TKey, TValue> where TKey : notnull
	{
		public int Count => 1;
		public bool IsReadOnly => true;
		public TValue this[TKey k] => comparer.Equals(k, key) ? value : throw new KeyNotFoundException();
		public IEnumerable<TKey> Keys => [key];
		public IEnumerable<TValue> Values => [value];
		public bool ContainsKey(TKey k) => comparer.Equals(k, key);
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			yield return new KeyValuePair<TKey, TValue>(key, value);
		}
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public bool TryGetValue(TKey k, [MaybeNullWhen(false)] out TValue v)
		{
			if (comparer.Equals(k, key))
			{
				v = value;
				return true;
			}

			v = default;
			return false;
		}
	}

	private sealed class FrugalDictionary2<TKey, TValue>(TKey key1, TValue value1, TKey key2, TValue value2, IEqualityComparer<TKey> comparer) : IReadOnlyDictionary<TKey, TValue> where TKey : notnull
	{
		public static FrugalDictionary2<TKey, TValue> Create(TKey key1, TValue value1, TKey key2, TValue value2, IEqualityComparer<TKey>? comparer = null)
			=> new(key1, value1, key2, value2, comparer ?? EqualityComparer<TKey>.Default);

		public int Count => 2;
		public bool IsReadOnly => true;
		public TValue this[TKey k] => comparer.Equals(k, key1) ? value1 : comparer.Equals(k, key2) ? value2 : throw new KeyNotFoundException();
		public IEnumerable<TKey> Keys => [key1, key2];
		public IEnumerable<TValue> Values => [value1, value2];
		public bool ContainsKey(TKey k) => comparer.Equals(k, key1) || comparer.Equals(k, key2);
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			yield return new KeyValuePair<TKey, TValue>(key1, value1);
			yield return new KeyValuePair<TKey, TValue>(key2, value2);
		}
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public bool TryGetValue(TKey k, [MaybeNullWhen(false)] out TValue v)
		{
			if (comparer.Equals(k, key1))
			{
				v = value1;
				return true;
			}
			if (comparer.Equals(k, key2))
			{
				v = value2;
				return true;
			}
			v = default;
			return false;
		}
	}
}
