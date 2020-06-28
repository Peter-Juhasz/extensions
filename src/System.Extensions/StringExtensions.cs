using Microsoft.Extensions.Primitives;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System
{
    public static class StringExtensions
    {
        public static bool IsEmpty(this string str) => str.Length == 0;

        public static bool IsNullOrEmpty(this string? str) => String.IsNullOrEmpty(str);

        public static bool IsNullOrWhiteSpace(this string? str) => String.IsNullOrWhiteSpace(str);

        public static bool IsWhiteSpace(this string str) => String.IsNullOrWhiteSpace(str);

        public static string? TreatEmptyAsNull(this string str) => str.Length == 0 ? null : str;

        public static string? TreatWhiteSpaceAsNull(this string str) => str.IsWhiteSpace() ? null : str;

        public static StringSegment Subsegment(this string str, int start, int length) => new StringSegment(str, start, length);

        public static StringSegment Subsegment(this string str, Range range)
        {
            var (offset, length) = range.GetOffsetAndLength(str.Length);
            return new StringSegment(str, offset, length);
        }

        public static bool StartsWith(this StringSegment str, char subject) => str.Length >= 1 && str[0] == subject;

        public static bool EndsWith(this StringSegment str, char subject) => str.Length >= 1 && str[^0] == subject;

        public static string Ellipsis(this string value, int maxLength, in string ellipsis = "...") => value.Length <= maxLength ? value : value.Substring(0, maxLength - ellipsis.Length) + ellipsis;

        public static string Remove(this string value, string subject) => value.Replace(subject, String.Empty);

        public static string Remove(this string value, string subject, StringComparison comparison) => value.Replace(subject, String.Empty, comparison);

        public static string TrimSuffix(this string value, string suffix) =>
            value.EndsWith(suffix) ? value[..^suffix.Length] : value;

        public static string TrimPrefix(this string value, string prefix) =>
            value.StartsWith(prefix) ? value[prefix.Length..] : value;

        public static StringSegment TrimStart(this StringSegment segment, char ch)
        {
            if (segment.Length == 0)
            {
                return segment;
            }

            int start = 0;
            while (segment[start] == ch && start <= segment.Length)
                start++;
            if (start == 0)
                return segment;
            return segment.Subsegment(start);
        }

        public static StringSegment TrimStart(this StringSegment segment, params char[] ch)
        {
            if (segment.Length == 0)
            {
                return segment;
            }

            int start = 0;
            while (ch.Contains(segment[start]) && start <= segment.Length)
                start++;
            if (start == 0)
                return segment;
            return segment.Subsegment(start);
        }

        public static StringSegment Trim(this StringSegment segment, char ch)
        {
            var start = 0;
            while (start < segment.Length && segment[start] == ch)
                start++;

            var end = segment.Length;
            while (end > 0 && segment[end - 1] == ch)
                end--;

            return segment.Subsegment(start, end - start);
        }

        public static StringSegment TrimEnd(this StringSegment segment, char ch)
        {
            var end = segment.Length;
            while (end > 0 && segment[end - 1] == ch)
                end--;

            return segment.Subsegment(0, end);
        }

        public static StringSegment TrimEnd(this StringSegment segment, params char[] ch)
        {
            var end = segment.Length;
            while (end > 0 && ch.Contains(segment[end - 1]))
                end--;

            return segment.Subsegment(0, end);
        }

        public static int LastIndexOfAny(this StringSegment segment, int startIndex, params char[] ch)
        {
            for (int i = startIndex; i >= 0; i--)
            {
                if (ch.Contains(segment[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        public static int LastIndexOfAny(this StringSegment segment, params char[] ch) => segment.LastIndexOfAny(segment.Length - 1, ch);

        public static Uri ToUri(this string str, UriKind kind) => new Uri(str, kind);

        public static StringTokenizer Split(this StringSegment segment, char ch) => segment.Split(new[] { ch });

        public static IEnumerable<char> AsEnumerable(this StringSegment segment)
        {
            if (segment.Length == 0)
            {
                yield break;
            }

            for (int i = 0; i < segment.Length; i++)
            {
                yield return segment[i];
            }
        }
    }
}
