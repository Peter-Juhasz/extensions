using Microsoft.Extensions.Primitives;
using System.Buffers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace System;

public static partial class UriExtensions
{
	extension(Uri uri)
	{
		public Uri AppendFragment(string fragment)
		{
			string originalString = uri.OriginalString;
			return new Uri(originalString + "#" + UrlEncoder.Default.Encode(fragment), UriKind.RelativeOrAbsolute);
		}

		public Uri AppendQueryString(string name, string? value)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (string.IsNullOrEmpty(value))
			{
				return uri;
			}

			string originalString = uri.OriginalString;
			return new Uri(string.Concat(originalString + (originalString.Contains("?") ? "&" : "?"), UrlEncoder.Default.Encode(name), "=", UrlEncoder.Default.Encode(value)), UriKind.RelativeOrAbsolute);
		}

		public Uri AppendQueryString(string name, int value)
		{
			return uri.AppendQueryString(name, value.ToString(CultureInfo.InvariantCulture));
		}

		public Uri AppendQueryString(string name, int? value)
		{
			return uri.AppendQueryString(name, value?.ToString(CultureInfo.InvariantCulture));
		}

		public Uri AppendQueryString(string name, double value)
		{
			return uri.AppendQueryString(name, value.ToString(CultureInfo.InvariantCulture));
		}

		public Uri AppendQueryString(string name, long value)
		{
			return uri.AppendQueryString(name, value.ToString(CultureInfo.InvariantCulture));
		}

		public Uri AppendQueryString(string name, long? value)
		{
			return uri.AppendQueryString(name, value?.ToString(CultureInfo.InvariantCulture));
		}

