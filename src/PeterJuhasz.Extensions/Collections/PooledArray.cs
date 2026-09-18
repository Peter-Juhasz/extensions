using System.Buffers;

namespace System;

public struct PooledArray<T> : IDisposable
{
	private readonly ArrayPool<T> _pool;
	private T[]? _object;

	public readonly T[] Array => _object!;

	private PooledArray(ArrayPool<T> pool, T[] array)
		: this()
	{
		_pool = pool;
		_object = array;
	}

	public static PooledArray<T> Rent(ArrayPool<T> pool, int minimumLength) =>
		new(pool, pool.Rent(minimumLength));

	public static implicit operator T[](in PooledArray<T> pooledArray) => pooledArray.Array;

	public static implicit operator Span<T>(in PooledArray<T> pooledArray) => pooledArray.Array;

	public static implicit operator ReadOnlySpan<T>(in PooledArray<T> pooledArray) => pooledArray.Array;

	public static implicit operator Memory<T>(in PooledArray<T> pooledArray) => pooledArray.Array;

	public static implicit operator ReadOnlyMemory<T>(in PooledArray<T> pooledArray) => pooledArray.Array;

	public void Dispose()
	{
		if (_object is { } obj)
		{
			_pool.Return(obj);
			_object = null;
		}
	}
}

public static partial class Extensions
{
	public static PooledArray<T> GetPooledArray<T>(this ArrayPool<T> pool, int minimumLength) =>
		PooledArray<T>.Rent(pool, minimumLength);
}