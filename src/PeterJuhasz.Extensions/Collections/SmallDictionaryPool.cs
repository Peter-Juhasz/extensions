using Microsoft.Extensions.ObjectPool;

namespace System.Collections.Generic;

public static partial class SmallDictionaryPool<TKey, TValue>
{
	public static readonly ObjectPool<SmallDictionary<TKey, TValue>> Default = DefaultPool.Create(Policy.Instance);

	public static ObjectPool<SmallDictionary<TKey, TValue>> Create(int size = 20)
		=> DefaultPool.Create(Policy.Instance, size);

	public static PooledObject<SmallDictionary<TKey, TValue>> GetPooledObject()
		=> Default.GetPooledObject();

	public static PooledObject<SmallDictionary<TKey, TValue>> GetPooledObject(out SmallDictionary<TKey, TValue> set)
		=> Default.GetPooledObject(out set);

	private sealed class Policy(int? initialCapacity = null) : IPooledObjectPolicy<SmallDictionary<TKey, TValue>>
	{
		public static readonly Policy Instance = new();

		public SmallDictionary<TKey, TValue> Create() => initialCapacity is int ic ? new(capacity: ic) : [];

		public bool Return(SmallDictionary<TKey, TValue> list)
		{
			list.Clear();
			return true;
		}
	}
}
