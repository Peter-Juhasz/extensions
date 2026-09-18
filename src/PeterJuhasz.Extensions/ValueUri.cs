using Microsoft.AspNetCore.Internal;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace System;

// TODO: support credentials

public readonly ref struct ValueUri
{
	public ValueUri(ReadOnlySpan<char> uri)
	{
		if (uri.IsEmpty || uri.IsWhiteSpace())
		{
			throw new ArgumentException("URI cannot be empty or whitespace.", nameof(uri));
		}

		Uri = uri;
	}
	public ValueUri(string uri) : this(uri.AsSpan())
	{
		ArgumentNullException.ThrowIfNull(uri);
	}

	public static bool TryParse(string? str, out ValueUri uri)
	{
		if (str.IsNullOrWhiteSpace())
		{
			uri = default;
			return false;
		}

		return TryParse(str.AsSpan(), out uri);
	}

	public static bool TryParse(ReadOnlySpan<char> str, out ValueUri uri)
	{
		if (str.IsEmpty || str.IsWhiteSpace())
		{
			uri = default;
			return false;
		}

		// TODO: validate URI format

		uri = new(str);
		return true;
	}

	private static readonly SearchValues<char> Delimiters = SearchValues.Create("/:?#");
	private static readonly SearchValues<char> AndOrHash = SearchValues.Create("&#");
	private static readonly SearchValues<char> QuestionMarkOrHash = SearchValues.Create("?#");

	public ReadOnlySpan<char> Uri { get; }

	public ReadOnlySpan<char> SchemeSpan
	{
		get
		{
			var delimiter = Uri.IndexOf("://");
			if (delimiter > -1)
			{
				return Uri[..delimiter];
			}

			return default;
		}
	}

	public string? Scheme
	{
		get
		{
			var scheme = SchemeSpan;
			if (scheme.IsEmpty)
			{
				return null;
			}

			if (scheme.Equals(System.Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
			{
				return System.Uri.UriSchemeHttps;
			}
			else if (scheme.Equals(System.Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
			{
				return System.Uri.UriSchemeHttp;
			}

			return new string(scheme);
		}
	}

	public ReadOnlySpan<char> HostSpan
	{
		get
		{
			var span = Uri;
			if (span.IndexOf("://") is int index and > -1)
			{
				span = span[(index + 3)..];
			}
			else if (Uri.StartsWith("//"))
			{
				span = Uri[2..];
			}
			if (span.IsEmpty)
			{
				return default;
			}

			var delimiterIndex = span.IndexOfAny(Delimiters);
			if (delimiterIndex != -1)
			{
				return span[..delimiterIndex];
			}

			return span;
		}
	}

	public string? Host
	{
		get
		{
			var host = HostSpan;
			if (host.IsEmpty)
			{
				return null;
			}

			return new string(host);
		}
	}

	public bool IsHost(ReadOnlySpan<char> host) => host.Equals(HostSpan, StringComparison.OrdinalIgnoreCase);

	public bool IsDomainOrAnySubdomain(ReadOnlySpan<char> domain)
	{
		if (domain.IsEmpty)
		{
			return false;
		}

		var host = HostSpan;
		if (host.IsEmpty)
		{
			return false;
		}

		if (!host.EndsWith(domain, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		if (host.Length == domain.Length)
		{
			return true;
		}

		if (host[host.Length - domain.Length - 1] == '.')
		{
			return true;
		}

		return false;
	}

	public int? Port
	{
		get
		{
			var index = Uri.IndexOf(':');
			if (index == -1)
			{
				return null;
			}

			var span = Uri[(index + 1)..];
			if (span.IsEmpty)
			{
				return null;
			}

			if (span.StartsWith("//"))
			{
				span = span[2..];

				if (span.IsEmpty)
				{
					return null;
				}

				index = span.IndexOf(':');
				if (index == -1)
				{
					return null;
				}
				span = span[(index + 1)..];
			}

			var delimiterIndex = span.IndexOfAny(Delimiters);
			if (delimiterIndex != -1)
			{
				span = span[..delimiterIndex];
			}

			if (int.TryParse(span, out var port) && port > 0 && port < 65536)
			{
				return port;
			}

			return null;
		}
	}

	public bool IsHttps()
	{
		var scheme = SchemeSpan;
		if (!scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
		{
			return Port is 443;
		}

		return false;
	}

	public bool IsHttpOrHttps()
	{
		var scheme = SchemeSpan;
		if (scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ||
			scheme.Equals("http", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (scheme.IsEmpty)
		{
			return Port is 80 or 443;
		}

		return false;
	}

	public bool IsPathEmpty => PathSpan is [] or "/";

	public ReadOnlySpan<char> PathSpan
	{
		get
		{
			var span = Uri;
			if (span.IndexOf("://") is int index and > -1)
			{
				span = span[(index + 3)..];
			}
			else if (Uri.StartsWith("//"))
			{
				span = Uri[2..];
			}
			var delimiterIndex = span.IndexOf('/');
			if (delimiterIndex == -1)
			{
				return default;
			}

			span = span[delimiterIndex..];

			if (span.IndexOfAny(QuestionMarkOrHash) is int idx and > -1)
			{
				span = span[..idx];
			}

			return span;
		}
	}

	public int PathSegmentCount
	{
		get
		{
			var path = PathSpan;
			if (path.Length <= 1)
			{
				return 0;
			}
			var count = 0;
			foreach (var item in path.Split('/'))
			{
				var (_, length) = item.GetOffsetAndLength(path.Length);
				if (length == 0)
				{
					continue;
				}

				count++;
			}
			return count;
		}
	}

	public bool TryGetPathSegmentSpan(int index, out ReadOnlySpan<char> segment)
	{
		var path = PathSpan;
		if (path.IsEmpty)
		{
			segment = default;
			return false;
		}
		var i = -1;
		foreach (var item in path.Split('/'))
		{
			if (i == index)
			{
				segment = path[item];
				if (segment.IsEmpty)
				{
					segment = default;
					return false;
				}
				return true;
			}

			i++;
		}

		segment = default;
		return false;
	}

	public bool TryGetPathSegment(int index, [NotNullWhen(true)] out string? segment)
	{
		if (!TryGetPathSegmentSpan(index, out var segmentSpan))
		{
			segment = null;
			return false;
		}

		if (segmentSpan.IsEmpty)
		{
			segment = null;
			return false;
		}

		if (!segmentSpan.Contains('%'))
		{
			segment = new string(segmentSpan);
			return true;
		}

		Span<char> inline = stackalloc char[segmentSpan.Length];
		var written = UrlDecoder.DecodeRequestLine(segmentSpan, inline);
		segment = new string(inline[..written]);
		return true;
	}

	public bool TryGetPathSegment<T>(int index, [NotNullWhen(true)] out T? value) where T : ISpanParsable<T>
	{
		if (!TryGetPathSegmentSpan(index, out var segmentSpan))
		{
			value = default;
			return false;
		}

		if (segmentSpan.IsEmpty)
		{
			value = default;
			return false;
		}

		if (!segmentSpan.Contains('%'))
		{
			return T.TryParse(segmentSpan, null, out value);
		}

		Span<char> inline = stackalloc char[segmentSpan.Length];
		var written = UrlDecoder.DecodeRequestLine(segmentSpan, inline);
		return T.TryParse(inline[..written], null, out value);
	}

	public bool IsPath(ReadOnlySpan<char> match, StringComparison comparison = StringComparison.Ordinal)
	{
		match = match.Trim('/');
		var path = PathSpan.Trim('/');
		if (path.IsEmpty)
		{
			return match.IsEmpty;
		}
		if (!path.Contains('%'))
		{
			return path.Equals(match, comparison);
		}
		Span<char> inline = stackalloc char[path.Length];
		var written = UrlDecoder.DecodeRequestLine(path, inline);
		var decodedPath = inline[..written];
		return decodedPath.AsReadOnly().Equals(match, comparison);
	}

	public bool IsPathSegment(int index, ReadOnlySpan<char> match, StringComparison comparison = StringComparison.Ordinal)
	{
		if (TryGetPathSegmentSpan(index, out var segmentSpan))
		{
			if (segmentSpan.IsEmpty)
			{
				return match.IsEmpty;
			}

			if (!segmentSpan.Contains('%'))
			{
				return segmentSpan.Equals(match, comparison);
			}

			Span<char> inline = stackalloc char[segmentSpan.Length];
			var written = UrlDecoder.DecodeRequestLine(segmentSpan, inline);
			var segment = inline[..written];
			return segment.AsReadOnly().Equals(match, comparison);
		}

		return false;
	}

	public bool IsPathSegment(int index, Func<ReadOnlySpan<char>, bool> predicate)
	{
		if (!TryGetPathSegmentSpan(index, out var segmentSpan))
		{
			return false;
		}

		if (segmentSpan.IsEmpty)
		{
			return false;
		}

		if (!segmentSpan.Contains('%'))
		{
			return predicate(segmentSpan);
		}

		Span<char> inline = stackalloc char[segmentSpan.Length];
		var written = UrlDecoder.DecodeRequestLine(segmentSpan, inline);
		var writtenSpan = inline[..written];
		return predicate(writtenSpan);
	}

	public bool IsPathSegment(int index, SearchValues<char> predicate)
	{
		if (!TryGetPathSegmentSpan(index, out var segmentSpan))
		{
			return false;
		}

		if (segmentSpan.IsEmpty)
		{
			return false;
		}

		if (!segmentSpan.Contains('%'))
		{
			return segmentSpan.ContainsAnyExcept(predicate);
		}

		Span<char> inline = stackalloc char[segmentSpan.Length];
		var written = UrlDecoder.DecodeRequestLine(segmentSpan, inline);
		var writtenSpan = inline[..written];
		return segmentSpan.ContainsAnyExcept(predicate);
	}

	public bool TryGetPathSegment(int index, Func<ReadOnlySpan<char>, bool> predicate, Span<char> output, out int written)
	{
		if (!TryGetPathSegmentSpan(index, out var segmentSpan))
		{
			written = 0;
			return false;
		}

		if (segmentSpan.IsEmpty)
		{
			written = 0;
			return false;
		}

		if (!segmentSpan.Contains('%'))
		{
			if (!predicate(segmentSpan))
			{
				written = 0;
				return false;
			}

			segmentSpan.CopyTo(output);
			written = segmentSpan.Length;
			return true;
		}

		if (output.Length < segmentSpan.Length)
		{
			written = 0;
			return false;
		}

		written = UrlDecoder.DecodeRequestLine(segmentSpan, output);
		var writtenSpan = output[..written];
		if (!predicate(writtenSpan))
		{
			written = 0;
			return false;
		}
		return true;
	}

	public bool TryGetPathSegment(int index, Func<ReadOnlySpan<char>, bool> predicate, [NotNullWhen(true)] out string? segment)
	{
		if (!TryGetPathSegmentSpan(index, out var segmentSpan))
		{
			segment = null;
			return false;
		}

		if (segmentSpan.IsEmpty)
		{
			segment = null;
			return false;
		}

		if (!segmentSpan.Contains('%'))
		{
			if (!predicate(segmentSpan))
			{
				segment = null;
				return false;
			}

			segment = new string(segmentSpan);
			return true;
		}

		Span<char> inline = stackalloc char[segmentSpan.Length];
		var written = UrlDecoder.DecodeRequestLine(segmentSpan, inline);
		var writtenSpan = inline[..written];
		if (!predicate(writtenSpan))
		{
			segment = null;
			return false;
		}
		segment = new string(writtenSpan);
		return true;
	}


	public ReadOnlySpan<char> QuerySpan
	{
		get
		{
			var span = Uri;
			if (Uri.IndexOf('#') is int fragmentIndex and > -1)
			{
				span = Uri[..fragmentIndex];
			}

			var delimiter = span.IndexOf('?');
			if (delimiter > -1)
			{
				return span[delimiter..];
			}

			return default;
		}
	}

	public ReadOnlySpan<char> GetQueryValueRaw(string name)
	{
		var query = QuerySpan;
		if (query.IsEmpty)
		{
			return default;
		}

		var offset = 0;
		while (offset < query.Length)
		{
			var index = query[offset..].IndexOf(name, StringComparison.OrdinalIgnoreCase);
			if (index == -1)
			{
				return default;
			}

			// positions are relative to the whole query, so the preceding character is always the real one
			index += offset;
			offset = index + 1;

			if (index > 0 && query[index - 1] is not ('&' or '?'))
			{
				continue;
			}

			var found = query[index..];
			if (name.Length >= found.Length)
			{
				return default;
			}

			if (found[name.Length] != '=')
			{
				continue;
			}

			found = found[(name.Length + 1)..];
			var valueEndIndex = found.IndexOfAny(AndOrHash);
			if (valueEndIndex != -1)
			{
				return found[..valueEndIndex];
			}

			return found;
		}

		return default;
	}

	// TODO: optimize this with another algorithm
	public bool ContainsQuery(string name) => !GetQueryValueRaw(name).IsEmpty;

	public bool ContainsAnyQuery(params ReadOnlySpan<string> names)
	{
		// TODO: optimize this with another algorithm

		foreach (var name in names)
		{
			if (ContainsQuery(name))
			{
				return true;
			}
		}

		return false;
	}

	public bool TryGetQueryValue(string name, [NotNullWhen(true)] out string? value)
	{
		var raw = GetQueryValueRaw(name);
		if (raw.IsEmpty)
		{
			value = default;
			return false;
		}

		if (!raw.Contains('%'))
		{
			value = new string(raw);
			return true;
		}

		Span<char> inline = stackalloc char[raw.Length];
		var written = UrlDecoder.DecodeRequestLine(raw, inline);
		value = new string(inline[..written]);
		return true;
	}

	public bool TryGetQueryValue(string name, Func<ReadOnlySpan<char>, bool> predicate, [NotNullWhen(true)] out string? value)
	{
		var raw = GetQueryValueRaw(name);
		if (raw.IsEmpty)
		{
			value = default;
			return false;
		}

		if (!raw.Contains('%'))
		{
			if (!predicate(raw))
			{
				value = default;
				return false;
			}

			value = new string(raw);
			return true;
		}

		Span<char> inline = stackalloc char[raw.Length];
		var written = UrlDecoder.DecodeRequestLine(raw, inline);
		var writtenSpan = inline[..written];
		if (!predicate(writtenSpan))
		{
			value = default;
			return false;
		}
		value = new string(writtenSpan);
		return true;
	}

	public bool TryGetQueryValue<T>(string name, [NotNullWhen(true)] out T? value, IFormatProvider? formatProvider = null) where T : ISpanParsable<T>
	{
		var raw = GetQueryValueRaw(name);
		if (raw.IsEmpty)
		{
			value = default;
			return false;
		}

		Span<char> inline = stackalloc char[raw.Length];
		var written = UrlDecoder.DecodeRequestLine(raw, inline);
		var decoded = new string(inline[..written]);
		return T.TryParse(decoded, formatProvider, out value);
	}

	public string? GetQueryValue(string name) => TryGetQueryValue(name, out var str) ? str : null;

	public T? GetQueryValue<T>(string name, IFormatProvider? formatProvider = null) where T : ISpanParsable<T> => TryGetQueryValue<T>(name, out T? str, formatProvider) ? str : default;


	public ReadOnlySpan<char> FragmentSpan
	{
		get
		{
			var delimiter = Uri.IndexOf('#');
			if (delimiter > -1)
			{
				return Uri[delimiter..];
			}

			return default;
		}
	}

	public ReadOnlySpan<char> GetFragmentValueSpan(string name)
	{
		var fragment = FragmentSpan;
		if (fragment.IsEmpty)
		{
			return default;
		}

		var offset = 0;
		while (offset < fragment.Length)
		{
			var index = fragment[offset..].IndexOf(name);
			if (index == -1)
			{
				return default;
			}

			// positions are relative to the whole fragment, so the preceding character is always the real one
			index += offset;
			offset = index + 1;

			if (index > 0 && fragment[index - 1] is not ('&' or '#'))
			{
				continue;
			}

			var found = fragment[index..];
			if (name.Length >= found.Length)
			{
				return default;
			}

			if (found[name.Length] != '=')
			{
				continue;
			}

			found = found[(name.Length + 1)..];
			var valueEndIndex = found.IndexOf(';');
			if (valueEndIndex != -1)
			{
				return found[..valueEndIndex];
			}

			return found;
		}

		return default;
	}

	public bool TryGetFragmentQueryValue(string name, [NotNullWhen(true)] out string? value)
	{
		var raw = GetFragmentValueSpan(name);
		if (raw.IsEmpty)
		{
			value = default;
			return false;
		}

		if (!raw.Contains('%'))
		{
			value = new string(raw);
			return true;
		}

		Span<char> inline = stackalloc char[raw.Length];
		var written = UrlDecoder.DecodeRequestLine(raw, inline);
		value = new string(inline[..written]);
		return true;
	}


	public override string ToString() => new string(Uri);

	public Uri ToUri() => new(ToString());

	public InlineUriBuilder ToBuilder() => InlineUriBuilder.CreateFromUri(Uri);
}

public static partial class Extensions
{
	extension(string uri)
	{
		public ValueUri ToValueUri() => new(uri);
	}
}