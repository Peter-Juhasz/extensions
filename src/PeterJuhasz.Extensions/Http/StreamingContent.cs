namespace System.Net.Http;

public sealed class StreamingContent(Func<Stream, CancellationToken, Task> task) : HttpContent()
{
	protected override Stream CreateContentReadStream(CancellationToken cancellationToken) => CreateContentReadStreamAsync(cancellationToken).Result;

	protected override Task<Stream> CreateContentReadStreamAsync() => this.CreateContentReadStreamAsync(CancellationToken.None);

	protected override async Task<Stream> CreateContentReadStreamAsync(CancellationToken cancellationToken)
	{
		var memoryStream = new MemoryStream();
		await task(memoryStream, cancellationToken);
		memoryStream.Position = 0;
		return memoryStream;
	}


	protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
		this.SerializeToStreamAsync(stream, context, CancellationToken.None);

	protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
	{
		await task(stream, cancellationToken);
		await stream.FlushAsync(cancellationToken);
	}

	protected override void SerializeToStream(Stream stream, TransportContext? context, CancellationToken cancellationToken) => task(stream, cancellationToken).Wait(cancellationToken);


	protected override bool TryComputeLength(out long length)
	{
		length = -1;
		return false;
	}
}
