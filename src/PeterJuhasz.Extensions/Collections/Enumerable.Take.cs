
namespace System.Linq;

public static partial class EnumerableExtension
{
	extension<T>(IEnumerable<T> source)
	{
		public IEnumerable<T> TakeWhileIf(bool condition, Func<T, bool> predicate) => condition ? source.TakeWhile(predicate) : source;

		public IEnumerable<T> TakeIfNotNull(int? count) => count != null ? source.Take(count.Value) : source;
	}

	extension<TElement>(IEnumerable<TElement> source)
	{
		public IEnumerable<TElement> TakeWhileIfNotNull<TFilter>(TFilter? notNull, Func<TElement, TFilter, bool> predicate) where TFilter : class => source.TakeWhileIf(notNull != null, e => predicate(e, notNull!));

		public IEnumerable<TElement> TakeWhileIfNotNull<TFilter>(TFilter? notNull, Func<TElement, TFilter, bool> predicate) where TFilter : struct => source.TakeWhileIf(notNull != null, e => predicate(e, notNull!.Value));
	}

	extension<T>(IAsyncEnumerable<T> source)
	{
		public IAsyncEnumerable<T> TakeWhileIf(bool condition, Func<T, bool> predicate) => condition ? source.TakeWhile(predicate) : source;

		public IAsyncEnumerable<T> TakeIfNotNull(int? count) => count != null ? source.Take(count.Value) : source;
	}

	extension<TElement>(IAsyncEnumerable<TElement> source)
	{
		public IAsyncEnumerable<TElement> TakeWhileIfNotNull<TFilter>(TFilter? notNull, Func<TElement, TFilter, bool> predicate) where TFilter : class => source.TakeWhileIf(notNull != null, e => predicate(e, notNull!));

		public IAsyncEnumerable<TElement> TakeWhileIfNotNull<TFilter>(TFilter? notNull, Func<TElement, TFilter, bool> predicate) where TFilter : struct => source.TakeWhileIf(notNull != null, e => predicate(e, notNull!.Value));
	}

	extension<T>(IReadOnlyCollection<T> source)
	{
		public IEnumerable<T> Take(int count)
		{
			if (source.Count <= count)
			{
				return source;
			}

			return Enumerable.Take(source, count);
		}
	}

	extension<T>(IReadOnlyList<T> source)
	{
		public IReadOnlyList<T> TakeToList(int count)
		{
			if (source.Count <= count)
			{
				return source;
			}

			var result = new T[count];
			for (int i = 0; i < count; i++)
			{
				result[i] = source[i];
			}
			return result;
		}
	}
}
