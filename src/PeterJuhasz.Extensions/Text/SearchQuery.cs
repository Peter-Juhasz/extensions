using Microsoft.Extensions.Primitives;
using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace System.Text;

public class SearchQuery
{
	private SearchQuery(string query)
	{
		_query = query;
	}

	private SearchQuery(ReadOnlySpan<string> phrases)
	{
		_query = string.Join(" ", phrases!);
		var items = new NormalizedText[phrases.Length];
		for (int i = 0; i < phrases.Length; i++)
		{
			var original = phrases[i];
			items[i] = NormalizedText.Create(original);
		}
		_phrases = items;
	}

	private const int MaxStackLength = 1024;
	private static readonly CompareInfo _compareInfo = CultureInfo.CurrentCulture.CompareInfo;

	public static SearchQuery Create(string query)
	{
		if (query.IsNullOrWhiteSpace())
		{
			throw new ArgumentNullException(nameof(query), "Query cannot be null or whitespace.");
		}

		return new SearchQuery(query.Trim());
	}

	public static SearchQuery Create(ReadOnlySpan<string> phrases)
	{
		if (phrases.IsEmpty)
		{
			throw new ArgumentNullException(nameof(phrases), "Phrases cannot be empty.");
		}

		if (phrases.Length == 1)
		{
			return Create(phrases[0]);
		}

		return new SearchQuery(phrases);
	}

	private readonly string _query;
	public string Query => _query;

	private string? _normalizedQuery = null;
	[MemberNotNull(nameof(_normalizedQuery))]
	public string NormalizedQuery
	{
		get
		{
			if (_normalizedQuery == null)
			{
				_normalizedQuery = NormalizedText.Normalize(_query);
			}

			return _normalizedQuery;
		}
	}

	private bool? _hasMultiplePhrases = null;
	[MemberNotNull(nameof(_hasMultiplePhrases))]
	public bool HasMultiplePhrases
	{
		get
		{
			if (_hasMultiplePhrases == null)
			{
				if (_phrases != null)
				{
					_hasMultiplePhrases = _phrases.Length > 1;
				}

				else if (_query.Contains(' '))
				{
					_hasMultiplePhrases = true;
				}

				else
				{
					_hasMultiplePhrases = false;
				}
			}

			return _hasMultiplePhrases.Value;
		}
	}

	private NormalizedText[]? _phrases = null;
	[MemberNotNull(nameof(_phrases))]
	public ImmutableArray<NormalizedText> Phrases
	{
		get
		{
			if (_phrases == null)
			{
				var span = _query.AsSpan();
				using var builder = new PooledArrayBuilder<NormalizedText>();
				foreach (var range in _query.AsSpan().Split(' '))
				{
					var phraseSpan = span[range].Trim();
					if (phraseSpan.IsEmpty)
					{
						continue;
					}

					var original = phraseSpan.ToString();
					var phrase = NormalizedText.Create(original);
					builder.Add(phrase);
				}
				_phrases = builder.ToArray();
			}

			return ImmutableCollectionsMarshal.AsImmutableArray(_phrases);
		}
	}

	private SearchValues<string>? _normalizedSearchValues = null;
	[MemberNotNull(nameof(_normalizedSearchValues))]
	public SearchValues<string> GetNormalizedSearchValues()
	{
		if (_normalizedSearchValues == null)
		{
			if (HasMultiplePhrases)
			{
				var phrases = Phrases;
				var strings = ArrayPool<string>.Shared.Rent(phrases.Length);
				for (int i = 0; i < phrases.Length; i++)
				{
					strings[i] = phrases[i].NormalizedForm;
				}
				_normalizedSearchValues = SearchValues.Create(strings.AsSpan(0, phrases.Length), StringComparison.OrdinalIgnoreCase);
				ArrayPool<string>.Shared.Return(strings);
			}
			else
			{
				_normalizedSearchValues = SearchValues.Create([NormalizedQuery], StringComparison.OrdinalIgnoreCase);
			}
		}

		return _normalizedSearchValues;
	}

