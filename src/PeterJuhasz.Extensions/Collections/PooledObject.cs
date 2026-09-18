namespace Microsoft.Extensions.ObjectPool;

public struct PooledObject<T> : IDisposable
	where T : class
{
	private readonly ObjectPool<T> _pool;
	private T? _object;

	public readonly T Object => _object!;

	public PooledObject(ObjectPool<T> pool)
		: this()
	{
		_pool = pool;
		_object = pool.Get();
	}

	public void Dispose()
	{
		if (_object is { } obj)
		{
			_pool.Return(obj);
			_object = null;
		}
	}
}

internal static class DefaultPool
{
	public const int MaximumItemCount = 512;

	public static ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy, int size = 20)
		where T : class
		=> new DefaultObjectPool<T>(policy, size);

	public static PooledObject<T> GetPooledObject<T>(this ObjectPool<T> pool)
		where T : class
		=> new(pool);

	public static PooledObject<T> GetPooledObject<T>(this ObjectPool<T> pool, out T obj)
		where T : class
	{
		var pooledObject = pool.GetPooledObject();
		obj = pooledObject.Object;
		return pooledObject;
	}
}
