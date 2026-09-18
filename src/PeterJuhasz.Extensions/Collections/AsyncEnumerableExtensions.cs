using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace App.Server.Shared;

// https://github.com/dotnet/aspire/blob/main/src/Shared/ChannelExtensions.cs

public static class AsyncEnumerableExtensions
{
	extension<T>(IAsyncEnumerable<T> source)
	{
		public async IAsyncEnumerable<T> Do(Action<T> action, [EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			await foreach (var item in source.WithCancellation(cancellationToken))
			{
				action(item);
				yield return item;
			}
		}
		//[Experimental("TH0156")]
		public async IAsyncEnumerable<ImmutableArray<T>> Chunk(TimeSpan windowLength, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			var channel = Channel.CreateUnbounded<ImmutableArray<T>>(ChunkOptions);
			var writer = channel.Writer;

			using var completedCts = new CancellationTokenSource();
			var completedToken = completedCts.Token;
			using var _1 = ConcurrentQueuePool<T>.GetPooledObject(out var queue);
			_ = ReadAsync();
			_ = WriteAsync();

			await foreach (var chunk in channel.Reader.ReadAllAsync(cancellationToken))
			{
				yield return chunk;
			}

			async Task ReadAsync()
			{
				try
				{
					await foreach (var item in source.WithCancellation(cancellationToken))
					{
						queue.Enqueue(item);
					}

					completedCts.Cancel();
				}
				catch (Exception ex)
				{
					await FlushAsync();
					channel.Writer.TryComplete(ex);
				}
			}

			async Task WriteAsync()
			{
				using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, completedToken);
				try
				{
					using var timer = new PeriodicTimer(windowLength);
					while (await timer.WaitForNextTickAsync(cts.Token))
					{
						await FlushAsync();
					}
				}
				catch (OperationCanceledException) when (completedCts.IsCancellationRequested)
				{
					await FlushAsync();
					writer.TryComplete();
				}
				catch (Exception ex)
				{
					await FlushAsync();
					writer.TryComplete(ex);
				}
			}

			ValueTask FlushAsync()
			{
				using var buffer = new PooledArrayBuilder<T>();
				while (queue.TryDequeue(out var item))
				{
					buffer.Add(item);
				}

				if (buffer.Any())
				{
					return writer.WriteAsync(buffer.ToImmutableArray(), cancellationToken);
				}
				else
				{
					return ValueTask.CompletedTask;
				}
			}
		}

		[Experimental("TH0157")]
		public async IAsyncEnumerable<ImmutableArray<T>> ChunkUntil(TimeSpan windowLength, int maxLength, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			await using var enumerator = source.GetAsyncEnumerator(cancellationToken);

			// get first item
			var hasMore = await enumerator.MoveNextAsync();
			if (!hasMore)
			{
				yield break;
			}

			using var _1 = ListPool<T>.GetPooledObject(out var buffer);
			buffer.Add(enumerator.Current);

			var delayTask = Task.Delay(windowLength, cancellationToken);

			// read next items
			while (hasMore)
			{
				var moveNextTask = enumerator.MoveNextAsync().AsTask();
				var completedTask = await Task.WhenAny(moveNextTask, delayTask);

				if (completedTask == moveNextTask)
				{
					hasMore = moveNextTask.Result;

					if (hasMore)
					{
						buffer.Add(enumerator.Current);

						// reset
						delayTask = Task.Delay(windowLength, cancellationToken);

						// flush
						if (buffer.Count >= maxLength)
						{
							yield return buffer.ToImmutableArray();
							buffer.Clear();
						}
					}
				}
				else
				{
					// flush
					if (buffer.Any())
					{
						yield return buffer.ToImmutableArray();
						buffer.Clear();
					}

					// reset
					delayTask = Task.Delay(windowLength, cancellationToken);
				}
			}

			// final flush
			if (buffer.Any())
			{
				yield return buffer.ToImmutableArray();
			}
		}
	}

