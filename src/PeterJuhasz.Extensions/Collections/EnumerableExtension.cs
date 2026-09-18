using System.Collections;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace System.Linq;

public static partial class EnumerableExtension
{
	extension<T>(IReadOnlyList<T> source)
	{
		public bool TryGetSpan(out ReadOnlySpan<T> span)
		{
			switch (source)
			{
				case T[] array:
					span = array;
					return true;

				case List<T> list:
					span = CollectionsMarshal.AsSpan(list);
					return true;

				case ImmutableArray<T> immutableArray:
					span = immutableArray.AsSpan();
					return true;
			}

			span = default;
			return false;
		}

		public void CopyTo(Span<T> span)
		{
			if (source.TryGetSpan(out var sourceSpan))
			{
				sourceSpan.CopyTo(span);
			}

			if (span.Length < source.Count)
			{
				throw new ArgumentException("The destination span is too small to copy all the elements.");
			}

			for (int i = 0; i < source.Count; i++)
			{
				span[i] = source[i];
			}
		}



		public IReadOnlyList<T> ToList() => source;
		public List<T> ToMutableList() => source switch
		{
			List<T> list => list,
			_ => new(source)
		};

		public IReadOnlySet<T> ToReadOnlySet() => source switch
		{
			IReadOnlySet<T> set => set,
			{ Count: 0 } => FrozenSet<T>.Empty,
			{ Count: 1 } => FrugalSet.Create(source.First()),
			_ => source.ToFrozenSet()
		};

		public T? MaxBy<TSelector>(Func<T, TSelector> selector) where TSelector : IComparable<TSelector>
		{
			if (source.Count == 0)
			{
				return default;
			}

			if (source.Count == 1)
			{
				return source[0];
			}

			T result = source[0];
			TSelector max = selector(source[0]);

			for (int i = 1; i < source.Count; i++)
			{
				var item = source[i];
				var value = selector(item);
				if (value.CompareTo(max) > 0)
				{
					result = item;
					max = value;
				}
			}

			return result;
		}

		public T? MinBy<TSelector>(Func<T, TSelector> selector) where TSelector : IComparable<TSelector>
		{
			if (source.Count == 0)
			{
				return default;
			}

			if (source.Count == 1)
			{
				return source[0];
			}

			T result = source[0];
			TSelector min = selector(source[0]);

			for (int i = 1; i < source.Count; i++)
			{
				var item = source[i];
				var value = selector(item);
				if (value.CompareTo(min) < 0)
				{
					result = item;
					min = value;
				}
			}

			return result;
		}

		public IEnumerable<Range> GroupByUntilChangedRanges<TSelector>(Func<T, TSelector> selector, IEqualityComparer<TSelector>? equalityComparer = null)
		{
			if (source.Count == 0)
			{
				yield break;
			}

			if (source.Count == 1)
			{
				yield return Range.All;
				yield break;
			}

			equalityComparer ??= EqualityComparer<TSelector>.Default;
			var currentSelector = selector(source[0]);
			var rangeStart = 0;
			for (int i = 1; i < source.Count; i++)
			{
				var item = source[i];
				var newSelector = selector(item);
				if (!equalityComparer.Equals(currentSelector, newSelector))
				{
					yield return rangeStart..i;
					currentSelector = newSelector;
					rangeStart = i;
				}
			}

			if (rangeStart < source.Count)
			{
				yield return rangeStart..Index.End;
			}
		}
	}

	extension<T>(Span<T> source)
	{
		public ReadOnlySpan<T> AsReadOnly() => (ReadOnlySpan<T>)source;
	}

	extension<T>(IAsyncEnumerable<T> source)
	{
		public async IAsyncEnumerable<T> DistinctBy<TKey>(Func<T, TKey> keySelector)
		{
			using var _ = HashSetPool<TKey>.GetPooledObject(out var set);
			await foreach (var item in source)
			{
				if (set.Add(keySelector(item)))
				{
					yield return item;
				}
			}
		}
	}

