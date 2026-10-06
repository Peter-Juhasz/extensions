namespace System.Threading.Channels;

public class MultiContributorWrapper<T>(
	T value,
	int requiredContributors
)
{
	private readonly Lock _lock = new();

	private T _value = value;
	public T Value => _value;

	public int RequiredContributors => requiredContributors;

	private int _contributorCount = 0;

	/// <returns>Returns true if required contributors reached.</returns>
	public bool Apply(Func<T, T> update)
	{
		// can't use Interlocked.CompareExchange, because:
		// - T may be a value type
		// - even if T is a reference type, the update function may return the same instance
		lock (_lock)
		{
			EnsureNotCompleted();
			_value = update(_value);

			// count only after the update is applied, so the last contributor never observes a value with pending updates
			return Count();
		}
	}

	/// <returns>Returns true if required contributors reached.</returns>
	public bool Apply(Action<T> update)
	{
		lock (_lock)
		{
			EnsureNotCompleted();
			update(_value);
			return Count();
		}
	}

	/// <returns>Returns true if required contributors reached.</returns>
	public bool Add()
	{
		lock (_lock)
		{
			EnsureNotCompleted();
			return Count();
		}
	}

	private void EnsureNotCompleted()
	{
		if (_contributorCount >= requiredContributors)
		{
			throw new InvalidOperationException("Maximum number of contributors reached.");
		}
	}

	private bool Count() => ++_contributorCount == requiredContributors;
}
