namespace System.Net.Http;

public sealed class BinaryDataContent(BinaryData data) : HttpContent()
{
	public BinaryData Data => data;


	protected override Stream CreateContentReadStream(CancellationToken cancellationToken) => Data.ToStream();

	protected override Task<Stream> CreateContentReadStreamAsync() => this.CreateContentReadStreamAsync(CancellationToken.None);

	protected override Task<Stream> CreateContentReadStreamAsync(CancellationToken cancellationToken)
	{
		return Task.FromResult(Data.ToStream());
	}


	protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
		this.SerializeToStreamAsync(stream, context, CancellationToken.None);

	protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
	{
		await stream.WriteAsync(data.ToMemory(), cancellationToken);
		await stream.FlushAsync(cancellationToken);
	}

	protected override void SerializeToStream(Stream stream, TransportContext? context, CancellationToken cancellationToken)
	{
		stream.Write(data.ToMemory().Span);
		stream.Flush();
	}


	protected override bool TryComputeLength(out long length)
	{
		length = data.Length;
		return true;
	}
}
