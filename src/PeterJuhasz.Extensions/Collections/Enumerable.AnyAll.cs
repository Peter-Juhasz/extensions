using Microsoft.Extensions.Primitives;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Linq;

public static partial class EnumerableExtension
{
	extension<T>(IEnumerable<T> source)
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Any([NotNullWhen(true)] out T? value)
		{
			using var enumerator = source.GetEnumerator();
			if (enumerator.MoveNext())
			{
				value = enumerator.Current!;
				return true;
			}

			value = default;
			return false;
		}
	}

	extension(int n)
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Any() => n > 0;
	}

	extension<T>(ReadOnlySpan<T> source)
	{
		/*public static bool Any<T>(this IEnumerable<T> source) => source switch
{
ICollection<T> collection => collection.Count > 0,
IReadOnlyCollection<T> readOnlyCollection => readOnlyCollection.Count > 0,
_ => Enumerable.Any(source)
};*/

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public static bool Any<TItem, TKey>(this SortedIndexedList<TItem, TKey> source) where TKey : IEquatable<TKey> => source.Count > 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Any() => source.Length > 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Any(Func<T, bool> predicate) => source.TryFirst(predicate, out _, out _);

		public bool All(Func<T, bool> predicate)
		{
			for (int i = 0; i < source.Length; i++)
			{
				if (!predicate(source[i]))
				{
					return false;
				}
			}

			return true;
		}
	}

	extension<T>(T[] source)
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Any() => source.Length > 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Any(Func<T, bool> predicate) => source.AsSpan().AsReadOnly().TryFirst(predicate, out _, out _);

		public bool All(Func<T, bool> predicate) => source.AsSpan().AsReadOnly().All(predicate);
	}

	extension<T>(IReadOnlyCollection<T>? source)
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Any() => source?.Count > 0;
	}

	extension<T>(IReadOnlyCollection<T> source)
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Any(Func<T, bool> predicate) => source switch
		{
			T[] array => array.Any(predicate),
			ImmutableArray<T> immutableArray => immutableArray.Any(predicate),
			{ Count: 0 } => false,
			IReadOnlyList<T> list => list.Any(predicate),
			_ => Enumerable.Any(source, predicate)
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Empty() => source.Count == 0;

		public bool All(Func<T, bool> predicate) => source switch
		{
			T[] array => array.All(predicate),
			ImmutableArray<T> immutableArray => immutableArray.All(predicate),
			{ Count: 0 } => true,
			IReadOnlyList<T> list => list.All(predicate),
			_ => Enumerable.All(source, predicate)
		};
	}

	extension<T>(IReadOnlyList<T> source)
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Any(Func<T, bool> predicate) => source.TryFirst(predicate, out _, out _);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool AnyBackwards(Func<T, bool> predicate) => source.TryLast(predicate, out _, out _);

		public bool All(Func<T, bool> predicate)
		{
			for (int i = 0; i < source.Count; i++)
			{
				if (!predicate(source[i]))
				{
					return false;
				}
			}

			return true;
		}

		public bool AllEquals<TSelector>(Func<T, TSelector> selector, IEqualityComparer<TSelector>? comparer = null)
		{
			if (source.Count <= 1)
			{
				return true;
			}

			var first = selector(source[0]);
			comparer ??= EqualityComparer<TSelector>.Default;
			for (int i = 1; i < source.Count; i++)
			{
				if (!comparer.Equals(first, selector(source[i])))
				{
					return false;
				}
			}

			return true;
		}


		public bool Contains(T subject)
		{
			for (int i = 0; i < source.Count; i++)
			{
				if (source[i] is null)
				{
					if (subject is null)
					{
						return true;
					}
				}
				else if (source[i]!.Equals(subject))
				{
					return true;
				}
			}

			return false;
		}
	}

	extension<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> source)
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Any() => source.Count > 0;
	}

	extension(string source)
	{
		public bool All(Func<char, bool> predicate)
		{
			for (int i = 0; i < source.Length; i++)
			{
				if (!predicate(source[i]))
				{
					return false;
				}
			}

			return true;
		}
	}

	extension(StringValues source)
	{
		public bool Contains<T>(T subject)
		{
			foreach (var item in source)
			{
				if (item!.Equals(subject))
				{
					return true;
				}
			}

			return false;
		}
	}
	/*public static bool Contains<T>(this IEnumerable<T> source, T subject) => source switch
{
	IReadOnlySet<T> set => set.Contains(subject),
	IReadOnlyList<T> list => list.Contains(subject),
	_ => Enumerable.Contains(source, subject)
};*/
}