	extension<TKey, TValue>(IImmutableDictionary<TKey, TValue> source) where TValue : class
	{
		public IImmutableDictionary<TKey, TValue> AddIfNotNull(TKey key, TValue? value) =>
		value is not null ? source.Add(key, value) : source;
	}

	extension<TElement>(IImmutableSet<TElement>? source)
	{
		public IImmutableSet<TElement>? NullIfEmpty() => source is { Count: > 0 } ? source : null;
	}

	extension<TKey, TElement>(IReadOnlyDictionary<TKey, TElement>? source)
	{
		public IReadOnlyDictionary<TKey, TElement>? NullIfEmpty() => source is { Count: > 0 } ? source : null;
	}

	extension<TKey, TElement>(IImmutableDictionary<TKey, TElement>? source)
	{
		public IImmutableDictionary<TKey, TElement>? NullIfEmpty() => source is { Count: > 0 } ? source : null;
	}

	extension<TElement>(IImmutableList<TElement>? source)
	{
		public IImmutableList<TElement>? NullIfEmpty() => source is { Count: > 0 } ? source : null;
	}

	extension<TElement>(IReadOnlyList<TElement>? source)
	{
		public IReadOnlyList<TElement>? NullIfEmpty() => source is { Count: > 0 } ? source : null;
	}

	extension<TElement>(IReadOnlyCollection<TElement>? source)
	{
		public IReadOnlyCollection<TElement>? NullIfEmpty() => source is { Count: > 0 } ? source : null;
	}

	extension<TElement>(TElement[]? source)
	{
		public TElement[]? NullIfEmpty() => source is { Length: > 0 } ? source : null;
	}

	extension(string? source)
	{
		public string? NullIfEmpty() => source.IsNullOrEmpty() ? null : source;

		public string? NullIfWhiteSpace() => source.IsNullOrWhiteSpace() ? null : source;
	}

	extension<T>(T[] source)
	{
		public IReadOnlyList<T> ToList() => source;
		public IReadOnlyList<T> AsReadOnlyList() => source;

		public void OrderByInPlace<TResult>(Func<T, TResult> selector)
			where TResult : IComparable<TResult>
		{
			Array.Sort(source, new Comparison<T>((x, y) => selector(x).CompareTo(selector(y))));
		}

		public void OrderByDescendingInPlace<TResult>(Func<T, TResult> selector)
			where TResult : IComparable<TResult>
		{
			Array.Sort(source, new Comparison<T>((x, y) => selector(y).CompareTo(selector(x))));
		}

		public void Replace(Func<T, bool> predicate, Func<T, T> replacement)
		{
			for (int i = 0; i < source.Length; i++)
			{
				var item = source[i];
				if (predicate(item))
				{
					source[i] = replacement(item);
				}
			}
		}
	}

	extension<T>(List<T> list)
	{
		public ImmutableArray<T> ToImmutableArray()
		{
			var buffer = new T[list.Count];
			list.CopyTo(buffer);
			return ImmutableCollectionsMarshal.AsImmutableArray(buffer);
		}

		public ref T ItemAsRef(int index) => ref CollectionsMarshal.AsSpan(list)[index];
	}

	extension<T>(IImmutableList<T> list)
	{
		public ImmutableArray<T> ToImmutableArray()
		{
			var buffer = new T[list.Count];
			if (list is ImmutableList<T> immutableList)
			{
				immutableList.CopyTo(buffer);
			}
			else
			{
				for (int i = 0; i < list.Count; i++)
				{
					buffer[i] = list[i];
				}
			}
			return ImmutableCollectionsMarshal.AsImmutableArray(buffer);
		}
	}

	extension<T>(IOrderedEnumerable<T> source)
	{
		public List<T> ToMutableList() => source switch
		{
			List<T> list => list,
			_ => new List<T>(source)
		};
		public IOrderedEnumerable<T> ThenByTrueFirst(Func<T, bool> selector) => source.ThenBy(i => !selector(i));
	}

