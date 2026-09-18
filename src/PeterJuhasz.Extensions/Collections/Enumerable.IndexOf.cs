
namespace System.Linq;

public static partial class EnumerableExtension
{
	extension<T>(IReadOnlyList<T> source) where T : IEquatable<T>
	{
		public int IndexOf(T value, int startIndex = 0)
		{
			for (int i = startIndex; i < source.Count; i++)
			{
				if (source[i].Equals(value))
				{
					return i;
				}
			}

			return -1;
		}
	}

	extension<T>(IReadOnlyList<T> source)
	{
		public int IndexOf(Func<T, bool> predicate, int startIndex = 0)
		{
			for (int i = startIndex; i < source.Count; i++)
			{
				if (predicate(source[i]))
				{
					return i;
				}
			}

			return -1;
		}
		public int LastIndexOf(Func<T, bool> predicate, int startIndex = -1)
		{
			for (int i = (startIndex == -1 ? source.Count - 1 : startIndex); i >= 0; i--)
			{
				if (predicate(source[i]))
				{
					return i;
				}
			}

			return -1;
		}
	}

	extension<T>(ReadOnlySpan<T> source)
	{
		public int IndexOf(Func<T, bool> predicate, int startIndex = 0)
		{
			for (int i = startIndex; i < source.Length; i++)
			{
				if (predicate(source[i]))
				{
					return i;
				}
			}

			return -1;
		}
	}
}
