using Microsoft.Extensions.ObjectPool;

namespace System.Collections.Generic;

public static partial class ListPool<T>
{
	public static readonly ObjectPool<List<T>> Default = DefaultPool.Create(Policy.Instance);

	public static ObjectPool<List<T>> Create(int size = 20)
		=> DefaultPool.Create(Policy.Instance, size);

	public static PooledObject<List<T>> GetPooledObject()
		=> Default.GetPooledObject();

	public static PooledObject<List<T>> GetPooledObject(out List<T> list)
		=> Default.GetPooledObject(out list);

	private sealed class Policy(int? initialCapacity = null) : IPooledObjectPolicy<List<T>>
	{
		public static readonly Policy Instance = new();

		public List<T> Create() => initialCapacity is int ic ? new(capacity: ic) : [];

		public bool Return(List<T> list)
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
