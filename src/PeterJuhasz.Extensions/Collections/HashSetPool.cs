using Microsoft.Extensions.ObjectPool;

namespace System.Collections;

public static partial class HashSetPool<T>
{
	public static readonly ObjectPool<HashSet<T>> Default = DefaultPool.Create(Policy<T>.Instance);

	public static readonly ObjectPool<HashSet<string>> IgnoreCasePool = DefaultPool.Create(new Policy<string>(StringComparer.OrdinalIgnoreCase));

	public static ObjectPool<HashSet<T>> Create(int size = 20)
		=> DefaultPool.Create(Policy<T>.Instance, size);

	public static PooledObject<HashSet<T>> GetPooledObject()
		=> Default.GetPooledObject();

	public static PooledObject<HashSet<T>> GetPooledObject(out HashSet<T> set)
		=> Default.GetPooledObject(out set);

	public static PooledObject<HashSet<string>> GetIgnoreCasePooledObject(out HashSet<string> set)
		=> IgnoreCasePool.GetPooledObject(out set);

#pragma warning disable CS0693 // Type parameter has the same name as the type parameter from outer type
	private sealed class Policy<T>(IEqualityComparer<T>? comparer = null, int? initialCapacity = null) : IPooledObjectPolicy<HashSet<T>>
	{
		public static readonly Policy<T> Instance = new();

		public HashSet<T> Create() => initialCapacity is int ic ? new(capacity: ic, comparer) : [];

		public bool Return(HashSet<T> list)
		{
			var count = list.Count;

			list.Clear();

			if (count > DefaultPool.MaximumItemCount)
			{
				list.TrimExcess();
			}

			return true;
		}
	}
}

public static partial class SmallSetPool<T> where T : notnull
{
	public static readonly ObjectPool<SmallSet<T>> Default = DefaultPool.Create(Policy.Instance);

	public static ObjectPool<SmallSet<T>> Create(int size = 20)
		=> DefaultPool.Create(Policy.Instance, size);

	public static PooledObject<SmallSet<T>> GetPooledObject()
		=> Default.GetPooledObject();

	public static PooledObject<SmallSet<T>> GetPooledObject(out SmallSet<T> set)
		=> Default.GetPooledObject(out set);

	private sealed class Policy(int? initialCapacity = null) : IPooledObjectPolicy<SmallSet<T>>
	{
		public static readonly Policy Instance = new();

		public SmallSet<T> Create() => initialCapacity is int ic ? new(capacity: ic) : [];

		public bool Return(SmallSet<T> list)
		{
			list.Clear();
			return true;
		}
	}
}
