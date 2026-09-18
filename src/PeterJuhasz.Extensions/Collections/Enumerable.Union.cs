using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace System.Linq;

public static partial class EnumerableExtension
{
	extension(StringValues source)
	{
		public StringValues Union(StringValues subject)
		{
			if (source.Count == 0)
			{
				return subject;
			}

			if (subject.Count == 0)
			{
				return source;
			}

			var sourceHasAnyMissing = false;
			foreach (var item in subject)
			{
				if (!source.Contains(item))
				{
					sourceHasAnyMissing = true;
					break;
				}
			}
			if (!sourceHasAnyMissing)
			{
				return source;
			}

			var subjectHasAnyMissing = false;
			foreach (var item in source)
			{
				if (!subject.Contains(item))
				{
					subjectHasAnyMissing = true;
					break;
				}
			}
			if (!subjectHasAnyMissing)
			{
				return subject;
			}

			var result = new string?[source.Count + subject.Count];
			var i = 0;
			foreach (var item in source)
			{
				if (result.Contains(item))
				{
					continue;
				}

				result[i] = item;
				i++;
			}

			if (i < result.Length)
			{
				var newResult = new string[i];
				Array.Copy(result, newResult, i);
				result = newResult;
			}

			return new StringValues(result);
		}
	}

	extension<T>(IEnumerable<T> source)
	{
		public IEnumerable<T> Union(IReadOnlyCollection<T> second) => second.Count == 0 ? source : Enumerable.Union(source, second);

		public IEnumerable<T> Concat(IReadOnlyCollection<T> second) => second.Count == 0 ? source : Enumerable.Concat(source, second);
	}

	extension<T>(IReadOnlyCollection<T> first)
	{
		public IEnumerable<T> Union(IReadOnlyCollection<T> second) => (first.Count, second.Count) switch
		{
			(0, 0) => [],
			(0, _) => second,
			(_, 0) => first,
			_ => Enumerable.Union(first, second)
		};
		public IEnumerable<T> Concat(IReadOnlyCollection<T> second) => (first.Count, second.Count) switch
		{
			(0, 0) => [],
			(0, _) => second,
			(_, 0) => first,
			_ => Enumerable.Concat(first, second)
		};
	}

	extension<T>(IReadOnlySet<T>? first)
	where T : notnull
	{
		public IReadOnlySet<T>? Union(IReadOnlySet<T>? second)
		{
			if (first == null)
			{
				return second;
			}

			if (second == null)
			{
				return first;
			}

			var merged = first.Union((IReadOnlyCollection<T>)second).ToHashSet();
			return merged.Count > 0 ? merged : null;
		}

		public IReadOnlySet<T>? Intersect(IReadOnlySet<T>? second)
		{
			if (first == null)
			{
				return second;
			}

			if (second == null)
			{
				return first;
			}

			var merged = Enumerable.Intersect(first, second).ToHashSet();
			return merged.Count > 0 ? merged : null;
		}
	}

	extension<T>(IReadOnlyList<T> first)
	{
		public IReadOnlyList<T> ConcatToProjection(IReadOnlyList<T> second) => (first.Count, second.Count) switch
		{
			(0, 0) => [],
			(0, _) => second,
			(_, 0) => first,
			_ => new ConcatList2<T>(first, second)
		};
	}

	extension<T>(ImmutableArray<IReadOnlyList<T>> lists)
	{
		public IReadOnlyList<T> ConcatToProjection() => new ConcatList<T>(lists);
	}
}
