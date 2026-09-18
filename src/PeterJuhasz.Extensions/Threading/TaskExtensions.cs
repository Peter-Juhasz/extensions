using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace System.Threading.Tasks;

public static class TaskExtensions
{
	extension(Task task)
	{
		/// <summary>
		/// Catches all exceptions thrown by the task and logs them using the provided logger.
		/// </summary>
		public async Task CatchAll(ILogger logger, LogLevel logLevel = LogLevel.Warning)
		{
			try
			{
				await task;
			}
			catch (Exception ex)
			{
				logger.Log(logLevel, ex, message: null);
			}
		}

		/// <summary>
		/// Catches all exceptions except <see cref="OperationCanceledException"/> thrown by the task and logs them using the provided logger.
		/// </summary>
		public async Task Catch(ILogger logger, CancellationToken cancellationToken, LogLevel logLevel = LogLevel.Warning)
		{
			try
			{
				await task;
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				throw;
			}
			catch (Exception ex)
			{
				logger.Log(logLevel, ex, ex.Message);
			}
		}
	}

	extension(ValueTask task)
	{
		public async ValueTask CatchAll(ILogger logger, LogLevel logLevel = LogLevel.Warning)
		{
			try
			{
				await task;
			}
			catch (Exception ex)
			{
				logger.Log(logLevel, ex, ex.Message);
			}
		}

		public async ValueTask Catch(ILogger logger, CancellationToken cancellationToken, LogLevel logLevel = LogLevel.Warning)
		{
			try
			{
				await task;
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				throw;
			}
			catch (Exception ex)
			{
				logger.Log(logLevel, ex, ex.Message);
			}
		}
	}

	extension<T>(Task<T> task)
	{
		public async Task<T?> CatchAll(ILogger logger, T? fallback = default, LogLevel logLevel = LogLevel.Warning)
		{
			try
			{
				return await task;
			}
			catch (Exception ex)
			{
				logger.Log(logLevel, ex, ex.Message);
				return fallback;
			}
		}

		/// <summary>
		/// Catches all exceptions except <see cref="OperationCanceledException"/> thrown by the task and logs them using the provided logger.
		/// </summary>
		public async Task<T?> Catch(ILogger logger, CancellationToken cancellationToken, T? fallback = default, LogLevel logLevel = LogLevel.Warning)
		{
			try
			{
				return await task;
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				throw;
			}
			catch (Exception ex)
			{
				logger.Log(logLevel, ex, ex.Message);
				return fallback;
			}
		}

		/// <summary>
		/// Catches all exceptions except <see cref="OperationCanceledException"/> thrown by the task and logs them using the provided logger.
		/// </summary>
		public async Task<T?> Catch(ILogger logger, TimeSpan timeout, CancellationToken cancellationToken, T? fallback = default, LogLevel logLevel = LogLevel.Warning)
		{
			var catchTask = task.Catch(logger, cancellationToken, fallback, logLevel);
			var timeoutTask = Task.Delay(timeout, cancellationToken);
			var completedTask = await Task.WhenAny(catchTask, timeoutTask);
			if (completedTask == timeoutTask)
			{
				return fallback;
			}

			return await catchTask;
		}
	}

	extension<T>(ValueTask<T> task)
	{
		/// <summary>
		/// Catches all exceptions except <see cref="OperationCanceledException"/> thrown by the task and logs them using the provided logger.
		/// </summary>
		public async ValueTask<T?> Catch(ILogger logger, CancellationToken cancellationToken, T? fallback = default, LogLevel logLevel = LogLevel.Warning)
		{
			try
			{
				return await task;
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				throw;
			}
			catch (Exception ex)
			{
				logger.Log(logLevel, ex, ex.Message);
				return fallback;
			}
		}
	}

	extension<T>(Task<IReadOnlyList<T>> task)
	{
		public async IAsyncEnumerable<T> AsAsyncEnumerable()
		{
			var result = await task;
			foreach (var item in result)
			{
				yield return item;
			}
		}
	}

	extension<TSource>(IAsyncEnumerable<TSource> source)
	{
		public async IAsyncEnumerable<TSource> Catch<TException>(
			Func<TException, bool> when,
			Func<IAsyncEnumerable<TSource>> onError,
			ILogger logger,
			[EnumeratorCancellation] CancellationToken cancellationToken = default
		)
			where TException : Exception
		{
			// REVIEW: This implementation mirrors the Ix implementation, which does not protect GetEnumerator
			//         using the try statement either. A more trivial implementation would use await foreach
			//         and protect the entire loop using a try statement, with two breaking changes:
			//
			//         - Also protecting the call to GetAsyncEnumerator by the try statement.
			//         - Invocation of the handler after disposal of the failed first sequence.

			var hasError = false;

			await using (var e = source.GetAsyncEnumerator(cancellationToken))
			{
				while (true)
				{
					TSource c;

					try
					{
						if (!await e.MoveNextAsync())
							break;

						c = e.Current;
					}
					catch (TException ex) when (when(ex))
					{
						logger.LogWarning(ex, ex.Message);
						hasError = true;
						break;
					}

					yield return c;
				}
			}

			if (hasError)
			{
				await foreach (var item in onError().WithCancellation(cancellationToken))
				{
					yield return item;
				}
			}
		}

		public IAsyncEnumerable<TSource> Catch(ILogger logger, CancellationToken cancellationToken = default) =>
			source.Catch<TSource, Exception>(_ => true, () => AsyncEnumerable.Empty<TSource>(), logger, cancellationToken);
	}
}
