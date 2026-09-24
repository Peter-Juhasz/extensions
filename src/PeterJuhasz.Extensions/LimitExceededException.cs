namespace System;

public class LimitExceededException : Exception
{
	public LimitExceededException(int limit, int actual)
		: base($"Limit reached. Maximum: {limit}, Actual: {actual}")
	{
		Limit = limit;
		Actual = actual;
	}

	public int Limit { get; }
	public int Actual { get; }
}
