using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace System.Threading.Channels;

public sealed class ForkedAsyncEnumerable<T>
{
	private List<Channel<T>>? _builder = new();
	private Channel<T>[]? _channels = null;
	private bool _isStarted = false;

	private static readonly UnboundedChannelOptions _options = new()
	{
		SingleReader = true,
		SingleWriter = true,
	};

	public IAsyncEnumerable<T> Fork(CancellationToken cancellationToken)
	{
		if (_isStarted)
		{
			throw new InvalidOperationException("The ForkedAsyncEnumerable has already been started.");
		}

		Debug.Assert(_builder != null);

		var channel = Channel.CreateUnbounded<T>(_options);
		_builder.Add(channel);
		return channel.Reader.ReadAllAsync(cancellationToken);
	}

	public Task StartAsync(IAsyncEnumerable<T> source, CancellationToken cancellationToken)
	{
		if (Interlocked.CompareExchange(ref _isStarted, true, false) == true)
		{
			throw new InvalidOperationException("The ForkedAsyncEnumerable has already been started.");
		}

		if (_builder is null or { Count: 0 })
		{
			return Task.CompletedTask;
		}

		cancellationToken.ThrowIfCancellationRequested();

		_channels = [.. _builder];
		_builder = null;

		return Copy(source, cancellationToken);
	}

	private async Task Copy(IAsyncEnumerable<T> source, CancellationToken cancellationToken)
	{
		Debug.Assert(_channels != null);

		var channels = _channels;

		try
		{
			await foreach (var item in source.WithCancellation(cancellationToken))
			{
				foreach (var channel in channels)
				{
					channel.Writer.TryWrite(item);
				}
			}
		}
		catch (Exception ex)
		{
			foreach (var channel in channels)
			{
				channel.Writer.TryComplete(ex);
			}
		}
		finally
		{
			foreach (var channel in channels)
			{
				channel.Writer.TryComplete();
			}
		}
	}
}

public static partial class Extensions
{
	extension<T>(IAsyncEnumerable<T> source)
	{
		public (IAsyncEnumerable<T> first, IAsyncEnumerable<T> second) Fork(ILogger logger, CancellationToken cancellationToken)
		{
			var forked = new ForkedAsyncEnumerable<T>();
			var first = forked.Fork(cancellationToken);
			var second = forked.Fork(cancellationToken);
			_ = forked.StartAsync(source, cancellationToken).CatchAll(logger);
			return (first, second);
		}

		public IAsyncEnumerable<T>[] Fork(int count, ILogger logger, CancellationToken cancellationToken)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(count);

			if (count == 0)
			{
				return [];
			}

			if (count == 1)
			{
				return [source];
			}

			var forked = new ForkedAsyncEnumerable<T>();
			var channels = new IAsyncEnumerable<T>[count];
			for (var i = 0; i < count; i++)
			{
				channels[i] = forked.Fork(cancellationToken);
			}
			_ = forked.StartAsync(source, cancellationToken).CatchAll(logger);
			return channels;
		}

		public IAsyncEnumerable<T> Fork(Func<IAsyncEnumerable<T>, CancellationToken, Task> fork, ILogger logger, CancellationToken cancellationToken)
		{
			var (first, second) = source.Fork(logger, cancellationToken);
			_ = fork(second, cancellationToken).CatchAll(logger);
			return first;
		}
	}
}