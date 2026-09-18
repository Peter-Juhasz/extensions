using Microsoft.Extensions.Logging.Abstractions;

namespace System.Threading.Channels;

public class MergedAsyncEnumerable<T>
{
	private List<IAsyncEnumerable<T>>? _enumerables;

	private static readonly UnboundedChannelOptions _options = new()
	{
		SingleReader = true,
		SingleWriter = false,
	};

	public void Add(IAsyncEnumerable<T> enumerable)
	{
		_enumerables ??= [];
		_enumerables.Add(enumerable);
	}

	public IAsyncEnumerable<T> StartAsync(CancellationToken cancellationToken = default)
	{
		if (_enumerables is null or { Count: 0 })
		{
			return AsyncEnumerable.Empty<T>();
		}

		if (_enumerables is [var single])
		{
			return single;
		}

		var channel = Channel.CreateUnbounded<T>(_options);
		var writer = channel.Writer;
		var completedCount = 0;

		var tasks = new Task[_enumerables.Count];
		for (var i = 0; i < _enumerables.Count; i++)
		{
			var enumerable = _enumerables[i];
			tasks[i] = CopyAsync(enumerable);
		}

		_ = Task.WhenAll(tasks).CatchAll(NullLogger.Instance);
		_enumerables = null;

		return channel.Reader.ReadAllAsync(cancellationToken);

		async Task CopyAsync(IAsyncEnumerable<T> enumerable)
		{
			try
			{
				await foreach (var item in enumerable.WithCancellation(cancellationToken))
				{
					writer.TryWrite(item);
				}
			}
			catch (Exception ex)
			{
				writer.TryComplete(ex);
			}
			finally
			{
				if (Interlocked.Increment(ref completedCount) == tasks.Length)
				{
					writer.TryComplete();
				}
			}
		}
	}
}


public static partial class Extensions
{
	public static IAsyncEnumerable<T> MergeParallel<T>(
		this ReadOnlySpan<IAsyncEnumerable<T>> source,
		CancellationToken cancellationToken
	)
	{
		if (source.IsEmpty)
		{
			return AsyncEnumerable.Empty<T>();
		}

		if (source is [var single])
		{
			return single;
		}

		var merged = new MergedAsyncEnumerable<T>();
		foreach (var enumerable in source)
		{
			merged.Add(enumerable);
		}
		return merged.StartAsync(cancellationToken);
	}
}