		public Uri AppendQueryString(string name, bool value)
		{
			return uri.AppendQueryString(name, value.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
		}

		public Uri AppendQueryString(string name, bool? value)
		{
			return uri.AppendQueryString(name, value?.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
		}

		public Uri AppendQueryString(string name, Guid value)
		{
			return uri.AppendQueryString(name, value.ToString());
		}

		public Uri AppendQueryString(string name, DateTimeOffset value)
		{
			return uri.AppendQueryString(name, value.ToString("O"));
		}

		public Uri AppendQueryString(string name, DateTimeOffset? value)
		{
			return uri.AppendQueryString(name, value?.ToString("O"));
		}

		public Uri AppendQueryString(string name, DateOnly value)
		{
			return uri.AppendQueryString(name, value.ToString("O"));
		}

		public Uri AppendQueryString(string name, DateOnly? value)
		{
			return uri.AppendQueryString(name, value?.ToString("O"));
		}

		public Uri AppendQueryString(string name, IEnumerable<string> value)
		{
			foreach (string item in value)
			{
				uri = uri.AppendQueryString(name, item);
			}

			return uri;
		}

		[RequiresUnreferencedCode("Uses reflection to create generic types at runtime.")]
		public Uri AppendAsQueryString<T>(T obj) where T : notnull
		{
			return uri.AppendAsQueryString(obj, typeof(T));
		}

		[RequiresUnreferencedCode("Uses reflection to create generic types at runtime.")]
		public Uri AppendAsQueryString(object obj, Type type)
		{
			var result = uri;
			foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				var value = property.GetValue(obj);
				if (Object.ReferenceEquals(value, null))
				{
					continue;
				}

				if (value is IEnumerable enumerable && property.PropertyType.IsGenericType)
				{
					var genericType = property.PropertyType.GenericTypeArguments[0];
					foreach (var item in enumerable)
					{
						ProcessValue(item, genericType);
					}
				}
				else
				{
					ProcessValue(value, property.PropertyType);
				}

				void ProcessValue(object? value, Type type)
				{
					var underlyingType = Nullable.GetUnderlyingType(type);
					var isNullable = underlyingType != null;
					if (isNullable)
					{
						value = type.GetProperty(nameof(Nullable<int>.Value))!.GetValue(value);
					}

					string? stringValue = value switch
					{
						Guid guid => guid == Guid.Empty ? null : guid.ToString(),
						int i => i.ToString(),
						bool b => isNullable switch
						{
							true => b.ToString(),
							false => b switch
							{
								true => b.ToString(),
								false => null,
							}
						},
						DateTime d => d.ToString("O"),
						DateTimeOffset d => d.ToString("O"),
						TimeSpan t => t.ToString(),
						string str => str,
						decimal str => str.ToString(),
						double str => str.ToString(),
						float str => str.ToString(),
						null => null,
						_ => (underlyingType ?? type).IsEnum ? value.ToString() : null,
					};

					if (stringValue != null)
					{
						var camelCase = Char.ToLower(property!.Name[0]) + property.Name[1..];
						result = result!.AppendQueryString(camelCase, stringValue);
					}
				}
			}

			return result;
		}

		public Uri Append(string relativeUri)
		{
			if (!uri.IsAbsoluteUri)
			{
				return new Uri(uri.ToString() + relativeUri, UriKind.Relative);
			}

			return new Uri(uri, relativeUri);
		}

		public Uri Append(Uri relativeUri)
		{
			return new Uri(uri, relativeUri);
		}

		public Uri RelativeTo(string host)
		{
			if (!uri.IsAbsoluteUri)
			{
				return uri;
			}

			if (uri.Host.Equals(host, StringComparison.InvariantCultureIgnoreCase))
			{
				return new Uri(uri.PathAndQuery, UriKind.Relative);
			}

			return uri;
		}

		public string GetOrigin()
		{
			return uri.GetComponents(UriComponents.HostAndPort | UriComponents.NormalizedHost, UriFormat.Unescaped);
		}

		public bool TryRemoveQueryParameters(out Uri removed, params ReadOnlySpan<string> names)
		{
			removed = uri.RemoveQueryParameters(names);
			return removed != uri;
		}

		public Uri RemoveQueryParameters(params ReadOnlySpan<string> names)
		{
			if (names.Length == 0)
			{
				return uri;
			}

			var query = uri.Query;
			if (string.IsNullOrEmpty(query))
			{
				return uri;
			}

			foreach (var name in names)
			{
				foreach (var index in query.IndexesOf(name, StringComparison.OrdinalIgnoreCase))
				{
					if (index == 0 || query[index - 1] == '?' || query[index - 1] == '&')
					{
						int end = query.IndexOf('&', index);
						if (end == -1)
						{
							end = query.Length;
						}
						query = query.Remove(index, end - index);
						break;
					}
				}
			}

			if (query == uri.Query)
			{
				return uri;
			}

			return new UriBuilder(uri)
			{
				Query = query?.Length > 1 ? query : null,
				Port = uri.IsDefaultPort ? -1 : uri.Port,
			}.Uri;
		}

		public bool TryGetQueryParameter(string name, out string? value)
		{
			var query = uri.Query;
			if (string.IsNullOrEmpty(query))
			{
				value = null;
				return false;
			}

			foreach (var index in query.IndexesOf(name, StringComparison.OrdinalIgnoreCase))
			{
				if (index == 0 || query[index - 1] is '?' or '&')
				{
					int end = query.IndexOf('&', index);
					if (end == -1)
					{
						end = query.Length;
					}

					int valueStart = index + name.Length + 1;
					if (valueStart < end)
					{
						value = Uri.UnescapeDataString(query[valueStart..end]);
						return true;
					}
					break;
				}
			}

			value = null;
			return false;
		}

		[SuppressMessage("Reliability", "CA2014:Do not use stackalloc in loops", Justification = "<Pending>")]
		public string GetSafeUrlString()
		{
			using var pooled = StringBuilderPool.GetPooledObject(out var builder);

			// scheme
			var scheme = uri.Scheme.ToLowerInvariant();
			if (scheme != Uri.UriSchemeHttp && scheme != Uri.UriSchemeHttps)
			{
				throw new ArgumentException("Invalid URL scheme.", nameof(uri));
			}
			builder.Append(uri.Scheme);
			builder.Append("://");

			// host
			if (uri.Host.AsSpan().ContainsAnyExcept(AllowedHostCharacters))
			{
				throw new ArgumentException("Invalid character in URL host.", nameof(uri));
			}
			builder.Append(uri.Host);

			// port
			if (uri.Port is not (80 or 443
#if DEBUG
				or 44310
#endif
			))
			{
				throw new ArgumentException("Invalid URL port.", nameof(uri));
			}
#if DEBUG
			if (uri.Port is not (80 or 443))
			{
				builder.Append(':');
				builder.Append(uri.Port);
			}
#endif

			// path
			if (uri.LocalPath.AsSpan().IndexOfAnyExcept(AllowedPathCharacters) is int firstIndex and not -1)
			{
				builder.Append(uri.LocalPath.AsSpan(0, firstIndex));

				var encoding = Encoding.UTF8;
				// TODO: accelerate search with another IndexOf call
				for (int i = firstIndex; i < uri.LocalPath.Length; i++)
				{
					var ch = uri.LocalPath[i];
					if (AllowedPathCharacters.Contains(ch))
					{
						builder.Append(ch);
					}
					else
					{
						Span<byte> bytes = stackalloc byte[4];
						encoding.TryGetBytes([ch], bytes, out var written);
						Span<char> hex = stackalloc char[2];
						for (int j = 0; j < written; j++)
						{
							builder.Append('%');
							bytes[j].TryFormat(hex, out _, "X2");
							builder.Append(hex);
						}
					}
				}
			}
			else
			{
				builder.Append(uri.LocalPath);
			}

			// query
			if (uri.Query is { Length: > 0 } query)
			{
				if (query.AsSpan().IndexOfAnyExcept(AllowedQueryCharacters) is int firstIndexQuery and not -1)
				{
					builder.Append(query.AsSpan(0, firstIndexQuery));

					var encoding = Encoding.UTF8;
					// TODO: accelerate search with another IndexOf call
					for (int i = firstIndexQuery; i < query.Length; i++)
					{
						var ch = query[i];
						if (AllowedQueryCharacters.Contains(ch))
						{
							builder.Append(ch);
						}
						else
						{
							Span<byte> bytes = stackalloc byte[4];
							encoding.TryGetBytes([ch], bytes, out var written);
							Span<char> hex = stackalloc char[2];
							for (int j = 0; j < written; j++)
							{
								builder.Append('%');
								bytes[j].TryFormat(hex, out _, "X2");
								builder.Append(hex);
							}
						}
					}
				}
				else
				{
					builder.Append(uri.Query);
				}
			}

			return builder.ToString();
		}
	}

