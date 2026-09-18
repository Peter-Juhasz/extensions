using Microsoft.Extensions.ObjectPool;

namespace System.IO;

public static partial class MemoryStreamPool
{
	public static readonly ObjectPool<MemoryStream> Default = DefaultPool.Create(Policy.Instance);

	public static ObjectPool<MemoryStream> Create(int size = 20)
		=> DefaultPool.Create(Policy.Instance, size);

	public static PooledObject<MemoryStream> GetPooledObject()
		=> Default.GetPooledObject();

	public static PooledObject<MemoryStream> GetPooledObject(out MemoryStream list)
		=> Default.GetPooledObject(out list);

	private sealed class Policy(int? initialCapacity = null, int maximumCapacity = 16_384) : IPooledObjectPolicy<MemoryStream>
	{
		public static readonly Policy Instance = new();

		public MemoryStream Create() => initialCapacity is int ic ? new(capacity: ic) : new();

		public bool Return(MemoryStream list)
		{
			list.SetLength(0);

			if (list.Capacity > maximumCapacity)
			{
				list.Capacity = maximumCapacity;
			}

			return true;
		}
	}
}