	extension<T>(IReadOnlyCollection<T> source)
	{
		public SmallDictionary<TKey, TValue> ToSmallDictionary<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> valueSelector)
		{
			var dictionary = new SmallDictionary<TKey, TValue>(capacity: source.Count);
			foreach (var item in source)
			{
				dictionary.Add(keySelector(item), valueSelector(item));
			}
			return dictionary;
		}
	}

	extension<T>(IEnumerable<T> source)
	{
		public SmallDictionary<TKey, TValue> ToSmallDictionary<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> valueSelector)
		{
			var dictionary = new SmallDictionary<TKey, TValue>();
			foreach (var item in source)
			{
				dictionary.Add(keySelector(item), valueSelector(item));
			}
			return dictionary;
		}

		public SmallSet<T> ToSmallSet()
		{
			var dictionary = new SmallSet<T>();
			foreach (var item in source)
			{
				dictionary.Add(item);
			}
			return dictionary;
		}


		public IOrderedEnumerable<T> OrderByTrueFirst(Func<T, bool> selector) => source.OrderBy(i => !selector(i));

		public IEnumerable<T> DistinctUntilChanged(IEqualityComparer<T> comparer)
		{
			using var enumerator = source.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				yield break;
			}
			var current = enumerator.Current;
			yield return current;
			while (enumerator.MoveNext())
			{
				if (!comparer.Equals(current, enumerator.Current))
				{
					current = enumerator.Current;
					yield return current;
				}
			}
		}
	}

	extension<T>(List<T> source)
	{
		public void OrderByInPlace<TResult>(Func<T, TResult> selector)
		where TResult : IComparable<TResult>
		{
			source.Sort(new Comparison<T>((x, y) => selector(x).CompareTo(selector(y))));
		}

		public void OrderByDescendingInPlace<TResult>(Func<T, TResult> selector)
			where TResult : IComparable<TResult>
		{
			source.Sort(new Comparison<T>((x, y) => selector(y).CompareTo(selector(x))));
		}
	}

	extension<T>(IReadOnlyList<T> items)
	{
		public bool IsOrderedBy<TSelector>(Func<T, TSelector> selector)
		where TSelector : IComparable<TSelector>
		{
			if (items.Count <= 1)
			{
				return true;
			}

			var previous = selector(items[0]);
			for (int i = 1; i < items.Count; i++)
			{
				var current = selector(items[i]);
				if (previous.CompareTo(current) > 0)
				{
					return false;
				}

				previous = current;
			}

			return true;
		}

		public T[] OrderByToArray<TSelector>(Func<T, TSelector> selector)
			where TSelector : IComparable<TSelector>
		{
			var result = new T[items.Count];
			if (items.TryGetSpan(out var span))
			{
				span.CopyTo(result);
			}
			else
			{
				for (int i = 0; i < items.Count; i++)
				{
					result[i] = items[i];
				}
			}
			Array.Sort(result, new Comparison<T>((x, y) => selector(x).CompareTo(selector(y))));
			return result;
		}

		public T[] OrderByToArray<TSelector>(Func<T, TSelector> selector, IComparer<TSelector> comparer)
		{
			var result = new T[items.Count];
			if (items.TryGetSpan(out var span))
			{
				span.CopyTo(result);
			}
			else
			{
				for (int i = 0; i < items.Count; i++)
				{
					result[i] = items[i];
				}
			}
			Array.Sort(result, new Comparison<T>((x, y) => comparer.Compare(selector(x), selector(y))));
			return result;
		}

		public T[] OrderByDescendingToArray<TSelector>(Func<T, TSelector> selector)
			where TSelector : IComparable<TSelector>
		{
			var result = new T[items.Count];
			if (items.TryGetSpan(out var span))
			{
				span.CopyTo(result);
			}
			else
			{
				for (int i = 0; i < items.Count; i++)
				{
					result[i] = items[i];
				}
			}
			Array.Sort(result, new Comparison<T>((x, y) => selector(y).CompareTo(selector(x))));
			return result;
		}

		public IReadOnlyList<T> OrderByToList<TSelector>(Func<T, TSelector> selector)
			where TSelector : IComparable<TSelector>
		{
			if (items.IsOrderedBy(selector))
			{
				return items;
			}

			return items.OrderByToArray(selector);
		}
	}

	extension<T>(IEnumerable<T> items)
	{
		public IOrderedEnumerable<T> Prefer(Func<T, bool> selector) =>
		items.OrderBy(k => !selector(k));
	}

	extension<T>(IImmutableList<T> source)
	{
		public IImmutableList<T> Replace(Func<T, bool> predicate, Func<T, T> replacement)
		{
			var result = source;

			for (int i = 0; i < source.Count; i++)
			{
				var item = source[i];
				if (predicate(item))
				{
					result = result.SetItem(i, replacement(item));
				}
			}

			return result;
		}

		public IImmutableList<T> Replace(Func<T, bool> predicate, T replacement)
		{
			var result = source;

			for (int i = 0; i < source.Count; i++)
			{
				var item = source[i];
				if (predicate(item))
				{
					result = result.SetItem(i, replacement);
				}
			}

			return result;
		}

		public int FindIndex(Func<T, bool> predicate)
		{
			for (int i = 0; i < source.Count; i++)
			{
				if (predicate(source[i]))
				{
					return i;
				}
			}

			return -1;
		}
	}

	extension<TKey, TValue>(IImmutableDictionary<TKey, TValue> source)
	{
		public IImmutableDictionary<TKey, TValue> SetItems(ReadOnlySpan<KeyValuePair<TKey, TValue>> items)
		{
			foreach (var item in items)
			{
				source = source.SetItem(item.Key, item.Value);
			}

			return source;
		}
	}

	extension<TKey>(IImmutableDictionary<TKey, int> attemptStatusCounts)
	{
		public IImmutableDictionary<TKey, int> UpdateCounts(TKey from, TKey to) => attemptStatusCounts
		.SetItem(from, attemptStatusCounts.TryGetValue(from, out var fromCount) ? fromCount - 1 : 0)
		.SetItem(to, attemptStatusCounts.TryGetValue(to, out var toCount) ? toCount + 1 : 1)
	;

		public IImmutableDictionary<TKey, int> Increment(TKey from, int count = 1) => attemptStatusCounts
			.SetItem(from, (attemptStatusCounts.TryGetValue(from, out var fromCount) ? fromCount : 0) + count)
		;

		public IImmutableDictionary<TKey, int> AddByKeys(IEnumerable<KeyValuePair<TKey, int>> other)
		{
			// TODO: optimize for large dictionaries

			foreach (var kvp in other)
			{
				attemptStatusCounts = attemptStatusCounts.SetItem(kvp.Key, (attemptStatusCounts.TryGetValue(kvp.Key, out var count) ? count : 0) + kvp.Value);
			}

			return attemptStatusCounts;
		}
	}

	extension<TKey, TValue>(IDictionary<TKey, TValue> dictionary) where TKey : notnull
	{
		public void AddOrUpdate(TKey key, TValue addValue, Func<TValue, TValue> updateValueFactory)
		{
			if (dictionary.TryGetValue(key, out var existingValue))
			{
				dictionary[key] = updateValueFactory(existingValue);
			}
			else
			{
				dictionary[key] = addValue;
			}
		}

		public TValue? GetOrDefault(TKey key) => dictionary.TryGetValue(key, out var value) ? value : default;

		public IReadOnlyDictionary<TKey, TValue> AsOrToReadOnly()
		{
			if (dictionary is IReadOnlyDictionary<TKey, TValue> readOnlyDictionary)
			{
				return readOnlyDictionary;
			}

			return new ReadOnlyDictionary<TKey, TValue>(dictionary);
		}
	}

	extension<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> dictionary) where TKey : notnull
	{
		public IDictionary<TKey, TValue> AsOrToMutable()
		{
			if (dictionary is IDictionary<TKey, TValue> mutableDictionary)
			{
				return mutableDictionary;
			}

			return new Dictionary<TKey, TValue>(dictionary);
		}
	}


	extension<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> dictionary)
	{
		public void CopyTo(IDictionary<TKey, TValue> target)
		{
			foreach (var kvp in dictionary)
			{
				target[kvp.Key] = kvp.Value;
			}
		}
	}
}

