
namespace System.Linq;

public static partial class EnumerableExtension
{
	extension<T>(IReadOnlyList<T> source)
	{
		public IEnumerable<T> Distinct() => source.Distinct(EqualityComparer<T>.Default);

		public IEnumerable<T> Distinct(IEqualityComparer<T> comparer)
		{
			if (source.Count <= 1)
			{
				return source;
			}

			if (source.Count == 2)
			{
				if (comparer.Equals(source[0], source[1]))
				{
					return [source[0]];
				}
				else
				{
					return source;
				}
			}

			if (source.Count > 16)
			{
				return ((IEnumerable<T>)source).Distinct(comparer);
			}

			for (int i = 0; i < source.Count; i++)
			{
				for (int j = 0; j < source.Count; j++)
				{
					if (i == j)
					{
						continue;
					}

					if (comparer.Equals(source[i], source[j]))
					{
						return ((IEnumerable<T>)source).Distinct(comparer);
					}
				}
			}

			return source;
		}


		public IEnumerable<T> DistinctBy<TSelector>(Func<T, TSelector> selector) => source.DistinctBy(selector, EqualityComparer<TSelector>.Default);

		public IEnumerable<T> DistinctBy<TSelector>(Func<T, TSelector> selector, IEqualityComparer<TSelector> comparer)
		{
			if (source.Count <= 1)
			{
				return source;
			}

			if (source.Count == 2)
			{
				if (comparer.Equals(selector(source[0]), selector(source[1])))
				{
					return [source[0]];
				}
				else
				{
					return source;
				}
			}

			if (source.Count > 16)
			{
				return ((IEnumerable<T>)source).DistinctBy(selector, comparer);
			}

			for (int i = 0; i < source.Count; i++)
			{
				for (int j = 0; j < source.Count; j++)
				{
					if (i == j)
					{
						continue;
					}

					if (comparer.Equals(selector(source[i]), selector(source[j])))
					{
						return ((IEnumerable<T>)source).DistinctBy(selector, comparer);
					}
				}
			}

			return source;
		}
	}

	extension<T>(IReadOnlySet<T> source)
	{
		public IReadOnlySet<T> Distinct() => source;
	}
}
