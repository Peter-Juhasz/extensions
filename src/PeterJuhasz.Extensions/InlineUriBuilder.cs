using Microsoft.Extensions.ObjectPool;
using System.Buffers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;

namespace System;

public sealed class InlineUriBuilder : IDisposable
{
	private static readonly ArrayPool<char> BufferPool = ArrayPool<char>.Shared;

	public InlineUriBuilder(string? baseUrl = null, UrlEncoder? encoder = null, int maximumLength = 128)
	{
		_encoder = encoder ?? UrlEncoder.Default;
		_buffer = BufferPool.Rent(maximumLength);
		if (baseUrl != null)
		{
			SetBaseUrl(baseUrl);
		}
	}

	private readonly UrlEncoder _encoder;
	private char[] _buffer;
	private State _lastWritten = State.Empty;
	private int _totalWritten = 0;

	public ReadOnlySpan<char> Written => _buffer.AsSpan(.._totalWritten);

	private Span<char> Remaining
	{
		get
		{
			if (_lastWritten is State.Empty && _totalWritten == 0 && _buffer.Length == 0)
			{
				_buffer = BufferPool.Rent(128);
			}

			return _buffer.AsSpan(_totalWritten);
		}
	}


	public static InlineUriBuilder CreateFromHost(string host, string scheme = "https", int port = 443)
	{
		var builder = Pool.Default.Get()
			.AppendScheme(scheme)
			.AppendHost(host)
		;

		if (!(scheme is "https" && port is 443) &&
			!(scheme is "http" && port is 80))
		{
			builder = builder.AppendPort(port);
		}

		return builder;
	}

	public static InlineUriBuilder CreateFromPath([StringSyntax(StringSyntaxAttribute.Uri)] string path) => Pool.Default.Get()
		.AppendPath(path)
	;

	/// <remarks>Works with safe URLs only.</remarks>
	public static InlineUriBuilder CreateFromUri([StringSyntax(StringSyntaxAttribute.Uri)] string uri) => CreateFromUri(uri.AsSpan());

	/// <remarks>Works with safe URLs only.</remarks>
	public static InlineUriBuilder CreateFromUri([StringSyntax(StringSyntaxAttribute.Uri)] ReadOnlySpan<char> uri)
	{
		var builder = Pool.Default.Get();
		builder.SetBaseUrl(uri);
		return builder;
	}

	public static InlineUriBuilder CreateFromUri(Uri uri) => CreateFromUri(uri.OriginalString);


	/// <remarks>Works with safe URLs only.</remarks>
	private void SetBaseUrl(ReadOnlySpan<char> uri)
	{
		if (_lastWritten is not State.Empty)
		{
			throw new InvalidOperationException();
		}

		uri.CopyTo(Remaining);
		_totalWritten += uri.Length;

		if (uri.Contains('#'))
		{
			_lastWritten = State.Fragment;
			return;
		}

		if (uri.Contains('?'))
		{
			_lastWritten = State.Query;
			return;
		}

		if (uri.LastIndexOf('/') > "https://".Length)
		{
			_lastWritten = State.Path;
			return;
		}

		if (uri.LastIndexOf(':') > "https://".Length)
		{
			_lastWritten = State.Port;
			return;
		}

		_lastWritten = State.Host;
	}

	public InlineUriBuilder AppendScheme(ReadOnlySpan<char> scheme)
	{
		if (scheme is not ("http" or "https" or "ws" or "wss"))
		{
			throw new ArgumentOutOfRangeException(nameof(scheme));
		}

		if (_lastWritten is not State.Empty)
		{
			throw new InvalidOperationException();
		}

		EnsureAdditionalBufferLength(scheme.Length + 1);

		scheme.CopyTo(Remaining);
		_totalWritten += scheme.Length;
		Write(':');
		_lastWritten = State.Scheme;

		return this;
	}

