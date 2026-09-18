using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace System.Linq;

public static partial class EnumerableExtension
{
	extension<T>(IReadOnlyCollection<T> source)
	{
		public IReadOnlyCollection<TResult> Select<TResult>(Func<T, TResult> selector)
		{
			if (source.Count == 0)
			{
				return [];
			}

			if (source is IReadOnlyList<T> list)
			{
				return list.SelectToList(selector);
			}

			var result = new TResult[source.Count];
			var i = 0;
			foreach (var item in source)
			{
				result[i] = selector(item);
				i++;
			}
			return result;
		}

		public IReadOnlyCollection<T> Do(Action<T> action)
		{
			foreach (var item in source)
			{
				action(item);
			}

			return source;
		}
	}

	extension<T>(ImmutableArray<T> source)
	{
		public TResult[] SelectToArray<TResult>(Func<T, TResult> selector)
		{
			if (source.Length == 0)
			{
				return [];
			}

			var result = new TResult[source.Length];
			for (int i = 0; i < source.Length; i++)
			{
				var item = source[i];
				result[i] = selector(item);
			}
			return result;
		}

		public IReadOnlyList<TResult> SelectToList<TResult>(Func<T, TResult> selector) => source.SelectToArray(selector);
	}

	extension<T>(T[] source)
	{
		public TResult[] SelectToArray<TResult>(Func<T, TResult> selector)
		{
			if (source.Length == 0)
			{
				return [];
			}

			var result = new TResult[source.Length];
			for (int i = 0; i < source.Length; i++)
			{
				var item = source[i];
				result[i] = selector(item);
			}
			return result;
		}

		public IReadOnlyList<TResult> SelectToList<TResult>(Func<T, TResult> selector) => source.SelectToArray(selector);
	}

	extension<T>(IReadOnlyList<T> source)
	{
		public TResult[] SelectToArray<TResult>(Func<T, TResult> selector)
		{
			if (source.Count == 0)
			{
				return [];
			}

			var result = new TResult[source.Count];
			for (int i = 0; i < source.Count; i++)
			{
				var item = source[i];
				result[i] = selector(item);
			}
			return result;
		}

		public ImmutableArray<TResult> SelectToImmutableArray<TResult>(Func<T, TResult> selector)
		{
			if (source.Count == 0)
			{
				return [];
			}

			var result = new TResult[source.Count];
			for (int i = 0; i < source.Count; i++)
			{
				var item = source[i];
				result[i] = selector(item);
			}
			return ImmutableCollectionsMarshal.AsImmutableArray(result);
		}

		public IReadOnlyList<TResult> SelectToList<TResult>(Func<T, TResult> selector) => source.SelectToArray(selector);

		public TResult[] SelectToArray<TResult>(Func<T, int, TResult> selector)
		{
			var result = new TResult[source.Count];
			for (int i = 0; i < source.Count; i++)
			{
				var item = source[i];
				result[i] = selector(item, i);
			}
			return result;
		}

		public IReadOnlyList<TResult> SelectToList<TResult>(Func<T, int, TResult> selector) => source.SelectToArray(selector);

		/// <summary>
		/// Projects the elements of a list to a new list without copying.
		/// </summary>
		public IReadOnlyList<TSelector> SelectToProjection<TSelector>(Func<T, TSelector> selector)
		{
			if (source.Count == 0)
			{
				return [];
			}

			return new ListProjection<T, TSelector>(source, selector);
		}

		public IReadOnlyList<T> Do(Action<T> action)
		{
			for (int i = 0; i < source.Count; i++)
			{
				var item = source[i];
				action(item);
			}

			return source;
		}
	}

	extension<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> source)
	{
		public TResult[] SelectToArray<TResult>(Func<KeyValuePair<TKey, TValue>, TResult> selector)
		{
			var result = new TResult[source.Count];
			var i = 0;
			foreach (var item in source)
			{
				result[i] = selector(item);
				i++;
			}
			return result;
		}

		public ImmutableArray<TResult> SelectToImmutableArray<TResult>(Func<KeyValuePair<TKey, TValue>, TResult> selector)
		{
			var array = source.SelectToArray(selector);
			return ImmutableCollectionsMarshal.AsImmutableArray(array);
		}
	}

	extension<T>(IEnumerable<T> source)
	{
		public IEnumerable<T> Do(Action<T> action)
		{
			foreach (var item in source)
			{
				action(item);
				yield return item;
			}
		}
	}

	extension<TSource>(IEnumerable<TSource> source)
	{
		public IEnumerable<TResult> SelectWhere<TResult>(Func<TSource, (bool Matches, TResult Item)> selector)
		{
			foreach (var item in source)
			{
				var selection = selector(item);
				if (selection.Matches)
				{
					yield return selection.Item;
				}
			}
		}
	}
}