	extension(Uri originalUri)
	{
		public Uri WithFileName(string newFileName)
		{
			// Extract the base path without the last segment (filename)
			string basePath = originalUri.GetLeftPart(UriPartial.Path);
			basePath = basePath.Remove(basePath.Length - originalUri.Segments[^1].Length);

			// Combine the new base path with the new filename
			string newUriString = basePath + newFileName;

			// Append the original query string if it exists
			if (!string.IsNullOrEmpty(originalUri.Query))
			{
				newUriString += originalUri.Query;
			}

			// Create and return the new Uri
			return new Uri(newUriString);
		}
	}

	extension(string str)
	{
		public Uri ToUri()
		{
			return new Uri(str, UriKind.Absolute);
		}

		public Uri ToUri(UriKind kind)
		{
			return new Uri(str, kind);
		}

		public Regex ToRegex(RegexOptions options) => new(str, options);

		public Regex ToRegex() => new(str);

		public string EncodeHtml() => HtmlEncoder.Default.Encode(str);

		public string EncodeXml() => str.EncodeHtml();

		[SuppressMessage("Reliability", "CA2014:Do not use stackalloc in loops", Justification = "<Pending>")]
		public string EncodeUri()
		{
			var firstToEncode = str.AsSpan().IndexOfAnyExcept(AllowedUriDataCharacters);
			if (firstToEncode == -1)
			{
				return str;
			}

			using var _1 = StringBuilderPool.GetPooledObject(out var builder);
			builder.Append(str, 0, firstToEncode);
			for (int i = firstToEncode; i < str.Length; i++)
			{
				// TODO: optimize search with another IndexOf call
				var ch = str[i];
				if (AllowedUriDataCharacters.Contains(ch))
				{
					builder.Append(ch);
				}
				else
				{
					Span<byte> bytes = stackalloc byte[4];
					Encoding.UTF8.TryGetBytes([ch], bytes, out var written);
					Span<char> hex = stackalloc char[2];

					for (int j = 0; j < written; j++)
					{
						builder.Append('%');
						bytes[j].TryFormat(hex, out _, "X2");
						builder.Append(hex);
					}
				}
			}
			return builder.ToString();
		}
	}