	public bool ContainsAnyNormalized(ReadOnlySpan<char> normalizedStr)
	{
		if (normalizedStr.IsWhiteSpace())
		{
			return false;
		}

		if (HasMultiplePhrases)
		{
			var searchValues = GetNormalizedSearchValues();
			foreach (var index in normalizedStr.IndexesOf(searchValues))
			{
				if (index == 0 || Char.IsWhiteSpace(normalizedStr[index - 1]))
				{
					return true;
				}
			}

			return false;
		}
		else
		{
			return normalizedStr.ContainsAtWordStart(NormalizedQuery, StringComparison.Ordinal);
		}
	}

	public bool ContainsAny(string str)
	{
		if (str.IsNullOrWhiteSpace())
		{
			return false;
		}

		var normalized = NormalizedText.Normalize(str);
		return ContainsAnyNormalized(normalized);
	}

	public bool ContainsAny(NormalizedText str) => ContainsAnyNormalized(str.NormalizedForm);

	public bool ContainsAny<T>(TextSearchOptimized<T> str)
	{
		foreach (var text in str.NormalizedText)
		{
			if (ContainsAnyNormalized(text))
			{
				return true;
			}
		}

		return false;
	}

	public bool ContainsAllNormalized(ReadOnlySpan<char> normalizedStr)
	{
		if (normalizedStr.IsWhiteSpace())
		{
			return false;
		}

		if (HasMultiplePhrases)
		{
			foreach (var phrase in Phrases)
			{
				if (!normalizedStr.ContainsAtWordStart(phrase.NormalizedForm, StringComparison.Ordinal))
				{
					return false;
				}
			}

			return true;
		}
		else
		{
			return normalizedStr.ContainsAtWordStart(NormalizedQuery, StringComparison.Ordinal);
		}
	}

	public bool ContainsAll(ReadOnlySpan<char> str)
	{
		if (str.IsWhiteSpace())
		{
			return false;
		}

		if (!HasMultiplePhrases)
		{
			return str.IndexOfAtWordStart(_query, _compareInfo, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) != -1;
		}
		else if (!NormalizedText.ShouldNormalize(str))
		{
			return ContainsAllNormalized(str);
		}
		else if (str.Length <= MaxStackLength)
		{
			Span<char> output = stackalloc char[str.Length];
			NormalizedText.Normalize(str, output);
			return ContainsAllNormalized(output);
		}
		else
		{
			char[] output = ArrayPool<char>.Shared.Rent(str.Length);
			NormalizedText.Normalize(str, output);
			var result = ContainsAllNormalized(output);
			ArrayPool<char>.Shared.Return(output);
			return result;
		}

	}

	public bool ContainsAll(NormalizedText str) => ContainsAllNormalized(str.NormalizedForm);

	public bool ContainsAll<T>(TextSearchOptimized<T> str)
	{
		if (str.NormalizedText is { Count: 0 })
		{
			return false;
		}

		if (str.NormalizedText is [var single])
		{
			return ContainsAllNormalized(single);
		}

		if (!HasMultiplePhrases)
		{
			return ContainsAny(str);
		}

		// find phrases scattered across multiple texts
		var phrases = Phrases;
		Span<bool> contains = stackalloc bool[phrases.Length];

		foreach (var text in str.NormalizedText)
		{
			foreach (var matchRange in MatchesNormalized(text))
			{
				for (int phraseIndex = 0; phraseIndex < phrases.Length; phraseIndex++)
				{
					var matchSpan = text.AsSpan()[matchRange];
					if (phrases[phraseIndex].NormalizedForm.AsSpan().SequenceEqual(matchSpan))
					{
						contains[phraseIndex] = true;
						break;
					}
				}
			}
		}

		foreach (var b in contains)
		{
			if (!b)
			{
				return false;
			}
		}

		return true;
	}

	public bool ContainsAll<T>(LazyTextSearchOptimized<T> str) => ContainsAll((TextSearchOptimized<T>)str);


