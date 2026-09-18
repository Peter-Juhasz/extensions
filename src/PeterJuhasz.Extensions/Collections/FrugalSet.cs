namespace System.Collections.Frozen;

public static class FrugalSet
{
	public static IReadOnlySet<T> Empty<T>() => FrozenSet<T>.Empty;

	public static IReadOnlySet<T> Create<T>(T item) => new FrugalSet1<T>(item);

	private sealed class FrugalSet1<T> : IReadOnlySet<T>
	{
		private readonly T item;
		private readonly IEqualityComparer<T> comparer;

		public FrugalSet1(T item, IEqualityComparer<T> comparer)
		{
			this.item = item;
			this.comparer = comparer;
		}
		public FrugalSet1(T item) : this(item, EqualityComparer<T>.Default) { }

		public int Count => 1;

		public bool Contains(T item) => comparer.Equals(this.item, item);

		public IEnumerator<T> GetEnumerator()
		{
			yield return item;
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

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