	public InlineUriBuilder AppendHost(ReadOnlySpan<char> host)
	{
		if (_lastWritten is not (State.Empty or State.Scheme))
		{
			throw new InvalidOperationException();
		}

		if (host.ContainsAnyExcept(UriExtensions.AllowedHostCharacters))
		{
			throw new ArgumentOutOfRangeException(nameof(host));
		}

		EnsureAdditionalBufferLength(2 + host.Length);

		Write('/');
		Write('/');

		host.CopyTo(Remaining);
		_totalWritten += host.Length;
		_lastWritten = State.Host;

		return this;
	}

	public InlineUriBuilder AppendPort(int port)
	{
		if (_lastWritten is not (State.Host))
		{
			throw new InvalidOperationException();
		}

		Write(':');

		if (!port.TryFormat(Remaining, out var written))
		{
			throw new InvalidOperationException();
		}
		_totalWritten += written;
		_lastWritten = State.Port;

		return this;
	}

	public InlineUriBuilder AppendPath([StringSyntax(StringSyntaxAttribute.Uri)] ReadOnlySpan<char> path)
	{
		if (_lastWritten is not (State.Empty or State.Host or State.Port or State.Path))
		{
			throw new InvalidOperationException();
		}

		if (path.IsEmpty)
		{
			return this;
		}

		if (path.ContainsAnyExcept(UriExtensions.AllowedPathCharacters))
		{
			throw new ArgumentOutOfRangeException(nameof(path));
		}

		EnsureAdditionalBufferLength(path.Length + 1);

		if (_lastWritten is State.Path)
		{
			var toWrite = path;
			if (Written[^1] == '/')
			{
				toWrite = toWrite.TrimStart('/');
			}
			else if (path[0] != '/')
			{
				Write('/');
			}

			toWrite.CopyTo(Remaining);
			_totalWritten += toWrite.Length;
		}
		else
		{
			if (_lastWritten is not State.Empty && path[0] != '/')
			{
				Write('/');
			}

			path.CopyTo(Remaining);
			_totalWritten += path.Length;
		}

		_lastWritten = State.Path;

		return this;
	}

	public InlineUriBuilder AppendPath([InterpolatedStringHandlerArgument(""), StringSyntax(StringSyntaxAttribute.Uri)] ref AppendPathInterpolatedStringHandler handler) => this;

	public InlineUriBuilder AppendPathSegment([StringSyntax(StringSyntaxAttribute.Uri)] ReadOnlySpan<char> segment)
	{
		if (_lastWritten is not (State.Empty or State.Host or State.Port or State.Path))
		{
			throw new InvalidOperationException();
		}

		if (segment.IsEmpty)
		{
			throw new ArgumentOutOfRangeException(nameof(segment));
		}

		if ((_lastWritten is State.Empty or State.Host or State.Port) ||
			(_lastWritten is State.Path && Written[^1] != '/'))
		{
			EnsureAdditionalBufferLength(1);
			Write('/');
		}

		if (segment.ContainsAnyExcept(UriExtensions.AllowedPathSegmentCharacters))
		{
			EnsureAdditionalBufferLength(segment.Length * 3);
			if (_encoder.Encode(segment, Remaining, out _, out var written) != OperationStatus.Done)
			{
				throw new InvalidOperationException();
			}
			_totalWritten += written;
		}
		else
		{
			EnsureAdditionalBufferLength(segment.Length);
			segment.CopyTo(Remaining);
			_totalWritten += segment.Length;
		}

		_lastWritten = State.Path;

		return this;
	}

	public InlineUriBuilder AppendPathSegment(Guid segment)
	{
		if (_lastWritten is not (State.Empty or State.Host or State.Port or State.Path))
		{
			throw new InvalidOperationException();
		}

		if (_lastWritten is State.Empty or State.Host or State.Port ||
			(_lastWritten is State.Path && Written[^1] != '/'))
		{
			EnsureAdditionalBufferLength(1);
			Write('/');
		}

		EnsureAdditionalBufferLength(36);
		if (!segment.TryFormat(Remaining, out var written))
		{
			throw new InvalidOperationException();
		}
		_totalWritten += written;

		_lastWritten = State.Path;

		return this;
	}

