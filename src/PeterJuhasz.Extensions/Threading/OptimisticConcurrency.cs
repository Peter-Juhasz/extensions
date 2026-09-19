namespace System.Threading.Tasks;

public static class OptimisticConcurrency
{
	public record class Options(
		int? MaximumTryCount = null,
		Func<int, TimeSpan>? DelayBetweenRetries = null,
		TimeSpan? Timeout = default
	)
	{
		public static readonly Options Default = new();
	}

	public static async Task RetryAsync(Func<CancellationToken, ValueTask> factory, Options? options, CancellationToken cancellationToken)
	{
		options ??= Options.Default;

		var effective = cancellationToken;
		CancellationTokenSource? linked = null;
		if (options.Timeout != null)
		{
			linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			linked.CancelAfter(options.Timeout.Value);
			effective = linked.Token;
		}
		var retryCount = 0;

		try
		{
			while (true)
			{
				effective.ThrowIfCancellationRequested();

				if (retryCount++ > options.MaximumTryCount)
				{
					throw new OperationCanceledException("The operation could not be completed due to repeated conflicts.");
				}

				try
				{
					await factory(effective);
					return;
				}
				catch (ConflictException)
				{
					if (options.DelayBetweenRetries is { } delayFunc)
					{
						var delay = delayFunc(retryCount);
						if (delay > TimeSpan.Zero)
						{
							await Task.Delay(delay, effective);
						}
					}

					continue;
				}
			}
		}
		finally
		{
			linked?.Dispose();
		}
	}

	public static async Task RetryAsync(Func<CancellationToken, ValueTask> factory, CancellationToken cancellationToken) =>
		await RetryAsync(factory, Options.Default, cancellationToken);


	public static async Task<T> RetryAsync<T>(Func<CancellationToken, ValueTask<T>> factory, Options? options, CancellationToken cancellationToken)
	{
		options ??= Options.Default;

		var effective = cancellationToken;
		CancellationTokenSource? linked = null;
		if (options.Timeout != null)
		{
			linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			linked.CancelAfter(options.Timeout.Value);
			effective = linked.Token;
		}
		var retryCount = 0;

		try
		{
			while (true)
			{
				effective.ThrowIfCancellationRequested();

				if (retryCount++ > options.MaximumTryCount)
				{
					throw new OperationCanceledException("The operation could not be completed due to repeated conflicts.");
				}

				try
				{
					return await factory(effective);
				}
				catch (ConflictException)
				{
					if (options.DelayBetweenRetries is { } delayFunc)
					{
						var delay = delayFunc(retryCount);
						if (delay > TimeSpan.Zero)
						{
							await Task.Delay(delay, effective);
						}
					}

					continue;
				}
			}
		}
		finally
		{
			linked?.Dispose();
		}
	}

	public static async Task<T> RetryAsync<T>(Func<CancellationToken, ValueTask<T>> factory, CancellationToken cancellationToken) =>
		await RetryAsync(factory, Options.Default, cancellationToken);
}