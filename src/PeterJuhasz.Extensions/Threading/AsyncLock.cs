namespace System.Threading;

public sealed class AsyncLock
{
	public static Task<IDisposable> LockAsync(SemaphoreSlim semaphore)
	{
		Task<IDisposable> releaser = Task.FromResult((IDisposable)new Releaser(semaphore));
		Task wait = semaphore.WaitAsync();
		return wait.IsCompleted
			? releaser
			: wait.ContinueWith(
				(_, state) => (IDisposable)state!,
				releaser.Result,
				CancellationToken.None,
				TaskContinuationOptions.ExecuteSynchronously,
				TaskScheduler.Default
			);
	}

	private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
	{
		public void Dispose() => semaphore.Release();
	}
}
