namespace System.Threading.Channels;

public class MultiContributorWrapper<T>(
	T value,
	int requiredContributors
)
{
	private T _value = value;
	public T Value => _value;

	public int RequiredContributors => requiredContributors;

	private int _contributorCount = 0;

	/// <returns>Returns true if required contributors reached.</returns>
	public bool Apply(Func<T, T> update)
	{
		var isLast = Add();

		// can't use Interlocked.CompareExchange, because:
		// - T may be a value type
		// - even if T is a reference type, the update function may return the same instance
		lock (this)
		{
			_value = update(_value);
		}

		return isLast;
	}

	/// <returns>Returns true if required contributors reached.</returns>
	public bool Apply(Action<T> update)
	{
		var isLast = Add();

		lock (this)
		{
			update(_value);
		}

		return isLast;
	}

	/// <returns>Returns true if required contributors reached.</returns>
	public bool Add()
	{
		var newContributorCount = Interlocked.Increment(ref _contributorCount);
		if (newContributorCount > requiredContributors)
		{
			throw new InvalidOperationException("Maximum number of contributors reached.");
		}

		return newContributorCount == requiredContributors;
	}
}
