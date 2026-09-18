using Microsoft.Extensions.ObjectPool;

namespace System.Collections.Immutable;

public static partial class ImmutableArrayBuilderPool<T>
{
	public static readonly ObjectPool<ImmutableArray<T>.Builder> Default = DefaultPool.Create(Policy.Instance);

	public static ObjectPool<ImmutableArray<T>.Builder> Create(int size = 20)
		=> DefaultPool.Create(Policy.Instance, size);

	public static PooledObject<ImmutableArray<T>.Builder> GetPooledObject()
		=> Default.GetPooledObject();

	public static PooledObject<ImmutableArray<T>.Builder> GetPooledObject(out ImmutableArray<T>.Builder list)
		=> Default.GetPooledObject(out list);

	private sealed class Policy : IPooledObjectPolicy<ImmutableArray<T>.Builder>
	{
		public static readonly Policy Instance = new();

		public ImmutableArray<T>.Builder Create() => ImmutableArray.CreateBuilder<T>();

		public bool Return(ImmutableArray<T>.Builder list)
		{
			var count = list.Count;

			list.Clear();

			if (list.Capacity > DefaultPool.MaximumItemCount)
			{
				list.Capacity = DefaultPool.MaximumItemCount;
			}

			return true;
		}
	}
}
