using App.Server.Shared;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace System.Threading.Channels;

public class MultiProcessorChannel<TItem, TKey>(
	IAsyncEnumerable<TItem> source,
	Func<TItem, TKey> keySelector
)
	where TKey : notnull
{
	private readonly ConcurrentDictionary<TKey, MultiContributorWrapper<TItem>> processedItems = new();
	private readonly List<Func<IAsyncEnumerable<TItem>, CancellationToken, Task>> processorFactory = new();
	private readonly Channel<TItem> channel = Channel.CreateUnbounded<TItem>(new UnboundedChannelOptions
	{
		SingleReader = true,
		SingleWriter = false
	});
	private int finishedProcessors = 0;

	public int ProcessorCount => processorFactory.Count;

	public void AddProcessor<TResult>(
		Func<IAsyncEnumerable<TItem>, IAsyncEnumerable<KeyValuePair<TItem, TResult>>> processor,
		Func<TItem, TResult, TItem> update,
		Func<TResult, bool> hasValue
	)
	{
		processorFactory.Add(async (source, ct) =>
		{
			try
			{
				await foreach (var (item, value) in processor(source).WithCancellation(ct))
				{
					var key = keySelector(item);
					if (!processedItems.TryGetValue(key, out var wrapper))
					{
						continue;
					}

					if (hasValue(value))
					{
						if (wrapper.Apply(x => update(x, value)))
						{
							channel.Writer.TryWrite(wrapper.Value);
							processedItems.TryRemove(key, out _);
						}
					}
					else
					{
						if (wrapper.Add())
						{
							channel.Writer.TryWrite(wrapper.Value);
							processedItems.TryRemove(key, out _);
						}
					}
				}
			}
			catch (Exception ex)
			{
				channel.Writer.TryComplete(ex);
			}
			finally
			{
				if (Interlocked.Increment(ref finishedProcessors) == processorFactory.Count)
				{
					channel.Writer.TryComplete();
				}
			}
		});
	}

	public void AddProcessor<TResult>(
		Func<IAsyncEnumerable<TItem>, IAsyncEnumerable<TItem>> processor,
		Func<TItem, TResult> get,
		Func<TItem, TResult, TItem> update,
		Func<TResult, bool> hasValue
	) => AddProcessor(
		source => processor(source).Select(item => KeyValuePair.Create(item, get(item))),
		update,
		hasValue
	);

	public void AddProcessor(
		Func<IAsyncEnumerable<TItem>, IAsyncEnumerable<TItem>> processor,
		Func<TItem, TItem, TItem> update,
		Func<TItem, bool> hasValue
	) => AddProcessor(
		source => processor(source),
		get: static item => item,
		update,
		hasValue
	);

	public IAsyncEnumerable<TItem> GetProcessedItems(CancellationToken cancellationToken)
	{
		if (processorFactory.Count == 0)
		{
			return source;
		}

		return channel.Reader.ReadAllAsync(cancellationToken);
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		if (processorFactory.Count == 0)
		{
			return Task.CompletedTask;
		}

		// create processors
		var fork = new ForkedAsyncEnumerable<TItem>();
		var tasks = new Task[processorFactory.Count];
		for (int i = 0; i < processorFactory.Count; i++)
		{
			var factory = processorFactory[i];
			tasks[i] = factory(fork.Fork(cancellationToken), cancellationToken);
		}
		var processorTasks = Task.WhenAll(tasks);

		// start fork
		var wrapperFactory = source.Do(c =>
		{
			var wrapper = new MultiContributorWrapper<TItem>(c, tasks.Length);
			processedItems.TryAdd(keySelector(c), wrapper); // items with same id are ignored and deduplicated
		}, cancellationToken);
		var forkTask = fork.StartAsync(wrapperFactory, cancellationToken);

		return Task.WhenAll(forkTask, processorTasks);
	}
}

public static partial class Extensions
{
	public static IAsyncEnumerable<T> AugmentParallelAsync<T, TKey>(
		this IAsyncEnumerable<T> source,
		Func<T, TKey> keySelector,
		Action<MultiProcessorChannel<T, TKey>> builder,
		ILogger logger,
		CancellationToken cancellationToken
	)
		where TKey : notnull
	{
		var channel = new MultiProcessorChannel<T, TKey>(source, keySelector);
		builder(channel);

		if (channel.ProcessorCount == 0)
		{
			return source;
		}

		var processed = channel.GetProcessedItems(cancellationToken);
		_ = channel.StartAsync(cancellationToken).CatchAll(logger);
		return processed;
	}
}