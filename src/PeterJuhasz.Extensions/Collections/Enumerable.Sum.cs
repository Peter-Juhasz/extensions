using System.Numerics;

namespace System.Linq;

public static partial class EnumerableExtension
{
	extension<T>(IReadOnlyList<T> source)
	{
		public TValue Sum<TValue>(Func<T, TValue> predicate) where TValue : INumber<TValue>
		{
			var sum = TValue.Zero;

			for (int i = 0; i < source.Count; i++)
			{
				sum += predicate(source[i]);
			}

			return sum;
		}
	}
}
