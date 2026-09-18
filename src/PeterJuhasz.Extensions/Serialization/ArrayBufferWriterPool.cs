using Microsoft.Extensions.ObjectPool;

namespace System.Buffers;

public static partial class ArrayBufferWriterPool<T>
{
	public static readonly ObjectPool<ArrayBufferWriter<T>> Default = DefaultPool.Create(Policy.Instance);

	public static ObjectPool<ArrayBufferWriter<T>> Create(int size = 20)
		=> DefaultPool.Create(Policy.Instance, size);

	public static PooledObject<ArrayBufferWriter<T>> GetPooledObject()
		=> Default.GetPooledObject();

	public static PooledObject<ArrayBufferWriter<T>> GetPooledObject(out ArrayBufferWriter<T> set)
		=> Default.GetPooledObject(out set);

	private sealed class Policy : IPooledObjectPolicy<ArrayBufferWriter<T>>
	{
		public static readonly Policy Instance = new();

		private const int MaxCapacity = 1 * 1024 * 1024;

		public ArrayBufferWriter<T> Create() => new();

		public bool Return(ArrayBufferWriter<T> list)
		{
			if (list.Capacity > MaxCapacity)
			{
				return false;
			}

			list.Clear();
			return true;
		}
	}
}
