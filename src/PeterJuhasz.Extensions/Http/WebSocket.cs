using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace System.Net.Http;

public static partial class Extensions
{
	extension(WebSocket socket)
	{
		[RequiresUnreferencedCode("Uses reflection to create generic types at runtime.")]
		public async ValueTask SendAsJsonAsync<T>(T obj, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default)
		{
			using var _1 = ArrayBufferWriterPool<byte>.GetPooledObject(out var buffer);
			using var _2 = Utf8JsonWriterPool.GetPooledObject(out var writer);
			writer.Reset(buffer);

			JsonSerializer.Serialize(writer, obj, jsonSerializerOptions);

			await socket.SendAsync(buffer.WrittenMemory, WebSocketMessageType.Text, true, cancellationToken);
		}

		[RequiresUnreferencedCode("Uses reflection to create generic types at runtime.")]
		public async ValueTask SendAsJsonAsync<T>(T obj, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
		{
			using var _1 = ArrayBufferWriterPool<byte>.GetPooledObject(out var buffer);
			using var _2 = Utf8JsonWriterPool.GetPooledObject(out var writer);
			writer.Reset(buffer);

			JsonSerializer.Serialize(writer, obj, jsonTypeInfo);

			await socket.SendAsync(buffer.WrittenMemory, WebSocketMessageType.Text, true, cancellationToken);
		}

		[RequiresUnreferencedCode("Uses reflection to create generic types at runtime.")]
		public IAsyncEnumerable<T> ReadAsJsonAsync<T>(JsonSerializerOptions? jsonSerializerOptions, WebSocketJsonReaderOptions options, CancellationToken cancellationToken) =>
			socket.ReadAsJsonAsync<T>((jsonSerializerOptions ?? JsonSerializerOptions.Default).GetTypeInfo<T>(), options, cancellationToken);

		[RequiresUnreferencedCode("Uses reflection to create generic types at runtime.")]
		public async IAsyncEnumerable<T> ReadAsJsonAsync<T>(JsonTypeInfo<T> jsonTypeInfo, WebSocketJsonReaderOptions options, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			using var _ = ArrayBufferWriterPool<byte>.GetPooledObject(out var buffer);
			while (true)
			{
				cancellationToken.ThrowIfCancellationRequested();

				// stop if closed
				if (socket.State is WebSocketState.Closed or WebSocketState.CloseReceived or WebSocketState.Aborted)
				{
					break;
				}

				// clear buffer
				buffer.Clear();

				// read a full message, which may be multiple frames
				try
				{
					ValueWebSocketReceiveResult result;
					do
					{
						result = await socket.ReceiveAsync(buffer.GetMemory(options.ChunkSize), cancellationToken);
						buffer.Advance(result.Count);

						if (result.MessageType == WebSocketMessageType.Close)
						{
							yield break;
						}

						if (buffer.WrittenCount > options.MaxMessageBytes)
						{
							throw new InvalidOperationException($"WebSocket message exceeded {options.MaxMessageBytes} bytes.");
						}
					}
					while (!result.EndOfMessage);

					// ignore non-text/binary messages
					if (result.MessageType is not (WebSocketMessageType.Text or WebSocketMessageType.Binary))
					{
						continue;
					}
				}
				catch (WebSocketException wsex) when (wsex.WebSocketErrorCode == WebSocketError.InvalidState)
				{
					break;
				}
				catch (ObjectDisposedException)
				{
					break;
				}
				catch (WebSocketException)
				{
					throw;
				}
				catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
				{
					break;
				}

				// deserialize
				T? @event;
				try
				{
					@event = JsonSerializer.Deserialize<T>(buffer.WrittenSpan, jsonTypeInfo);
				}
				catch (JsonException jex) when (options.OnError != null)
				{
					options.OnError(jex);
					continue;
				}

				// process
				if (@event is null)
				{
					continue;
				}

				yield return @event;
			}
		}
	}
}

public record class WebSocketJsonReaderOptions(
	int ChunkSize = 16 * 1024,
	int MaxMessageBytes = 10 * 1024 * 1024,
	Action<Exception>? OnError = null
);