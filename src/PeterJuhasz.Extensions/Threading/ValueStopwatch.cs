namespace System.Diagnostics;

public readonly struct ValueStopwatch
{
	private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

	private readonly long _startTimestamp;

	private bool IsActive => _startTimestamp != 0;

	private ValueStopwatch(long startTimestamp)
	{
		_startTimestamp = startTimestamp;
	}

	public static ValueStopwatch StartNew() => new(Stopwatch.GetTimestamp());

	public TimeSpan GetElapsedTime()
	{
		// Start timestamp can't be zero in an initialized ValueStopwatch. It would have to be literally the first thing executed when the machine boots to be 0.
		// So it being 0 is a clear indication of default(ValueStopwatch)
		if (!IsActive)
		{
			throw new InvalidOperationException("An uninitialized, or 'default', ValueStopwatch cannot be used to get elapsed time.");
		}

		long end = Stopwatch.GetTimestamp();
		long timestampDelta = end - _startTimestamp;
		long ticks = (long)(TimestampToTicks * timestampDelta);
		return TimeSpan.FromTicks(ticks);
	}
}