	public bool TryIndexOfAnyNormalized(ReadOnlySpan<char> normalizedStr, out Range match)
	{
		if (HasMultiplePhrases)
		{
			var searchValues = GetNormalizedSearchValues();
			foreach (var index in normalizedStr.IndexesOf(searchValues))
			{
				if (index == 0 || !Char.IsLetterOrDigit(normalizedStr[index - 1]))
				{
					var remaining = normalizedStr[index..];
					foreach (var phrase in _phrases!)
					{
						if (remaining.StartsWith(phrase.NormalizedForm))
						{
							match = index..(index + phrase.NormalizedForm.Length);
							return true;
						}
					}

					throw new InvalidOperationException("No match found in normalized string.");
				}
			}
		}
		else
		{
			var index = normalizedStr.IndexOfAtWordStart(NormalizedQuery, StringComparison.Ordinal);
			if (index != -1)
			{
				match = index..(index + NormalizedQuery.Length);
				return true;
			}
		}

		match = default;
		return false;
	}

	public bool TryIndexOfAny(string str, out Range range)
	{
		if (!HasMultiplePhrases)
		{
			var index = str.IndexOfAtWordStart(_query, _compareInfo, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);
			if (index != -1)
			{
				range = index..(index + _query.Length);
				return true;
			}

			range = default;
			return false;
		}
		else if (!NormalizedText.ShouldNormalize(str))
		{
			return TryIndexOfAnyNormalized(str, out range);
		}
		else if (str.Length <= MaxStackLength)
		{
			Span<char> output = stackalloc char[str.Length];
			NormalizedText.Normalize(str, output);
			return TryIndexOfAnyNormalized(output, out range);
		}
		else
		{
			char[] output = ArrayPool<char>.Shared.Rent(str.Length);
			NormalizedText.Normalize(str, output);
			var result = TryIndexOfAnyNormalized(output, out range);
			ArrayPool<char>.Shared.Return(output);
			return result;
		}
	}

	public bool TryIndexOfAny(NormalizedText str, out Range range) => TryIndexOfAnyNormalized(str.NormalizedForm, out range);

	public bool TryIndexOfAny<T>(TextSearchOptimized<T> str, out Range range)
	{
		if (str.NormalizedText is { Count: 0 })
		{
			range = default;
			return false;
		}

		if (str.NormalizedText is [var single])
		{
			return TryIndexOfAnyNormalized(single, out range);
		}

		throw new NotSupportedException("Multiple phrases are not supported for IndexOf operation.");
	}

	public IndexOfEnumerable MatchesNormalized(ReadOnlySpan<char> normalizedStr) => new(normalizedStr, this);

	public IndexOfEnumerable Matches(string str)
	{
		var normalized = NormalizedText.Normalize(str);
		return MatchesNormalized(normalized);
	}

	public IndexOfEnumerable Matches(NormalizedText str) => MatchesNormalized(str.NormalizedForm);


	public readonly ref struct IndexOfEnumerable
	{
		public IndexOfEnumerable(ReadOnlySpan<char> span, SearchQuery ch)
		{
			this.span = span;
			this.ch = ch;
		}

		private readonly ReadOnlySpan<char> span;
		private readonly SearchQuery ch;

		public Enumerator GetEnumerator() => new(span, ch);

		public ref struct Enumerator
		{
			public Enumerator(ReadOnlySpan<char> span, SearchQuery ch)
			{
				this.span = span;
				this.ch = ch;
				Reset();
			}

			private readonly ReadOnlySpan<char> span;
			private readonly SearchQuery ch;
			private int _currentIndex;
			private int _matchLength;

			public readonly Range Current
			{
				get
				{
					if (_currentIndex == -1)
					{
						return default;
					}

					return _currentIndex..(_currentIndex + _matchLength);
				}
			}

			public void Reset() => _currentIndex = -1;

			public bool MoveNext()
			{
				var startIndex = _currentIndex;
				var remaining = span[(_currentIndex + 1)..];
				if (!ch.TryIndexOfAnyNormalized(remaining, out var match))
				{
					return false;
				}

				var (newRelativeIndex, length) = match.GetOffsetAndLength(remaining.Length);
				_matchLength = length;
				_currentIndex = startIndex + 1 + newRelativeIndex;
				return true;
			}
		}
	}
}

