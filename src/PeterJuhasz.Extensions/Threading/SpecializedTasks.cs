using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace System.Threading.Tasks;

public static class SpecializedTasks
{
	public static readonly Task<bool> True = Task.FromResult(true);
	public static readonly Task<bool> False = Task.FromResult(false);

	[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "This is a Task wrapper, not an asynchronous method.")]
	public static Task<T?> Default<T>()
		=> EmptyTasks<T>.Default;

	[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "This is a Task wrapper, not an asynchronous method.")]
	public static Task<T?> Null<T>() where T : class
		=> Default<T>();

	[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "This is a Task wrapper, not an asynchronous method.")]
	public static Task<IReadOnlyList<T>> EmptyReadOnlyList<T>()
		=> EmptyTasks<T>.EmptyReadOnlyList;

	[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "This is a Task wrapper, not an asynchronous method.")]
	public static Task<ImmutableArray<T>> EmptyImmutableArray<T>()
		=> EmptyTasks<T>.EmptyImmutableArray;

	[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "This is a Task wrapper, not an asynchronous method.")]
	public static Task<IEnumerable<T>> EmptyEnumerable<T>()
		=> EmptyTasks<T>.EmptyEnumerable;

	private static class EmptyTasks<T>
	{
		public static readonly Task<T?> Default = Task.FromResult<T?>(default);
		public static readonly Task<IEnumerable<T>> EmptyEnumerable = Task.FromResult<IEnumerable<T>>([]);
		public static readonly Task<ImmutableArray<T>> EmptyImmutableArray = Task.FromResult(ImmutableArray<T>.Empty);
		public static readonly Task<IReadOnlyList<T>> EmptyReadOnlyList = Task.FromResult<IReadOnlyList<T>>([]);
	}
}