using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic;

public class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
{
	public bool Equals(T? x, T? y) => ReferenceEquals(x, y);

	public int GetHashCode(T? obj) => RuntimeHelpers.GetHashCode(obj);

	public static readonly ReferenceEqualityComparer<T> Instance = new();
}

public class DelegateComparer<T, TSelector>(Func<T, TSelector> selector) : IComparer<T>, IEqualityComparer<T>
	where TSelector : IComparable<TSelector>, IEquatable<TSelector>
{
	public int Compare(T? x, T? y)
	{
		if (x is null && y is null) return 0;
		if (x is null) return 1;
		if (y is null) return -1;

		return selector(x).CompareTo(selector(y));
	}

	public bool Equals(T? x, T? y)
	{
		if (x is null && y is null) return true;
		if (x is null) return false;
		if (y is null) return false;

		return selector(x).Equals(selector(y));
	}

	public int GetHashCode([DisallowNull] T obj) => selector(obj).GetHashCode();
}

public class ReverseDelegateComparer<T, TSelector>(Func<T, TSelector> selector) : IComparer<T>, IEqualityComparer<T>
	where TSelector : IComparable<TSelector>, IEquatable<TSelector>
{
	public int Compare(T? x, T? y)
	{
		if (x is null && y is null) return 0;
		if (x is null) return 1;
		if (y is null) return -1;

		return selector(y).CompareTo(selector(x));
	}

	public bool Equals(T? x, T? y)
	{
		if (x is null && y is null) return true;
		if (x is null) return false;
		if (y is null) return false;

		return selector(x).Equals(selector(y));
	}

	public int GetHashCode([DisallowNull] T obj) => selector(obj).GetHashCode();
}