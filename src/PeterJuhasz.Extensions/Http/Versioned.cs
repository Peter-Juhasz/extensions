namespace System.Net.Http;

public readonly record struct Versioned<T>(T Value, string ETag)
{
	public static implicit operator T(Versioned<T> versioned) => versioned.Value;
}
