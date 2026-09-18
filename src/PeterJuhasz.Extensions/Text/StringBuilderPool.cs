using Microsoft.Extensions.ObjectPool;

namespace System.Text;

public static partial class StringBuilderPool
{
	private static readonly StringBuilderPooledObjectPolicy DefaultPolicy = new()
	{
		MaximumRetainedCapacity = 128_000
	};
	public static readonly ObjectPool<StringBuilder> Default = DefaultPool.Create(DefaultPolicy);

	public static ObjectPool<StringBuilder> Create(int size = 20)
		=> DefaultPool.Create(DefaultPolicy, size);

	public static PooledObject<StringBuilder> GetPooledObject()
		=> Default.GetPooledObject();

	public static PooledObject<StringBuilder> GetPooledObject(out StringBuilder list)
		=> Default.GetPooledObject(out list);
}
