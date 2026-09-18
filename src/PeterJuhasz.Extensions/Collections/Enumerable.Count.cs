
namespace System.Linq;

public static partial class EnumerableExtension
{
	extension<T>(IReadOnlyList<T> source)
	{
		public int Count(Func<T, bool> predicate) => source.Count(predicate, out _);
		public int Count(Func<T, bool> predicate, out int firstIndex)
		{
			var count = 0;

			firstIndex = source.IndexOf(predicate);
			if (firstIndex == -1)
			{
				return count;
			}

			var index = firstIndex;
			while (index != -1)
			{
				count++;
				index = source.IndexOf(predicate, startIndex: index + 1);
			}

			return count;
		}
	}
}
