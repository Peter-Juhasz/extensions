namespace App.Core.Threading;

public static class StaticInstanceCache
{
	public static T GetOrCreate<T>() where T : class, new() => TypeCache<T>.Instance;

	private static class TypeCache<T> where T : class, new()
	{
		public static readonly T Instance = new();
	}
}