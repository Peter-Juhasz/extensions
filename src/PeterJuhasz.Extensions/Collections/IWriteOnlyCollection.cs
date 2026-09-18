using System.Buffers;
using System.Threading.Channels;

namespace System.Collections.Generic;

public interface IWriteOnlyCollection<T>
{
	void Add(T item);
}

public static partial class Extensions
{
	public static IWriteOnlyCollection<T> AsWriteOnly<T>(this ChannelWriter<T> writer) =>
		new ChannelWriterWriteOnlyCollection<T>(writer);

	public static IWriteOnlyCollection<T> AsWriteOnly<T>(this Channel<T> writer) =>
		writer.Writer.AsWriteOnly();

	public static IWriteOnlyCollection<T> AsWriteOnly<T>(this ICollection<T> writer) =>
		new CollectionWriteOnlyCollection<T>(writer);

	public static IWriteOnlyCollection<T> AsWriteOnly<T>(this IBufferWriter<T> writer) =>
		new BufferWriterWriteOnlyCollection<T>(writer);

	private sealed class ChannelWriterWriteOnlyCollection<T>(ChannelWriter<T> writer) : IWriteOnlyCollection<T>
	{
		public void Add(T item) => writer.TryWrite(item);
	}

	private sealed class CollectionWriteOnlyCollection<T>(ICollection<T> writer) : IWriteOnlyCollection<T>
	{
		public void Add(T item) => writer.Add(item);
	}

	private sealed class BufferWriterWriteOnlyCollection<T>(IBufferWriter<T> writer) : IWriteOnlyCollection<T>
	{
		public void Add(T item) => writer.Write([item]);
	}
}