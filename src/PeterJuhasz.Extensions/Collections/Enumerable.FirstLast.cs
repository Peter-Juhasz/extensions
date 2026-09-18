using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Linq;

public static partial class EnumerableExtension
{
	extension<T>(IReadOnlyList<T> source)
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T First() => source[0];
		public T? FirstOrDefault() => source.Count > 0 ? source[0] : default;

		public T First(Func<T, bool> predicate) => source.TryFirst(predicate, out var item, out _) ? item : throw new InvalidOperationException();
		public T? FirstOrDefault(Func<T, bool> predicate) => source.TryFirst(predicate, out var item, out _) ? item : default;

		public bool TryIndexOf(Func<T, bool> predicate, out int index)
		{
			index = source.IndexOf(predicate);
			return index != -1;
		}

		public bool TryFirst([NotNullWhen(true)] out T? item)
		{
			if (source.Count == 0)
			{
				item = default;
				return false;
			}

			item = source.First()!;
			return true;
		}

		public bool TryFirst(Func<T, bool> predicate, [NotNullWhen(true)] out T? item) => source.TryFirst(predicate, out item, out _);
		public bool TryFirst(Func<T, bool> predicate, [NotNullWhen(true)] out T? item, out int index)
		{
			if (source.Count == 0)
			{
				item = default;
				index = -1;
				return false;
			}

			index = source.IndexOf(predicate);
			if (index == -1)
			{
				item = default;
				return false;
			}

			item = source[index]!;
			return true;
		}



		public bool TryLast(Func<T, bool> predicate, [NotNullWhen(true)] out T? item) => source.TryLast(predicate, out item, out _);
		public bool TryLast(Func<T, bool> predicate, [NotNullWhen(true)] out T? item, out int index)
		{
			index = source.LastIndexOf(predicate);
			if (index == -1)
			{
				item = default;
				return false;
			}

			item = source[index]!;
			return true;
		}

		public T Last() => source[^1];
		public T? LastOrDefault() => source.Count > 0 ? source[^1] : default;

		public T Single() => source is [T single] ? single : throw new InvalidOperationException("Sequence contains no elements.");
		public T? SingleOrDefault() => source switch
		{
			[T single] => single,
			[] => default,
			_ => throw new InvalidOperationException("Sequence contains no elements.")
		};
	}

	extension<T>(T[] source)
	{
		public T First() => source[0];
		public T? FirstOrDefault() => source.Length > 0 ? source[0] : default;
		public T First(Func<T, bool> predicate) => source.TryFirst(predicate, out var item, out _) ? item : throw new InvalidOperationException();
		public T? FirstOrDefault(Func<T, bool> predicate) => source.TryFirst(predicate, out var item, out _) ? item : default;
	}

	extension<T>(ImmutableArray<T> source) where T : struct
	{
		public T? FirstOrNull() => source.IsEmpty ? null : source[0];
	}

	extension<T>(IEnumerable<T> source)
	{
		public bool TryFirst([NotNullWhen(true)] out T? item)
		{
			foreach (var i in source)
			{
				item = i!;
				return true;
			}

			item = default;
			return false;
		}

		public bool TryFirst(Func<T, bool> predicate, [NotNullWhen(true)] out T? item)
		{
			if (source.TryGetNonEnumeratedCount(out var count) && count == 0)
			{
				item = default;
				return false;
			}

			foreach (var i in source)
			{
				if (predicate(i))
				{
					item = i!;
					return true;
				}
			}

			item = default;
			return false;
		}
	}

	extension<T>(ReadOnlySpan<T> source)
	{
		public bool TryFirst([NotNullWhen(true)] out T? item)
		{
			if (source.Length == 0)
			{
				item = default;
				return false;
			}

			item = source[0]!;
			return true;
		}

		public bool TryFirst(Func<T, bool> predicate, [NotNullWhen(true)] out T? item) => source.TryFirst(predicate, out item, out _);
		public bool TryFirst(Func<T, bool> predicate, [NotNullWhen(true)] out T? item, out int index)
		{
			if (source.Length == 0)
			{
				item = default;
				index = -1;
				return false;
			}

			index = source.IndexOf(predicate);
			if (index == -1)
			{
				item = default;
				return false;
			}

			item = source[index]!;
			return true;
		}
	}
}
