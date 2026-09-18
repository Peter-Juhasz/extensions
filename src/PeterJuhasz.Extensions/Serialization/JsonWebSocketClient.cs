using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;

namespace App.Core.Serialization;

[RequiresUnreferencedCode("Uses reflection to create generic types at runtime.")]
public abstract class JsonWebSocketClient<TInbound, TOutbound>(
	WebSocket webSocket,
	JsonTypeInfo<TInbound> inboundTypeInfo,
	JsonTypeInfo<TOutbound> outboundTypeInfo,
	ILogger logger
)
{
	public WebSocket WebSocket => webSocket;

	public virtual async IAsyncEnumerable<TInbound> ReceiveAsync([EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var loggedErrorCount = 0;
		const int MaxLoggedErrorCount = 5;

		await foreach (var @event in webSocket.ReadAsJsonAsync<TInbound>(inboundTypeInfo, new WebSocketJsonReaderOptions(
			OnError: LogExceptionSampled
		), cancellationToken))
		{
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}

			yield return @event;
		}

		void LogExceptionSampled(Exception ex)
		{
			if (loggedErrorCount++ < MaxLoggedErrorCount)
			{
				logger.LogError(ex, "Exception while listening to Twilio media stream.");
			}
		}
	}

	public virtual ValueTask SendAsync(TOutbound response, CancellationToken cancellationToken)
	{
		return webSocket.SendAsJsonAsync(response, outboundTypeInfo, cancellationToken);
	}
}
