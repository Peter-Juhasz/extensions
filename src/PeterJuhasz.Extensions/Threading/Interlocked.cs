using System.Diagnostics.CodeAnalysis;

namespace System.Threading.Tasks;

public static class InterlockedExtensions
{
	public static bool Initialize<T>([NotNull] ref T? target, T value) where T : class =>
		Interlocked.CompareExchange(ref target, value, null) == null;
}
