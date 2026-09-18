using Microsoft.Extensions.ObjectPool;

namespace System.Threading;

public static partial class CancellationTokenSourcePool
{
	public static readonly ObjectPool<CancellationTokenSource> Default = DefaultPool.Create(Policy.Instance);

	public static ObjectPool<CancellationTokenSource> Create(int size = 20)
		=> DefaultPool.Create(Policy.Instance, size);

	public static PooledObject<CancellationTokenSource> GetPooledObject(TimeSpan timeout)
	{
		var rent = Default.GetPooledObject();
		rent.Object.CancelAfter(timeout);
		return rent;
	}

	public static PooledObject<CancellationTokenSource> GetPooledObject(out CancellationTokenSource set) =>
		Default.GetPooledObject(out set);

	public static PooledObject<CancellationTokenSource> GetPooledObject(TimeSpan timeout, out CancellationTokenSource set)
	{
		var rent = Default.GetPooledObject(out set);
		set.CancelAfter(timeout);
		return rent;
	}

	private sealed class Policy : IPooledObjectPolicy<CancellationTokenSource>
	{
		public static readonly Policy Instance = new();

		public CancellationTokenSource Create() => new();

		public bool Return(CancellationTokenSource cts)
		{
			if (cts.TryReset())
			{
				return true;
			}

			cts.Dispose();
			return false;
		}
	}
}
