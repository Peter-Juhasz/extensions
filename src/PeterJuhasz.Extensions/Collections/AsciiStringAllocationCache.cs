using System.Buffers;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace System.Collections.Concurrent;

public sealed class AsciiStringAllocationCache
{
	[SuppressMessage("Reliability", "CA2014:Do not use stackalloc in loops", Justification = "<Pending>")]
	public AsciiStringAllocationCache(ReadOnlySpan<string> strings, bool ignoreCase = false)
	{
		_ignoreCase = ignoreCase;

		KeyValuePair<int, string>[] builder = new KeyValuePair<int, string>[strings.Length];
		for (int i = 0; i < strings.Length; i++)
		{
			var item = strings[i];

			// get bytes
			Span<byte> bytes = item.Length > 128 ? new byte[item.Length] : stackalloc byte[item.Length];
			if (Ascii.FromUtf16(item, bytes, out var written) != OperationStatus.Done)
			{
				throw new ArgumentException($"Couldn't convert string '{item}' to ASCII bytes.", nameof(strings));
			}

			// compute hash
			var hash = GetHashCode(bytes[..written]);

			builder[i] = new KeyValuePair<int, string>(hash, item);
		}

		_cache = builder.ToFrozenDictionary();
	}

	private readonly FrozenDictionary<int, string> _cache;
	private readonly bool _ignoreCase;

	public bool IgnoreCase => _ignoreCase;

	public bool TryGetValue(ReadOnlySpan<byte> bytes, [NotNullWhen(true)] out string? value)
	{
		var hash = GetHashCode(bytes);

		// try get from cache
		if (_cache.TryGetValue(hash, out value))
		{
			// double check equality to avoid collision
			if (_ignoreCase)
			{
				if (Ascii.EqualsIgnoreCase(bytes, value))
				{
					return true;
				}
			}
			else
			{
				if (Ascii.Equals(bytes, value))
				{
					return true;
				}
			}
		}

		return false;
	}

	private int GetHashCode(ReadOnlySpan<byte> bytes)
	{
		var hashCode = new HashCode();

		if (_ignoreCase)
		{
			// lower case
			Span<byte> lower = stackalloc byte[bytes.Length];
			if (Ascii.ToLower(bytes, lower, out var written) != OperationStatus.Done)
			{
				throw new ArgumentException("Couldn't convert bytes to lower case ASCII bytes.", nameof(bytes));
			}

			// compute hash
			hashCode.AddBytes(lower[..written]);
		}
		else
		{
			// compute hash
			hashCode.AddBytes(bytes);
		}

		return hashCode.ToHashCode();
	}
}
