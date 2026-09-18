using Microsoft.Extensions.ObjectPool;
using System.Buffers;

namespace System.Text.Json;

public static partial class Utf8JsonWriterPool
{
	public static readonly ObjectPool<Utf8JsonWriter> Default = DefaultPool.Create(Policy.Instance);

	public static ObjectPool<Utf8JsonWriter> Create(int size = 20)
		=> DefaultPool.Create(Policy.Instance, size);

	public static PooledObject<Utf8JsonWriter> GetPooledObject()
		=> Default.GetPooledObject();

	public static PooledObject<Utf8JsonWriter> GetPooledObject(out Utf8JsonWriter set)
		=> Default.GetPooledObject(out set);

	private sealed class Policy : IPooledObjectPolicy<Utf8JsonWriter>
	{
		public static readonly Policy Instance = new();

		public Utf8JsonWriter Create() => new(NullBufferWriter.Instance, new JsonWriterOptions()
		{
			Indented = false
		});

		public bool Return(Utf8JsonWriter list)
		{
			list.Reset();
			return true;
		}

		private sealed class NullBufferWriter : IBufferWriter<byte>
		{
			public static readonly NullBufferWriter Instance = new();

			public void Advance(int count) { throw new NotSupportedException(); }
			public Memory<byte> GetMemory(int sizeHint = 0) => Memory<byte>.Empty;
			public Span<byte> GetSpan(int sizeHint = 0) => [];
		}
	}
}