	public InlineUriBuilder AppendPathSegment(int segment)
	{
		if (_lastWritten is not (State.Empty or State.Host or State.Port or State.Path))
		{
			throw new InvalidOperationException();
		}

		if (_lastWritten is State.Empty or State.Host or State.Port ||
			(_lastWritten is State.Path && Written[^1] != '/'))
		{
			EnsureAdditionalBufferLength(1);
			Write('/');
		}

		EnsureAdditionalBufferLength(10);
		if (!segment.TryFormat(Remaining, out var written))
		{
			throw new InvalidOperationException();
		}
		_totalWritten += written;

		_lastWritten = State.Path;

		return this;
	}

	public InlineUriBuilder AppendPathSegment(long segment)
	{
		if (_lastWritten is not (State.Empty or State.Host or State.Port or State.Path))
		{
			throw new InvalidOperationException();
		}

		if (_lastWritten is State.Empty or State.Host or State.Port ||
			(_lastWritten is State.Path && Written[^1] != '/'))
		{
			EnsureAdditionalBufferLength(1);
			Write('/');
		}

		EnsureAdditionalBufferLength(20);
		if (!segment.TryFormat(Remaining, out var written))
		{
			throw new InvalidOperationException();
		}
		_totalWritten += written;

		_lastWritten = State.Path;

		return this;
	}

	public InlineUriBuilder AppendPathSegment(DateOnly segment)
	{
		if (_lastWritten is not (State.Empty or State.Host or State.Port or State.Path))
		{
			throw new InvalidOperationException();
		}

		if (_lastWritten is State.Empty or State.Host or State.Port ||
			(_lastWritten is State.Path && Written[^1] != '/'))
		{
			EnsureAdditionalBufferLength(1);
			Write('/');
		}

		EnsureAdditionalBufferLength(10);
		if (!segment.TryFormat(Remaining, out var written, format: "O", provider: CultureInfo.InvariantCulture))
		{
			throw new InvalidOperationException();
		}
		_totalWritten += written;

		_lastWritten = State.Path;

		return this;
	}

	public InlineUriBuilder AppendPathDelimiter()
	{
		if (_lastWritten is not (State.Empty or State.Host or State.Port or State.Path))
		{
			throw new InvalidOperationException();
		}

		EnsureAdditionalBufferLength(1);
		Write('/');
		_lastWritten = State.Path;
		return this;
	}

	public InlineUriBuilder AppendQueryString(ReadOnlySpan<char> name, ReadOnlySpan<char> value)
	{
		EnsureAdditionalBufferLength(1 + name.Length + 1 + value.Length * 6);
		WriteQueryStringName(name);

		if (_encoder.Encode(value, Remaining, out _, out var written) is OperationStatus status and not OperationStatus.Done)
		{
			throw new InvalidOperationException($"Failed to encode query string value: {status}");
		}
		_totalWritten += written;

		_lastWritten = State.Query;

		return this;
	}

	public InlineUriBuilder AppendQueryString(ReadOnlySpan<char> name, string? value)
	{
		if (value == null)
		{
			return this;
		}

		return AppendQueryString(name, value.AsSpan());
	}

	public InlineUriBuilder AppendQueryString(ReadOnlySpan<char> name, IEnumerable<string> values)
	{
		foreach (var value in values)
		{
			AppendQueryString(name, value);
		}

		return this;
	}

	public InlineUriBuilder AppendQueryString(ReadOnlySpan<char> name, ISpanFormattable value, ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
	{
		const int InlineBufferLength = 64;

		EnsureAdditionalBufferLength(1 + name.Length + 1 + InlineBufferLength);
		WriteQueryStringName(name);

		Span<char> inline = stackalloc char[InlineBufferLength];
		if (!value.TryFormat(inline, out var written, format, formatProvider))
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}
		if (_encoder.Encode(inline[..written], Remaining, out _, out var finalWritten) != OperationStatus.Done)
		{
			throw new InvalidOperationException();
		}
		_totalWritten += finalWritten;

		_lastWritten = State.Query;

		return this;
	}