public static partial class ReadOnlyListExtensions
{
	extension<T>(IReadOnlyList<T> source)
	{
		public ReadOnlyListEnumerable<T> AsValueEnumerable() => new(source);

		public ListSelectEnumerable<T, TResult> SelectValue<TResult>(Func<T, TResult> selector) => new(source, selector);

		public ListWhereEnumerable<T> WhereValue(Func<T, bool> predicate) => new(source, predicate);
	}

	extension<T>(IList<T> source)
	{
		public ListEnumerable<T> AsValueEnumerable() => new(source);
	}

	extension<T>(List<T> source)
	{
		public ListEnumerable<T> AsValueEnumerable() => new(source);
	}

	public readonly ref struct ReadOnlyListEnumerable<T>(IReadOnlyList<T> source)
	{
		public Enumerator GetEnumerator() => new(source);

		public ref struct Enumerator
		{
			internal Enumerator(IReadOnlyList<T> source)
			{
				Current = default!;
				enumerator = source;
				index = -1;
			}

			private readonly IReadOnlyList<T> enumerator;

			public T Current { get; private set; }

			private int index;

			public bool MoveNext()
			{
				if (++index < enumerator.Count)
				{
					Current = enumerator[index];
					return true;
				}
				else
				{
					return false;
				}
			}

			public void Reset()
			{
				index = -1;
			}

			public void Dispose() { }
		}
	}

