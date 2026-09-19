using Microsoft.Extensions.Primitives;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace System;

public static class CharExtensions
{
	// Characters from \u00c0 till \u024f: https://symbl.cc/en/unicode-table/#latin-1-supplement
	private const string LatinExtendedB =
			"aaaaaaaceeeeiiii" +
			"DNOOOOO\u00d7\u00d8UUUUYI\u00df" +
			"aaaaaaaceeeeiiii" +
			"\u00f0nooooo\u00f7\u00f8uuuuy\u00fey" +
			"aaaaaaccccccccdd" +
			"ddeeeeeeeeeegggg" +
			"gggghhhhiiiiiiii" +
			"iijjjjkkklllllll" +
			"lllnnnnnnnnnoooo" +
			"oooorrrrrrssssss" +
			"ssttttttuuuuuuuu" +
			"uuuuwwyyyzzzzzzf" +
			"bbbbbboccddddoee" +
			"effgyhltikklawnn" +
			"ooooopprsseltttt" +
			"uuuuyyzz3ee3255t" +
			"plll!dddjjjnnnaa" +
			"iioouuuuuuuuuuea" +
			"aaaaaggggkkoooo3" +
			"3jdddgghpnnaaaao" +
			"oaaaaeeeeiiiiooo" +
			"orrrruuuusstt33h" +
			"hnd88zzaaeeooooo" +
			"oooyybnbjbpacclt" +
			"sz??buaeejjqrrryy";

