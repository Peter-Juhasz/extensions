namespace System.Collections.Generic;

public class ListProjection<T, TSelector>(
	IReadOnlyList<T> source,
	Func<T, TSelector> selector
) : IReadOnlyList<TSelector>
{
	public int Count => source.Count;

	public TSelector this[int index] => selector(source[index]);

	public IEnumerator<TSelector> GetEnumerator()
	{
		foreach (var item in source)
		{
			yield return selector(item);
		}
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
