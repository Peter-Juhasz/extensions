namespace System.Threading;

public sealed class CancellationSeries(CancellationToken cancellationToken) : IDisposable
{
	private CancellationTokenSource? _cts = null;
	private volatile bool _disposed = false;

	public CancellationToken GetNext(TimeSpan? timeout = null)
	{
		if (_disposed)
		{
			return new(canceled: true);
		}

		if (cancellationToken.IsCancellationRequested)
		{
			if (Interlocked.Exchange(ref _cts, null) is CancellationTokenSource prior)
			{
				Cancel(prior);
			}

			return cancellationToken;
		}

		var newCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		if (timeout != null)
		{
			newCts.CancelAfter(timeout.Value);
		}

		var previous = Interlocked.Exchange(ref _cts, newCts);
		if (previous != null)
		{
			Cancel(previous);
		}

		return newCts.Token;
	}

	private static void Cancel(CancellationTokenSource cts)
	{
		using (cts)
		{
			if (!cts.IsCancellationRequested)
			{
				cts.Cancel();
			}
		}
	}

	public void Dispose()
	{
		_disposed = true;

		if (Interlocked.Exchange(ref _cts, null) is CancellationTokenSource prior)
		{
			Cancel(prior);
		}
	}
}