	private static readonly SearchValues<char> AllowedUriDataCharacters = SearchValues.Create("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789+-._");
	internal static readonly SearchValues<char> AllowedHostCharacters = SearchValues.Create("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-.");
	internal static readonly SearchValues<char> AllowedPathCharacters = SearchValues.Create("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-+%/_.@");
	internal static readonly SearchValues<char> AllowedPathSegmentCharacters = SearchValues.Create("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-+_.%@");
	internal static readonly SearchValues<char> AllowedQueryCharacters = SearchValues.Create("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-+%_.?=&[]:,@");
	internal static readonly SearchValues<char> AllowedQueryNameCharacters = SearchValues.Create("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_");
}

public static partial class UrlValidator
{

	// local part starts and ends with a letter or digit, and can have letters digits, or "-.!#$&+_" in between, but not two or more of those in a row
	// domain part must start letter or digit, and end with at least two letters or digits as TLD, in between can have letters, digits, or "-" or ".", but not two or more "." in a row
	[GeneratedRegex("^[a-zA-Z0-9](?:[-!#$&+_.]?[a-zA-Z0-9])*@[a-zA-Z0-9](?:[-]*[.]?[a-zA-Z0-9])*[.][a-zA-Z0-9]{2,}$")]
	private static partial Regex ValidEmailRegex();

	// local part starts and ends with a letter or digit, and can have letters digits, or "-.!#$&+_" in between, but not two or more of those in a row
	// domain part must start letter or digit, and end with at least two letters or digits as TLD, in between can have letters, digits, or "-" or ".", but not two or more "." in a row
	[GeneratedRegex(@"^\+\d+[0-9 \-/]{6,}$")]
	private static partial Regex ValidPhoneNumberRegex();

	public static bool IsValidEmail(StringSegment email)
	{
		if (!email.HasValue) return false;
		int atIndex = email.IndexOf('@');
		if (atIndex < 0 || atIndex > 64) return false;
		return ValidEmailRegex().IsMatch(email);
	}

	public static bool IsValidPhoneNumber(StringSegment phone)
	{
		if (!phone.HasValue) return false;
		if (phone.Length > 24) return false;
		return ValidPhoneNumberRegex().IsMatch(phone);
	}


	public static bool IsValidHyperlink(StringSegment url)
	{
		if (url.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < url.Length; i++)
		{
			if (url[i] == '@' || char.IsControl(url[i]))
			{
				return false;
			}
		}

		if (!Uri.TryCreate(url.Value, UriKind.RelativeOrAbsolute, out var uri))
		{
			return false;
		}

		if (uri.IsAbsoluteUri)
		{
			return (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
				&& string.IsNullOrEmpty(uri.UserInfo)
				&& uri.HostNameType == UriHostNameType.Dns
				&& uri.Host.Contains('.')
				&& !uri.IsLoopback;
		}
		else
		{
			return uri.OriginalString.StartsWith("/") && !uri.OriginalString.StartsWith("//");
		}
	}

}