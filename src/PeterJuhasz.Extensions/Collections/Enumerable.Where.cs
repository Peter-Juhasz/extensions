using System.Collections.Immutable;

namespace System.Linq;

public static partial class EnumerableExtension
{
	extension<T>(T[] source)
	{
		public T[] WhereToArray(Func<T, bool> predicate)
		{
			using var builder = new PooledArrayBuilder<T>();

			foreach (var item in source)
			{
				if (predicate(item))
				{
					builder.Add(item);
				}
			}

			if (builder.Count == source.Length)
			{
				return source;
			}

			return builder.ToArray();
		}
	}

	extension<T>(ImmutableArray<T> source)
	{
		public ImmutableArray<T> WhereToArray(Func<T, bool> predicate)
		{
			using var builder = new PooledArrayBuilder<T>();

			foreach (var item in source)
			{
				if (predicate(item))
				{
					builder.Add(item);
				}
			}

			if (builder.Count == source.Length)
			{
				return source;
			}

			return builder.ToImmutableArray();
		}
	}

	extension<T>(IReadOnlyList<T> source)
	{
		public IEnumerable<T> Where(Func<T, bool> predicate)
		{
			if (source.Count == 0)
			{
				return [];
			}

			if (source is [var single])
			{
				if (predicate(single))
				{
					return [single];
				}

				return [];
			}

			if (source.Count > 100)
			{
				return Enumerable.Where(source, predicate);
			}

			// TODO: optimize
			int matches = source.Count(predicate, out int firstIndex);
			if (matches == 0)
			{
				return [];
			}

			if (matches == source.Count)
			{
				return source;
			}

			if (matches == 1)
			{
				var item = source[firstIndex];
				return [item];
			}
			else if (matches == 2)
			{
				var item1 = source[firstIndex];
				var item2 = source[source.IndexOf(predicate, startIndex: firstIndex + 1)];
				return [item1, item2];
			}

			var results = new List<T>(capacity: matches);
			for (int i = firstIndex; i < source.Count; i++)
			{
				var item = source[i];
				if (predicate(item))
				{
					results.Add(item);
				}
			}
			return results;
		}
	}

	extension<T>(IEnumerable<T> source)
	{
		public IEnumerable<TResult> If<TResult>(bool condition, Func<IEnumerable<T>, IEnumerable<TResult>> ifTrue, Func<IEnumerable<T>, IEnumerable<TResult>> ifFalse) => condition ? ifTrue(source) : ifFalse(source);
		public IEnumerable<T> If(bool condition, Func<IEnumerable<T>, IEnumerable<T>> ifTrue) => source.If(condition, ifTrue, s => s);


		public IEnumerable<T> WhereIf(bool condition, Func<T, bool> predicate) => condition ? source.Where(predicate) : source;

		public IEnumerable<T> WhereIf<TFilter>(TFilter? filter, Func<TFilter?, bool> condition, Func<T, TFilter?, bool> predicate) => condition(filter) ? source.Where(e => predicate(e, filter)) : source;
	}

	extension<TElement>(IEnumerable<TElement> source)
	{
		public IEnumerable<TElement> WhereIfNotNull<TFilter>(TFilter? notNull, Func<TElement, TFilter, bool> predicate) where TFilter : class => source.WhereIf(notNull != null, e => predicate(e, notNull!));

		public IEnumerable<TElement> WhereIfNotNull<TFilter>(TFilter? notNull, Func<TElement, TFilter, bool> predicate) where TFilter : struct => source.WhereIf(notNull != null, e => predicate(e, notNull!.Value));

		public IEnumerable<TElement> WhereIfNotNullOrWhiteSpace(string? notNull, Func<TElement, string, bool> predicate) => source.WhereIf(!notNull.IsNullOrWhiteSpace(), e => predicate(e, notNull!));

		public IEnumerable<TElement> WhereIfAny<TFilter>(IReadOnlyCollection<TFilter>? filters, Func<TElement, IReadOnlyCollection<TFilter>, bool> predicate) => source.WhereIf(filters is { Count: > 0 }, e => predicate(e, filters!));

		public IEnumerable<TElement> WhereIfAny<TFilter>(IReadOnlySet<TFilter>? filters, Func<TElement, IReadOnlySet<TFilter>, bool> predicate) => source.WhereIf(filters is { Count: > 0 }, e => predicate(e, filters!));
	}

	extension<TElement>(IEnumerable<TElement?> source)
	{
		public IEnumerable<TElement> WhereNotNull() => source.Where(static e => e is not null)!;
	}

	extension(IEnumerable<string?> source)
	{
		public IEnumerable<string> WhereNotWhiteSpace() => source.Where(static e => !e.IsNullOrWhiteSpace())!;
	}

	extension<T>(IAsyncEnumerable<T> source)
	{
		public IAsyncEnumerable<T> WhereIf(bool condition, Func<T, bool> predicate) => condition ? source.Where(predicate) : source;
	}

	extension<TElement>(IAsyncEnumerable<TElement> source)
	{
		public IAsyncEnumerable<TElement> WhereIfNotNull<TFilter>(TFilter? notNull, Func<TElement, TFilter, bool> predicate) where TFilter : class => source.WhereIf(notNull != null, e => predicate(e, notNull!));

		public IAsyncEnumerable<TElement> WhereIfNotNull<TFilter>(TFilter? notNull, Func<TElement, TFilter, bool> predicate) where TFilter : struct => source.WhereIf(notNull != null, e => predicate(e, notNull!.Value));
	}
}
