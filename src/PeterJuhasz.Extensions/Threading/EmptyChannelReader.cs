using System.Diagnostics.CodeAnalysis;

namespace System.Threading.Channels;

public sealed class EmptyChannelReader<T> : ChannelReader<T>
{
	public override int Count => 0;

	public override bool CanCount => true;

	public override bool CanPeek => true;

	public override Task Completion => Task.CompletedTask;

	public override bool TryRead([MaybeNullWhen(false)] out T item)
	{
		item = default!;
		return false;
	}

	public override bool TryPeek([MaybeNullWhen(false)] out T item)
	{
		item = default;
		return false;
	}

	public override ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default) => new(false);

	private static readonly TaskCompletionSource<T> _never = new();

	public override ValueTask<T> ReadAsync(CancellationToken cancellationToken = default) => new(_never.Task);

	public static readonly EmptyChannelReader<T> Instance = new();
}