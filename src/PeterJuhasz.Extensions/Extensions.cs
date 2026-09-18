using System.Buffers.Text;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace System;

public static partial class Extensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T Coalesce<T>(this T value, T fallback) where T : struct, IEquatable<T> => value.Equals(default) ? fallback : value;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEnumerable<T> Safe<T>(this IEnumerable<T>? source) => source ?? []; // Array not Enumerable, because it implements IReadOnlyList<T> which further optimizations can rely on

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IReadOnlyCollection<T> Safe<T>(this IReadOnlyCollection<T>? source) => source ?? [];

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IReadOnlyList<T> Safe<T>(this IReadOnlyList<T>? source) => source ?? [];

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IReadOnlySet<T> Safe<T>(this IReadOnlySet<T>? source) => source ?? FrozenSet<T>.Empty;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IImmutableList<T> Safe<T>(this IImmutableList<T>? source) => source ?? [];

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IImmutableSet<T> Safe<T>(this IImmutableSet<T>? source) => source ?? [];

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IImmutableDictionary<TKey, TValue> Safe<TKey, TValue>(this IImmutableDictionary<TKey, TValue>? source) where TKey : notnull => source ?? ImmutableDictionary<TKey, TValue>.Empty;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ISet<T> Safe<T>(this ISet<T>? source) => source ?? ImmutableHashSet<T>.Empty;


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Max(this int a, int b) => Math.Max(a, b);


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T ParseAs<T>(this string value) where T : IParsable<T> => T.Parse(value, CultureInfo.InvariantCulture);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T ParseAs<T>(this string value, IFormatProvider provider) where T : IParsable<T> => T.Parse(value, provider);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryParseAs<T>(this string? value, out T? parsed) where T : IParsable<T> => T.TryParse(value, CultureInfo.InvariantCulture, out parsed);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryParseAs<T>(this ReadOnlySpan<char> value, out T? parsed) where T : ISpanParsable<T> => T.TryParse(value, CultureInfo.InvariantCulture, out parsed);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[RequiresUnreferencedCode("Uses reflection to create generic types at runtime.")]
	public static T? ParseAsJson<T>(this string value, JsonSerializerOptions? options = null) => JsonSerializer.Deserialize<T>(value, options);



	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ToUtf8String(this byte[] bytes) => Encoding.UTF8.GetString(bytes);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ToAsciiString(this byte[] bytes) => Encoding.ASCII.GetString(bytes);


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static byte[] ToUtf8Bytes(this string value) => Encoding.UTF8.GetBytes(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static byte[] ToAsciiBytes(this string value) => Encoding.ASCII.GetBytes(value);


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static byte[] FromBase64(this string value) => Convert.FromBase64String(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ToBase64(this byte[] value) => Convert.ToBase64String(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ToBase64(this ReadOnlySpan<byte> value) => Convert.ToBase64String(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ToBase64(this BinaryData value) => Convert.ToBase64String(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ToBase64Url(this byte[] value) => Base64Url.EncodeToString(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ToBase64Url(this ReadOnlySpan<byte> value) => Base64Url.EncodeToString(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ToBase64Url(this BinaryData value) => Base64Url.EncodeToString(value);


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static byte[] FromHex(this string value) => Convert.FromHexString(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ToHex(this byte[] value) => Convert.ToHexString(value);


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[RequiresUnreferencedCode("Uses reflection to create generic types at runtime.")]
	public static string ToJsonString(this object? value, JsonSerializerOptions? options = null) => JsonSerializer.Serialize(value, options);

	public static void RemoveInPlace<T>(this Span<T> span, Range range, out int newLength)
	{
		var (offset, length) = range.GetOffsetAndLength(span.Length);
		RemoveInPlace(span, offset, length, out newLength);
	}

	public static void RemoveInPlace<T>(this Span<T> span, int offset, int length, out int newLength)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(offset);
		ArgumentOutOfRangeException.ThrowIfNegative(length);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(offset + length, span.Length);

		if (length == 0)
		{
			newLength = span.Length;
			return;
		}

		var remaining = span[(offset + length)..];
		remaining.CopyTo(span[offset..]);
		newLength = span.Length - length;
	}

	public static void ReplaceInPlace<T>(this ref Span<T> span, ReadOnlySpan<T> oldValue, ReadOnlySpan<T> newValue, out int newLength) where T : IEquatable<T>
	{
		if (oldValue.IsEmpty)
		{
			throw new ArgumentException("Old value cannot be empty.", nameof(oldValue));
		}

		if (oldValue.Length < newValue.Length)
		{
			throw new ArgumentException("New value cannot be longer than old value.", nameof(newValue));
		}

		var deltaLength = oldValue.Length - newValue.Length;

		// TODO: optimize search span after each match
		while (span.IndexOf(oldValue) is int index and not -1)
		{
			var remaining = span[(index + oldValue.Length)..];
			var targetSpan = span.Slice(index, newValue.Length);
			newValue.CopyTo(targetSpan);
			remaining.CopyTo(span[(index + newValue.Length)..]);
			span = span[..(span.Length - deltaLength)];
		}

		newLength = span.Length;
	}
}
