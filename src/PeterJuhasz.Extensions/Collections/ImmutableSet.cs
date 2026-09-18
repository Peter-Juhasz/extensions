namespace System.Collections.Immutable;

public static class SetExtensions
{
	extension<T>(IImmutableSet<T> set)
	{
		public IImmutableSet<T> AddOrRemove(T item) =>
			set.Contains(item) ? set.Remove(item) : set.Add(item);
	}
}