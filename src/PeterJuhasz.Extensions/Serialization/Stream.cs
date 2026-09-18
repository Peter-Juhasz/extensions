using System.Runtime.CompilerServices;

namespace System.IO;

public static partial class Extensions
{
	extension(Stream stream)
	{
		public async IAsyncEnumerable<ReadOnlyMemory<byte>> ReadExactChunksAsync(int chunkLength, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			while (true)
			{
				var buffer = new byte[chunkLength];
				try
				{
					await stream.ReadExactlyAsync(buffer.AsMemory(), cancellationToken);
				}
				catch (EndOfStreamException)
				{
					yield break;
				}

				yield return buffer.AsMemory();
			}
		}

		public async IAsyncEnumerable<ReadOnlyMemory<byte>> ReadChunksAsync(int bufferSize, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			while (true)
			{
				var buffer = new byte[bufferSize];
				var bytesRead = await stream.ReadAsync(buffer.AsMemory(), cancellationToken);
				if (bytesRead == 0)
				{
					yield break;
				}

				yield return buffer.AsMemory(0, bytesRead);
			}
		}
	}
}
