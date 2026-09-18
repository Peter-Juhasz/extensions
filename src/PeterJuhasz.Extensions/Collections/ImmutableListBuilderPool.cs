using Microsoft.Extensions.ObjectPool;

namespace System.Collections.Immutable;

public static partial class ImmutableListBuilderPool<T>
{
	public static readonly ObjectPool<ImmutableList<T>.Builder> Default = DefaultPool.Create(Policy.Instance);

	public static ObjectPool<ImmutableList<T>.Builder> Create(int size = 20)
		=> DefaultPool.Create(Policy.Instance, size);

	public static PooledObject<ImmutableList<T>.Builder> GetPooledObject()
		=> Default.GetPooledObject();

	public static PooledObject<ImmutableList<T>.Builder> GetPooledObject(out ImmutableList<T>.Builder list)
		=> Default.GetPooledObject(out list);

	private sealed class Policy : IPooledObjectPolicy<ImmutableList<T>.Builder>
	{
		public static readonly Policy Instance = new();

		public ImmutableList<T>.Builder Create() => ImmutableList.CreateBuilder<T>();

		public bool Return(ImmutableList<T>.Builder list)
		{
			var count = list.Count;
			list.Clear();
			return true;
		}
	}
}
