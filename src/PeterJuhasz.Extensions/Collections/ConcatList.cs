using System.Collections.Immutable;

namespace System.Collections.Concurrent;

public class ConcatList2<T>(
	IReadOnlyList<T> first,
	IReadOnlyList<T> second
) : IReadOnlyList<T>
{
	public int Count => first.Count + second.Count;
	public T this[int index] => index < first.Count ? first[index] : second[index - first.Count];
	public IEnumerator<T> GetEnumerator()
	{
		foreach (var item in first)
		{
			yield return item;
		}
		foreach (var item in second)
		{
			yield return item;
		}
	}
	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

public class ConcatList<T>(
	ImmutableArray<IReadOnlyList<T>> lists
) : IReadOnlyList<T>
{
	public static IReadOnlyList<T> Create(params ReadOnlySpan<IReadOnlyList<T>> lists) => lists switch
	{
		[] => [],
		[var list] => list,
		[var first, var second] => new ConcatList2<T>(first, second),
		_ => new ConcatList<T>(lists.ToImmutableArray())
	};


	public int Count { get; } = lists.Sum(list => list.Count);

	public T this[int index]
	{
		get
		{
			foreach (var list in lists)
			{
				if (index < list.Count)
				{
					return list[index];
				}
				index -= list.Count;
			}
			throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
		}
	}
	public IEnumerator<T> GetEnumerator()
	{
		foreach (var list in lists)
		{
			foreach (var item in list)
			{
				yield return item;
			}
		}
	}
	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}