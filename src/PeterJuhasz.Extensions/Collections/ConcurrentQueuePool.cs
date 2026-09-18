using Microsoft.Extensions.ObjectPool;

namespace System.Collections.Concurrent;

public static partial class ConcurrentQueuePool<T>
{
	public static readonly ObjectPool<ConcurrentQueue<T>> Default = DefaultPool.Create(Policy.Instance);

	public static ObjectPool<ConcurrentQueue<T>> Create(int size = 20)
		=> DefaultPool.Create(Policy.Instance, size);

	public static PooledObject<ConcurrentQueue<T>> GetPooledObject()
		=> Default.GetPooledObject();

	public static PooledObject<ConcurrentQueue<T>> GetPooledObject(out ConcurrentQueue<T> set)
		=> Default.GetPooledObject(out set);

	private sealed class Policy : IPooledObjectPolicy<ConcurrentQueue<T>>
	{
		public static readonly Policy Instance = new();

		public ConcurrentQueue<T> Create() => new();

		public bool Return(ConcurrentQueue<T> list)
		{
			list.Clear();
			return true;
		}
	}
}
