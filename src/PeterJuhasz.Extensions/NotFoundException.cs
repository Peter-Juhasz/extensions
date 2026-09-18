

public class NotFoundException : Exception
{
	public NotFoundException(string message)
		: base(message)
	{ }

	public static NotFoundException Create<TEntity, TKey>(TKey key)
		=> new($"Entity of type '{typeof(TEntity)}' with key '{key}' was not found.");
}