public readonly record struct NormalizedText(
	string OriginalForm,
	string NormalizedForm
)
{
	private static readonly SearchValues<char> NormalizedOnly = SearchValues.Create("abcdefghijklmnopqrstuvwxyz0123456789.:;_()[]+- ");

	public static NormalizedText Create(string originalForm) => new(originalForm, Normalize(originalForm));

	public static void Normalize(ReadOnlySpan<char> text, Span<char> output)
	{
		for (int i = 0; i < text.Length; i++)
		{
			output[i] = text[i].RemoveDiacriticsLatinExtendedB();
		}

		var span = output[..text.Length];
		if (Ascii.ToLowerInPlace(span, out _) == OperationStatus.InvalidData)
		{
			if (span.Length < 32)
			{
				for (int i = 0; i < span.Length; i++)
				{
					span[i] = span[i].ToLowerInvariant();
				}
			}
			else if (span.Length < 1024)
			{
				Span<char> temporary = stackalloc char[span.Length];
				span.CopyTo(temporary);
				temporary[..span.Length].AsReadOnly().ToLowerInvariant(span);
			}
			else
			{
				char[] temporary = ArrayPool<char>.Shared.Rent(span.Length);
				span.CopyTo(temporary);
				temporary.AsSpan(0, span.Length).AsReadOnly().ToLowerInvariant(span);
				ArrayPool<char>.Shared.Return(temporary);
			}
		}
	}

	public static bool ShouldNormalize(ReadOnlySpan<char> text) => text.ContainsAnyExcept(NormalizedOnly);

	public static string Normalize(string text)
	{
		if (!ShouldNormalize(text))
		{
			return text;
		}

		return String.Create(text.Length, text, static (buffer, state) =>
		{
			Normalize(state, buffer);
		});
	}
}

public readonly record struct TextSearchOptimized<T>(
	T Value,
	StringValues NormalizedText
)
{
	public static TextSearchOptimized<T> Create(T value, string originalText) => new(value, System.Text.NormalizedText.Normalize(originalText));

	public static implicit operator T(TextSearchOptimized<T> value) => value.Value;
}

public struct LazyTextSearchOptimized<T>(T value, Func<T, StringValues> selector)
{
	public readonly T Value => value;

	private StringValues? _normalizedText;
	public StringValues NormalizedText
	{
		get
		{
			if (!_normalizedText.HasValue)
			{
				var texts = selector(value);
				var count = texts.Count;
				if (count == 0)
				{
					_normalizedText = StringValues.Empty;
				}
				else if (count == 1)
				{
					_normalizedText = new StringValues(System.Text.NormalizedText.Normalize(texts[0]!));
				}
				else
				{
					var normalizedTexts = new string[texts.Count];
					for (int i = 0; i < texts.Count; i++)
					{
						normalizedTexts[i] = System.Text.NormalizedText.Normalize(texts[i]!);
					}
					_normalizedText = new StringValues(normalizedTexts);
				}
			}

			return _normalizedText.Value;
		}
	}

	public static LazyTextSearchOptimized<T> Create(T value, Func<T, StringValues> selector) => new(value, selector);

	public static implicit operator T(LazyTextSearchOptimized<T> value) => value.Value;

	public static implicit operator TextSearchOptimized<T>(LazyTextSearchOptimized<T> value) => new(value.Value, value.NormalizedText);
}

public static partial class Extensions
{
	public static bool Contains<T>(this TextSearchOptimized<T> optimized, SearchQuery query) => query.ContainsAll(optimized);

	public static bool Contains<T>(this LazyTextSearchOptimized<T> optimized, SearchQuery query) => query.ContainsAll(optimized);

	public static SearchQuery ToSearchQuery(this string str) => SearchQuery.Create(str);
}

public class SearchQueryJsonConverter : JsonConverter<SearchQuery>
{
	public override SearchQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType switch
	{
		JsonTokenType.String => SearchQuery.Create(reader.GetString()!),
		JsonTokenType.Null => null!,
		_ => throw new JsonException(),
	};

	public override void Write(Utf8JsonWriter writer, SearchQuery value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStringValue(value.Query);
	}
}