	extension(char character)
	{
		public char RemoveDiacriticsLatinExtendedB()
		{
			if (character is >= '\u00c0' and <= '\u024f')
			{
				return LatinExtendedB[character - '\u00c0'];
			}
			return character;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public char ToUpperAscii()
		{
			if (character >= 'a' && character <= 'z')
			{
				return (char)(character - 32);
			}
			return character;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public char ToLowerAscii()
		{
			if (character >= 'A' && character <= 'Z')
			{
				return (char)(character + 32);
			}
			return character;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public char ToLowerInvariant() => char.ToLowerInvariant(character);
	}

	public static void RemoveDiacriticsLatinExtendedB(ReadOnlySpan<char> text, Span<char> output)
	{
		for (int i = 0; i < text.Length; i++)
		{
			output[i] = text[i].RemoveDiacriticsLatinExtendedB();
		}
	}

	extension(string text)
	{
		public string RemoveDiacriticsLatinExtendedB() => string.Create(text.Length, text, static (span, state) =>
		{
			RemoveDiacriticsLatinExtendedB(state, span);
		});
	}
}

public static partial class Extensions
{
	extension(string)
	{
		public static string Create(Action<StringBuilder> action)
		{
			using var _ = StringBuilderPool.GetPooledObject(out var sb);
			action(sb);
			return sb.ToString();
		}
	}

	extension([NotNullWhen(true)] string? n)
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Any() => n?.Length > 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool AnyNonWhiteSpace() => !n.IsNullOrWhiteSpace();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool AnyNonWhiteSpace([NotNullWhen(true)] out string? str)
		{
			str = n;
			return !n.IsNullOrWhiteSpace();
		}
	}

	extension(string? str)
	{
		[OverloadResolutionPriority(1)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string Safe() => str ?? String.Empty;

		public string? DefaultIfEmpty() => str.IsNullOrWhiteSpace() ? null : str;

		public bool TryMatch([StringSyntax(StringSyntaxAttribute.Regex)] string regex, [NotNullWhen(true)] out Match? match)
		{
			if (str == null)
			{
				match = null;
				return false;
			}

			if (Regex.Match(str, regex) is { Success: true } m)
			{
				match = m;
				return true;
			}

			match = null;
			return false;
		}

		public bool TryMatch(Regex regex, [NotNullWhen(true)] out Match? match)
		{
			if (str == null)
			{
				match = null;
				return false;
			}

			if (regex.Match(str) is { Success: true } m)
			{
				match = m;
				return true;
			}

			match = null;
			return false;
		}
	}

	extension([NotNullWhen(false)] string? str)
	{
		public bool IsNullOrEmpty() => string.IsNullOrEmpty(str);

		public bool IsNullOrWhiteSpace() => string.IsNullOrWhiteSpace(str);

		public StringValues ToStringValues() => str.IsNullOrWhiteSpace() ? StringValues.Empty : new(str);
	}

	extension(string source)
	{
		public string TrimPrefix(string prefix, StringComparison comparison = StringComparison.Ordinal)
		{
			if (source.StartsWith(prefix, comparison))
			{
				return source[prefix.Length..];
			}
			return source;
		}

		public bool ContainsAtWordStart(string value, StringComparison comparison) => source.IndexOfAtWordStart(value, comparison) != -1;

		public bool ContainsAtWordStart(string value, CompareInfo compareInfo, CompareOptions compareOptions) => source.IndexOfAtWordStart(value, compareInfo, compareOptions) != -1;



		public int IndexOfAtWordStart(string value, StringComparison comparison, int startIndex = 0)
		{
			var index = IndexOfAtWordStart(source.AsSpan(startIndex), value.AsSpan(), comparison);
			if (index == -1)
			{
				return -1;
			}

			return startIndex + index;
		}

		public int IndexOfAtWordStart(string value, CompareInfo compareInfo, CompareOptions compareOptions, int startIndex = 0)
		{
			var index = IndexOfAtWordStart(source.AsSpan(startIndex), value.AsSpan(), compareInfo, compareOptions);
			if (index == -1)
			{
				return -1;
			}
			return startIndex + index;
		}
	}

	extension(ReadOnlySpan<char> str)
	{
		public IndexOfCharEnumerable IndexesOf(char ch) => new(str, ch);
		public IndexOfCharSpanEnumerable IndexesOf(ReadOnlySpan<char> subject, StringComparison comparison) => new(str, subject, comparison);
		public IndexOfCharSpanCompareOptionsEnumerable IndexesOf(ReadOnlySpan<char> subject, CompareInfo compareInfo, CompareOptions compareOptions) => new(str, subject, compareInfo, compareOptions);
		public IndexOfSearchValuesEnumerable IndexesOf(SearchValues<string> ch) => new(str, ch);


		public int Count(SearchValues<char> subject)
		{
			var count = 0;
			var remaining = str;
			var index = remaining.IndexOfAny(subject);
			while (index != -1)
			{
				count++;
				remaining = remaining[(index + 1)..];
				index = remaining.IndexOfAny(subject);
			}
			return count;
		}


		public int IndexOfAtWordBoundary(ReadOnlySpan<char> subject, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase)
		{
			foreach (var index in str.IndexesOf(subject, comparison))
			{
				var wordBoundaryAtStart = index == 0 || !Char.IsLetterOrDigit(str[index - 1]);
				var wordBoundaryAtEnd = index + subject.Length == str.Length || !Char.IsLetterOrDigit(str[index + subject.Length]);
				if (wordBoundaryAtStart && wordBoundaryAtEnd)
				{
					return index;
				}
			}

			return -1;
		}

		public int IndexOfAtWordStart(ReadOnlySpan<char> subject, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase)
		{
			foreach (var index in str.IndexesOf(subject, comparison))
			{
				var wordBoundaryAtStart = index == 0 || !Char.IsLetterOrDigit(str[index - 1]);
				if (wordBoundaryAtStart)
				{
					return index;
				}
			}

			return -1;
		}

		public int IndexOfAtWordStart(ReadOnlySpan<char> subject, CompareInfo compareInfo, CompareOptions compareOptions)
		{
			foreach (var index in str.IndexesOf(subject, compareInfo, compareOptions))
			{
				var wordBoundaryAtStart = index == 0 || !Char.IsLetterOrDigit(str[index - 1]);
				if (wordBoundaryAtStart)
				{
					return index;
				}
			}

			return -1;
		}

		public bool ContainsAtWordBoundary(ReadOnlySpan<char> subject, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase) => str.IndexOfAtWordBoundary(subject, comparison) >= 0;

		public bool ContainsAtWordStart(ReadOnlySpan<char> subject, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase) => str.IndexOfAtWordStart(subject, comparison) >= 0;

		public int IndexOfAtWhiteSpaceBoundary(ReadOnlySpan<char> subject, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase)
		{
			foreach (var index in str.IndexesOf(subject, comparison))
			{
				var wordBoundaryAtStart = index == 0 || Char.IsWhiteSpace(str[index - 1]);
				var wordBoundaryAtEnd = index + subject.Length == str.Length || Char.IsWhiteSpace(str[index + subject.Length]);
				if (wordBoundaryAtStart && wordBoundaryAtEnd)
				{
					return index;
				}
			}

			return -1;
		}

		public bool ContainsAtWhiteSpaceBoundary(ReadOnlySpan<char> subject, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase) => str.IndexOfAtWhiteSpaceBoundary(subject, comparison) >= 0;
	}

	extension(string str)
	{
		public IndexOfCharEnumerable IndexesOf(char ch) => ((ReadOnlySpan<char>)str).IndexesOf(ch);
		public IEnumerable<int> IndexesOf(string subject, StringComparison comparison)
		{
			var index = str.IndexOf(subject, comparison);
			while (index != -1)
			{
				yield return index;
				index = str.IndexOf(subject, index + 1, comparison);
			}
		}

		public bool TryMatch(Regex regex, [NotNullWhen(true)] out string? result, string group = "value")
		{
			if (regex.Match(str) is { Success: true } match)
			{
				result = match.Groups[group].Value;
				return true;
			}

			result = null;
			return false;
		}

		public bool TryMatch([StringSyntax(StringSyntaxAttribute.Regex)] string regex, [NotNullWhen(true)] out string? result, string group = "value") =>
			str.TryMatch(new Regex(regex), out result, group);

		public string TrimEnd(int length, string ellipsis = "...")
		{
			if (str.Length <= length)
			{
				return str;
			}
			return string.Concat(str.AsSpan(0, length - ellipsis.Length), ellipsis);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public char IndexSafe(int index) => index < 0 || index >= str.Length ? default : str[index];
	}

	extension(string input)
	{
		public string RemoveDiacritics()
		{
			string stFormD = input.Normalize(NormalizationForm.FormD);
			using var _ = StringBuilderPool.GetPooledObject(out var sb);

			for (int i = 0; i < stFormD.Length; i++)
			{
				var uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[i]);
				if (uc != UnicodeCategory.NonSpacingMark)
				{
					sb.Append(stFormD[i]);
				}
			}

			return sb.ToString().Normalize(NormalizationForm.FormC);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ReplaceRegex([StringSyntax(StringSyntaxAttribute.Regex, "options")] string pattern, string replacement, RegexOptions options = RegexOptions.None) => Regex.Replace(input, pattern, replacement, options);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ReplaceRegex(Regex pattern, string replacement) => pattern.Replace(input, replacement);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string RemoveRegex(Regex pattern) => input.ReplaceRegex(pattern, String.Empty);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string RemoveRegex([StringSyntax(StringSyntaxAttribute.Regex, "options")] string pattern, RegexOptions options = RegexOptions.None) => input.ReplaceRegex(pattern, String.Empty, options);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int IndexOfRegex([StringSyntax(StringSyntaxAttribute.Regex, "options")] string pattern, int startIndex = 0, RegexOptions options = RegexOptions.None) => Regex.Match(input[startIndex..], pattern, options) switch
		{
			{ Success: true } m => startIndex + m.Index,
			_ => -1
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int IndexOfRegex(Regex regex, int startIndex = 0) => regex.Match(input[startIndex..]) switch
		{
			{ Success: true } m => startIndex + m.Index,
			_ => -1
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MatchCollection RegexMatches(Regex regex) => regex.Matches(input);

		public string Replace(Range range, ReadOnlySpan<char> replacement)
		{
			var (offset, length) = range.GetOffsetAndLength(input.Length);
			var buffer = new char[input.Length - length + replacement.Length];
			input.AsSpan(0, offset).CopyTo(buffer);
			replacement.CopyTo(buffer.AsSpan(offset));
			input.AsSpan(offset + length).CopyTo(buffer.AsSpan(offset + replacement.Length));
			return new string(buffer);
		}
	}

	extension(StringSegment segment)
	{
		public StringSegment Subsegment(Range range) => range.GetOffsetAndLength(segment.Length) is (int offset, int length) ? segment.Subsegment(offset, length) : throw new SwitchExpressionException();
	}

	extension(StringSegment str)
	{
		public bool StartsWith(char ch) => str.Length > 0 && str[0] == ch;

		public bool All(char ch) => !str.AsSpan().ContainsAnyExcept(ch);

		public int IndexOf(string subject, int start, StringComparison comparison)
		{
			var index = str.AsSpan(start).IndexOf(subject, comparison);
			if (index == -1)
			{
				return -1;
			}

			return start + index;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public char IndexSafe(int index) => index < 0 || index >= str.Length ? default : str[index];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public char PeekSafe(int index, int relative) => str.IndexSafe(index + relative);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public char PeekNextSafe(int index = 0, int relative = 1) => str.IndexSafe(index + relative);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public char PeekPreviousSafe(int index, int relative = 1) => str.IndexSafe(index - relative);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public char PeekPreviousOutOfBoundsSafe(int index = 0, int relative = 1) => str.Buffer!.IndexSafe(str.Offset + index - relative);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public char PeekNextOutOfBoundsSafe(int index = 0, int relative = 1) => str.Buffer!.IndexSafe(str.Offset + index + relative);
	}

	extension(StringSegment inner)
	{
		public int ToRelativeOffset(StringSegment outer)
		{
			if (inner.Buffer != outer.Buffer)
			{
				throw new ArgumentException("Segments must have the same buffer");
			}

			return inner.Offset - outer.Offset;
		}
	}

	extension<T>(ReadOnlySpan<T> span) where T : IEquatable<T>
	{
		public int CountWhile(SearchValues<T> set)
		{
			var firstNotIndex = span.IndexOfAnyExcept(set);
			if (firstNotIndex == -1)
			{
				return span.Length;
			}

			return firstNotIndex;
		}

		public int CountWhile(T value)
		{
			var firstNotIndex = span.IndexOfAnyExcept(value);
			if (firstNotIndex == -1)
			{
				return span.Length;
			}
			return firstNotIndex;
		}

		public int CountWhileBackwards(SearchValues<T> set)
		{
			var firstNotIndex = span.LastIndexOfAnyExcept(set);
			if (firstNotIndex == -1)
			{
				return span.Length;
			}

			return span.Length - (firstNotIndex + 1);
		}
	}

	extension(ReadOnlySpan<char> span)
	{
		public bool StartsWithAny(ReadOnlySpan<string> values, StringComparison comparison = StringComparison.Ordinal)
		{
			foreach (var value in values)
			{
				if (span.StartsWith(value, comparison))
				{
					return true;
				}
			}

			return false;
		}
	}

	extension(StringBuilder sb)
	{
		public bool Any() => sb.Length > 0;

		public StringBuilder TrimEnd()
		{
			int count;
			for (count = 0; count < sb.Length; count++)
			{
				if (!char.IsWhiteSpace(sb[sb.Length - count - 1]))
				{
					break;
				}
			}

			if (count > 0)
			{
				sb.Length -= count;
			}

			return sb;
		}

		public void CopyTo(Span<char> destination) => sb.CopyTo(0, destination, sb.Length);
	}

	extension(IEnumerable<string?> strings)
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string Join(string separator) => String.Join(separator, strings);
	}
}

public static partial class StringExtensions
{
	private static readonly ConcurrentDictionary<string, string> ToLowerCache = new();
	private static readonly ConcurrentDictionary<string, string> ToUpperCache = new();
	private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> FormatCache = new();

	extension(string str)
	{
		public string ToLowerInvariantCached() => ToLowerCache.GetOrAdd(str, static s => s.ToLowerInvariant());

		public string ToUpperInvariantCached() => ToUpperCache.GetOrAdd(str, static s => s.ToUpperInvariant());

		public StringSegment AsSegment(int offset, int length) => new(str, offset, length);

		public StringSegment GetSplitSegment(char separator, int index)
		{
			var range = str.AsSpan().GetSplitRange(separator, index);
			var (offset, length) = range.GetOffsetAndLength(str.Length);
			return new StringSegment(str, offset, length);
		}

		public string GetSplitString(char separator, int index) =>
			str[str.AsSpan().GetSplitRange(separator, index)];
	}

	extension(string format)
	{
		public string FormatInvariantCached(string arg)
		{
			var cache = FormatCache.GetOrAdd(format, static _ => new());
			if (cache.TryGetValue(arg, out var result))
			{
				return result;
			}

			return cache.GetOrAdd(arg, _ => String.Format(format, arg));
		}
	}

	extension(StringSegment str)
	{
		public bool Any() => str.Length > 0;

		public StringSegment GetSplitSegment(char separator, int index) =>
			str.Subsegment(str.AsSpan().GetSplitRange(separator, index));
	}

	extension(ReadOnlySpan<char> str)
	{
		public Range GetSplitRange(char separator, int index)
		{
			var i = -1;

			foreach (var range in str.Split(separator))
			{
				if (++i == index)
				{
					return range;
				}
			}

			throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range for the number of segments in the string.");
		}

		public ReadOnlySpan<char> GetSplitSpan(char separator, int index) =>
			str[str.GetSplitRange(separator, index)];

		public int GetSplitCount(char separator)
		{
			if (str.IsEmpty)
			{
				return 0;
			}

			var count = 1;
			var index = str.IndexOf(separator);
			var remaining = str;
			while (index != -1)
			{
				count++;
				remaining = remaining[(index + 1)..];
				index = remaining.IndexOf(separator);
			}
			return count;
		}
	}
}