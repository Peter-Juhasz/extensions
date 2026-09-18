using Microsoft.Extensions.Logging;

namespace System.Threading.Channels;

public static partial class Extensions
{
	private static readonly UnboundedChannelOptions SingleWriterReaderOptions = new()
	{
		SingleReader = true,
		SingleWriter = true,
	};

	extension<T>(IAsyncEnumerable<T> source)
	{
		public async Task ForkBy<TSelector>(
		Func<T, TSelector> selector,
		Func<IAsyncEnumerable<T>, CancellationToken, Task> fork,
		ILogger logger,
		CancellationToken cancellationToken
	)
		{
			var forks = new SmallDictionary<TSelector, ChannelWriter<T>>();
			var tasks = new List<Task>();

			await foreach (var item in source.WithCancellation(cancellationToken))
			{
				// compute key
				var key = selector(item);

				// get or create channel writer
				if (!forks.TryGetValue(key, out var writer))
				{
					// create channel
					var channel = Channel.CreateUnbounded<T>(SingleWriterReaderOptions);
					writer = channel.Writer;
					forks.Add(key, writer);

					// start processor
					tasks.Add(fork(channel.Reader.ReadAllAsync(cancellationToken), cancellationToken).CatchAll(logger));
				}

				// write item
				writer.TryWrite(item);
			}

			// complete all writers
			foreach (var (_, writer) in forks)
			{
				writer.TryComplete();
			}

			await Task.WhenAll(tasks);
		}
	}
}