	public InlineUriBuilder AppendQueryString<T>(ReadOnlySpan<char> name, T? value, ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null) where T : struct, ISpanFormattable
	{
		if (!value.HasValue)
		{
			return this;
		}

		const int InlineBufferLength = 64;
		EnsureAdditionalBufferLength(1 + name.Length + 1 + InlineBufferLength);
		WriteQueryStringName(name);

		Span<char> inline = stackalloc char[InlineBufferLength];
		if (!value.Value.TryFormat(inline, out var written, format, formatProvider))
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}
		if (_encoder.Encode(inline[..written], Remaining, out _, out var finalWritten) != OperationStatus.Done)
		{
			throw new InvalidOperationException();
		}
		_totalWritten += finalWritten;

		_lastWritten = State.Query;

		return this;
	}

	public InlineUriBuilder AppendQueryString(ReadOnlySpan<char> name, DateTimeOffset value) =>
		AppendQueryString(name, value, "O");

	public InlineUriBuilder AppendQueryString(ReadOnlySpan<char> name, DateTimeOffset? value)
	{
		if (!value.HasValue)
		{
			return this;
		}

		return AppendQueryString(name, value.Value, "O");
	}

	public InlineUriBuilder AppendQueryString(ReadOnlySpan<char> name, bool value)
	{
		EnsureAdditionalBufferLength(1 + name.Length + 1 + 5);
		WriteQueryStringName(name);

		if (value)
		{
			"true".CopyTo(Remaining);
			_totalWritten += 4;
		}
		else
		{
			"false".CopyTo(Remaining);
			_totalWritten += 5;
		}

		_lastWritten = State.Query;

		return this;
	}

	public InlineUriBuilder AppendQueryString(ReadOnlySpan<char> name, bool? value)
	{
		if (!value.HasValue)
		{
			return this;
		}

		return AppendQueryString(name, value.Value);
	}

	[RequiresUnreferencedCode("Uses reflection to create generic types at runtime.")]
	public InlineUriBuilder AppendAsQueryString<T>(T obj) where T : notnull => AppendAsQueryString(obj, typeof(T));

	[RequiresUnreferencedCode("Uses reflection to create generic types at runtime.")]
	public InlineUriBuilder AppendAsQueryString(object obj, Type type)
	{
		foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).AsReadOnlyList().AsValueEnumerable())
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
					AppendQueryStringCore(property, item, genericType);
				}
			}
			else if (value is IEnumerable enumerable2 && property.PropertyType.IsArray)
			{
				var genericType = property.PropertyType.GetElementType()!;
				foreach (var item in enumerable2)
				{
					AppendQueryStringCore(property, item, genericType);
				}
			}
			else
			{
				AppendQueryStringCore(property, value, property.PropertyType);
			}
		}

		return this;
	}

	[RequiresUnreferencedCode("Uses reflection to create generic types at runtime.")]
	private void AppendQueryStringCore(PropertyInfo property, object? value, Type type)
	{
		var underlyingType = Nullable.GetUnderlyingType(type);
		var isNullable = underlyingType != null;
		if (isNullable)
		{
			value = type.GetProperty(nameof(Nullable<int>.Value))!.GetValue(value);
		}

		switch (value)
		{
			case string str:
				AppendQueryString(property.Name, str);
				break;

			case bool str:
				AppendQueryString(property.Name, str);
				break;

			case DateTime span:
				AppendQueryString(property.Name, span, "O");
				break;

			case DateTimeOffset span:
				AppendQueryString(property.Name, span, "O");
				break;

			case ISpanFormattable span:
				AppendQueryString(property.Name, span);
				break;

			case null:
				break;

			case object obj:
				AppendQueryString(property.Name, obj.ToString());
				break;
		}
	}

	private void WriteQueryStringName(ReadOnlySpan<char> name)
	{
		if (name.IsWhiteSpace())
		{
			throw new ArgumentOutOfRangeException(nameof(name));
		}

		if (name.ContainsAnyExcept(UriExtensions.AllowedQueryNameCharacters))
		{
			throw new ArgumentOutOfRangeException(nameof(name));
		}

		if (_lastWritten is State.Host or State.Port)
		{
			Write('/');
		}
		else if (_lastWritten is not (State.Path or State.Query))
		{
			throw new InvalidOperationException();
		}

		if (_lastWritten is State.Query)
		{
			Write('&');
		}
		else
		{
			Write('?');
		}

		if (Char.IsAsciiLetterUpper(name[0]))
		{
			Remaining[0] = Char.ToLowerInvariant(name[0]);
			name[1..].CopyTo(Remaining[1..]);
		}
		else
		{
			name.CopyTo(Remaining);
		}
		_totalWritten += name.Length;

		Write('=');
	}

	public InlineUriBuilder AppendQueryStringRaw(ReadOnlySpan<char> query)
	{
		if (query.IsEmpty)
		{
			return this;
		}

		if (query[0] != '?')
		{
			throw new ArgumentOutOfRangeException(nameof(query));
		}

		if (_lastWritten is State.Host or State.Port)
		{
			EnsureAdditionalBufferLength(1);
			Write('/');
			_lastWritten = State.Path;
		}

		if (_lastWritten is not State.Path)
		{
			throw new InvalidOperationException();
		}

		EnsureAdditionalBufferLength(query.Length);

		query.CopyTo(Remaining);
		_totalWritten += query.Length;

		_lastWritten = State.Query;

		return this;
	}

	public InlineUriBuilder AppendFragment(ReadOnlySpan<char> fragment)
	{
		if (_lastWritten is State.Host or State.Port)
		{
			EnsureAdditionalBufferLength(1);
			Write('/');
			_lastWritten = State.Path;
		}

		if (_lastWritten is State.Path or State.Query)
		{
			EnsureAdditionalBufferLength(1);
			Write('#');
			_lastWritten = State.Fragment;
		}

		if (_lastWritten is not State.Fragment)
		{
			throw new InvalidOperationException();
		}

		EnsureAdditionalBufferLength(fragment.Length);
		if (_encoder.Encode(fragment, Remaining, out _, out var written) != OperationStatus.Done)
		{
			throw new InvalidOperationException();
		}
		_totalWritten += written;

		_lastWritten = State.Fragment;

		return this;
	}

	public InlineUriBuilder AppendFragmentRaw(ReadOnlySpan<char> fragment)
	{
		if (fragment.IsEmpty)
		{
			return this;
		}

		if (fragment[0] != '#')
		{
			throw new ArgumentOutOfRangeException(nameof(fragment));
		}

		if (_lastWritten is State.Host or State.Port)
		{
			EnsureAdditionalBufferLength(1);
			Write('/');
			_lastWritten = State.Path;
		}

		if (_lastWritten is not (State.Path or State.Query))
		{
			throw new InvalidOperationException();
		}

		EnsureAdditionalBufferLength(fragment.Length);

		fragment.CopyTo(Remaining);
		_totalWritten += fragment.Length;

		_lastWritten = State.Fragment;

		return this;
	}

	public InlineUriBuilder AppendFragmentQueryString(ReadOnlySpan<char> name, string? value)
	{
		if (name.IsWhiteSpace())
		{
			throw new ArgumentOutOfRangeException(nameof(name));
		}

		if (name.ContainsAnyExcept(UriExtensions.AllowedQueryNameCharacters))
		{
			throw new ArgumentOutOfRangeException(nameof(name));
		}

		if (value == null)
		{
			return this;
		}

		if (_lastWritten is State.Host or State.Port)
		{
			Write('/');
		}
		else if (_lastWritten is not (State.Path or State.Query or State.Fragment))
		{
			throw new InvalidOperationException();
		}

		if (_lastWritten is State.Fragment)
		{
			Write(';');
		}
		else
		{
			Write('#');
		}

		name.CopyTo(Remaining);
		_totalWritten += name.Length;

		Write('=');

		if (_encoder.Encode(value, Remaining, out _, out var written) != OperationStatus.Done)
		{
			throw new InvalidOperationException();
		}
		_totalWritten += written;

		_lastWritten = State.Fragment;

		return this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void EnsureBufferLength(int desiredLength)
	{
		if (desiredLength < _buffer.Length)
		{
			return;
		}

		var newBuffer = BufferPool.Rent(desiredLength);
		Written.CopyTo(newBuffer);
		BufferPool.Return(_buffer, clearArray: false);
		_buffer = newBuffer;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void EnsureAdditionalBufferLength(int delta) => EnsureBufferLength(_totalWritten + delta);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Write(char ch)
	{
		Remaining[0] = ch;
		_totalWritten++;
	}


	public void Clear()
	{
		_totalWritten = 0;
		BufferPool.Return(_buffer, clearArray: false);
		_buffer = [];
		_lastWritten = State.Empty;
	}


	public string GetIntermediateString() => new(Written);

	public override string ToString()
	{
		var result = GetIntermediateString();
		Dispose();
		return result;
	}

	public Uri ToUri() => new(ToString(), UriKind.RelativeOrAbsolute);


	public static implicit operator string(InlineUriBuilder builder) => builder.ToString();

	public static implicit operator Uri(InlineUriBuilder builder) => builder.ToUri();


	public void Dispose()
	{
		Clear();
		_lastWritten = State.Finished;
		Pool.Default.Return(this);
	}


	private enum State
	{
		Empty,
		Scheme,
		Host,
		Port,
		Path,
		Query,
		Fragment,
		Finished,
	}


	[InterpolatedStringHandler]
	public readonly ref struct AppendPathInterpolatedStringHandler
	{
		public AppendPathInterpolatedStringHandler(int literalLength, int formattedCount, InlineUriBuilder builder)
		{
			_builder = builder;
		}

		private readonly InlineUriBuilder _builder;

		public void AppendLiteral(string literal)
		{
			if (literal is null)
			{
				return;
			}

			_builder.AppendPath(literal.AsSpan());
		}

		public void AppendFormatted<T>(T value)
		{
			if (value is null)
			{
				return;
			}

			switch (value)
			{
				case int i:
					_builder.AppendPathSegment(i);
					break;

				case Guid i:
					_builder.AppendPathSegment(i);
					break;

				case string str:
					_builder.AppendPathSegment(str);
					break;

				default:
					throw new NotSupportedException($"Type {value.GetType()} is not supported for inline URI building.");
			}
		}
	}

	public static partial class Pool
	{
		public static readonly ObjectPool<InlineUriBuilder> Default = DefaultPool.Create(Policy.Instance);

		public static ObjectPool<InlineUriBuilder> Create(int size = 20)
			=> DefaultPool.Create(Policy.Instance, size);

		public static PooledObject<InlineUriBuilder> GetPooledObject()
			=> Default.GetPooledObject();

		public static PooledObject<InlineUriBuilder> GetPooledObject(out InlineUriBuilder list)
			=> Default.GetPooledObject(out list);

		private sealed class Policy() : IPooledObjectPolicy<InlineUriBuilder>
		{
			public static readonly Policy Instance = new();

			public InlineUriBuilder Create() => new();

			public bool Return(InlineUriBuilder list)
			{
				list.Clear();
				return true;
			}
		}
	}
}

public static partial class Extensions
{
	extension(Uri uri)
	{
		public InlineUriBuilder ToBuilder() => InlineUriBuilder.CreateFromUri(uri);
	}
}