	public readonly ref struct ListEnumerable<T>(IList<T> source)
	{
		public Enumerator GetEnumerator() => new(source);

		public ref struct Enumerator
		{
			internal Enumerator(IList<T> source)
			{
				Current = default!;
				enumerator = source;
				index = -1;
			}

			private readonly IList<T> enumerator;

			public T Current { get; private set; }

			private int index;

			public bool MoveNext()
			{
				if (++index < enumerator.Count)
				{
					Current = enumerator[index];
					return true;
				}
				else
				{
					return false;
				}
			}

			public void Reset()
			{
				index = -1;
			}

			public void Dispose() { }
		}
	}

	public readonly ref struct ListSelectEnumerable<T, TResult>
	{
		public ListSelectEnumerable(IReadOnlyList<T> source, Func<T, TResult> selector)
		{
			this.source = source;
			this.selector = selector;
		}

		private readonly IReadOnlyList<T> source;
		private readonly Func<T, TResult> selector;

		public Enumerator GetEnumerator() => new(source, selector);

		public ref struct Enumerator
		{
			internal Enumerator(IReadOnlyList<T> source, Func<T, TResult> selector)
			{
				Current = default!;
				enumerator = source;
				this.selector = selector;
				index = -1;
			}

			private readonly IReadOnlyList<T> enumerator;
			private readonly Func<T, TResult> selector;

			public TResult Current { get; private set; }

			private int index;

			public bool MoveNext()
			{
				if (++index < enumerator.Count)
				{
					Current = selector(enumerator[index]);
					return true;
				}
				else
				{
					return false;
				}
			}

			public void Reset()
			{
				index = -1;
			}

			public void Dispose() { }
		}
	}

	public readonly ref struct ListWhereEnumerable<T>
	{
		public ListWhereEnumerable(IReadOnlyList<T> source, Func<T, bool> predicate)
		{
			this.source = source;
			this.predicate = predicate;
		}

		private readonly IReadOnlyList<T> source;
		private readonly Func<T, bool> predicate;

		public Enumerator GetEnumerator() => new(source, predicate);

		public ref struct Enumerator
		{
			internal Enumerator(IReadOnlyList<T> source, Func<T, bool> predicate)
			{
				Current = default!;
				enumerator = source;
				this.predicate = predicate;
				index = -1;
			}

			private readonly IReadOnlyList<T> enumerator;
			private readonly Func<T, bool> predicate;

			public T Current { get; private set; }

			private int index;

			public bool MoveNext()
			{
				index++;

				while (index < enumerator.Count)
				{
					var item = enumerator[index];
					if (predicate(item))
					{
						Current = item;
						return true;
					}

					index++;
				}

				return false;
			}

			public void Reset()
			{
				index = -1;
			}

			public void Dispose() { }
		}
	}

}