	extension<TSource>(IAsyncEnumerable<TSource> source)
	{
		public async IAsyncEnumerable<TResult> SelectWhere<TResult>(Func<TSource, (bool Matches, TResult Item)> selector, [EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			await foreach (var item in source.WithCancellation(cancellationToken))
			{
				var selection = selector(item);
				if (selection.Matches)
				{
					yield return selection.Item;
				}
			}
		}

		public async IAsyncEnumerable<TResult> SelectMany<TResult>(
			Func<TSource, IEnumerable<TResult>> selector,
			[EnumeratorCancellation] CancellationToken cancellationToken
		)
		{
			await foreach (var item in source.WithCancellation(cancellationToken))
			{
				foreach (var result in selector(item))
				{
					yield return result;
				}
			}
		}


		public IAsyncEnumerable<TResult> SelectParallelAsync<TResult>(
			Func<TSource, CancellationToken, ValueTask<TResult>> selector,
			CancellationToken cancellationToken
		) =>
			SelectParallelAsync(source, selector, new ParallelOptions { CancellationToken = cancellationToken });

		public IAsyncEnumerable<TResult> SelectParallelAsync<TResult>(
			Func<TSource, CancellationToken, ValueTask<TResult>> selector,
			ParallelOptions parallelOptions
		)
		{
			var channel = Channel.CreateUnbounded<TResult>(new UnboundedChannelOptions
			{
				SingleReader = true,
				SingleWriter = false,
			});
			var writer = channel.Writer;

			_ = Run();

			return channel.Reader.ReadAllAsync(parallelOptions.CancellationToken);

			async Task Run()
			{
				try
				{
					await Parallel.ForEachAsync(source, parallelOptions, async (item, ct) =>
					{
						var result = await selector(item, ct);
						await writer.WriteAsync(result, ct);
					});
				}
				catch (Exception ex)
				{
					writer.TryComplete(ex);
				}
				finally
				{
					writer.TryComplete();
				}
			}
		}


		public IAsyncEnumerable<TResult> SelectParallelSafeAsync<TResult>(
			Func<TSource, CancellationToken, ValueTask<TResult>> selector,
			ILogger logger,
			CancellationToken cancellationToken
		) =>
			SelectParallelSafeAsync(source, selector, logger, new ParallelOptions { CancellationToken = cancellationToken });

		public IAsyncEnumerable<TResult> SelectParallelSafeAsync<TResult>(
			Func<TSource, CancellationToken, ValueTask<TResult>> selector,
			ILogger logger,
			ParallelOptions parallelOptions
		)
		{
			var channel = Channel.CreateUnbounded<TResult>(new UnboundedChannelOptions
			{
				SingleReader = true,
				SingleWriter = false,
			});
			var writer = channel.Writer;

			_ = Run();

			return channel.Reader.ReadAllAsync(parallelOptions.CancellationToken);

			async Task Run()
			{
				try
				{
					await Parallel.ForEachAsync(source, parallelOptions, async (item, ct) =>
					{
						try
						{
							var result = await selector(item, ct);
							await writer.WriteAsync(result, ct);
						}
						catch (OperationCanceledException) when (ct.IsCancellationRequested)
						{
							throw;
						}
						catch (Exception ex)
						{
							logger.LogWarning(ex, "Error processing item: {Item}", item);
						}
					});
				}
				catch (Exception ex)
				{
					writer.TryComplete(ex);
				}
				finally
				{
					writer.TryComplete();
				}
			}
		}
	}

	extension<T>(IAsyncEnumerable<ImmutableArray<T>> source)
	{
		public async IAsyncEnumerable<T> SelectMany(
			[EnumeratorCancellation] CancellationToken cancellationToken
		)
		{
			await foreach (var array in source.WithCancellation(cancellationToken))
			{
				foreach (var result in array)
				{
					yield return result;
				}
			}
		}
	}

	extension<T>(IAsyncEnumerable<T[]> source)
	{
		public async IAsyncEnumerable<T> SelectMany(
			[EnumeratorCancellation] CancellationToken cancellationToken
		)
		{
			await foreach (var array in source.WithCancellation(cancellationToken))
			{
				foreach (var result in array)
				{
					yield return result;
				}
			}
		}
	}

	private static readonly UnboundedChannelOptions ChunkOptions = new()
	{
		SingleReader = true,
		SingleWriter = true,
	};
}