using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System;

public static class IntExtensions
{
	private static readonly string?[] SmallIntCache = new string?[100];

	extension(long value)
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ToStringInvariant() => value.ToString(CultureInfo.InvariantCulture);

		public string ToStringInvariantCached() => LongCache.GetOrAdd(value, static l => l.ToStringInvariant());
	}

	extension(int value)
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ToStringInvariant()
		{
			if (value >= 0 && value < SmallIntCache.Length)
			{
				ref var cached = ref SmallIntCache[value];
				if (cached == null)
				{
					cached = value.ToString(CultureInfo.InvariantCulture);
				}

				return cached;
			}

			return value.ToString(CultureInfo.InvariantCulture);
		}

		public string ToStringInvariantCached() => BigIntCache.GetOrAdd(value, static l => l.ToStringInvariant());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int AbsoluteValue() => Math.Abs(value);
	}

	private static readonly ConcurrentDictionary<long, string> LongCache = new();
	private static readonly ConcurrentDictionary<int, string> BigIntCache = new();
}
