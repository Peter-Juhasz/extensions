using System.Buffers;
using System.Globalization;

namespace System;

public readonly ref struct IndexOfCharEnumerable
{
	public IndexOfCharEnumerable(ReadOnlySpan<char> span, char ch)
	{
		this.span = span;
		this.ch = ch;
	}

	private readonly ReadOnlySpan<char> span;
	private readonly char ch;

	public Enumerator GetEnumerator() => new(span, ch);

	public ref struct Enumerator
	{
		public Enumerator(ReadOnlySpan<char> span, char ch)
		{
			this.span = span;
			this.ch = ch;
			Reset();
		}

		private readonly ReadOnlySpan<char> span;
		private readonly char ch;

		private int _currentIndex;

		public int Current => _currentIndex;

		public void Reset() => _currentIndex = -1;

		public bool MoveNext()
		{
			var startIndex = _currentIndex;
			var newRelativeIndex = span[(_currentIndex + 1)..].IndexOf(ch);
			if (newRelativeIndex == -1)
			{
				return false;
			}

			_currentIndex = startIndex + 1 + newRelativeIndex;
			return true;
		}
	}
}

public readonly ref struct IndexOfCharSpanEnumerable
{
	public IndexOfCharSpanEnumerable(ReadOnlySpan<char> span, ReadOnlySpan<char> ch, StringComparison comparison)
	{
		this.span = span;
		this.ch = ch;
		this.comparison = comparison;
	}

	private readonly ReadOnlySpan<char> span;
	private readonly ReadOnlySpan<char> ch;
	private readonly StringComparison comparison;

	public Enumerator GetEnumerator() => new(span, ch, comparison);

	public ref struct Enumerator
	{
		public Enumerator(ReadOnlySpan<char> span, ReadOnlySpan<char> ch, StringComparison comparison)
		{
			this.span = span;
			this.ch = ch;
			this.comparison = comparison;
			Reset();
		}

		private readonly ReadOnlySpan<char> span;
		private readonly ReadOnlySpan<char> ch;
		private readonly StringComparison comparison;
		private int _currentIndex;

		public int Current => _currentIndex;

		public void Reset() => _currentIndex = -1;

		public bool MoveNext()
		{
			var startIndex = _currentIndex;
			var newRelativeIndex = span[(_currentIndex + 1)..].IndexOf(ch, comparison);
			if (newRelativeIndex == -1)
			{
				return false;
			}

			_currentIndex = startIndex + 1 + newRelativeIndex;
			return true;
		}
	}
}

public readonly ref struct IndexOfSearchValuesEnumerable
{
	public IndexOfSearchValuesEnumerable(ReadOnlySpan<char> span, SearchValues<string> ch)
	{
		this.span = span;
		this.ch = ch;
	}

	private readonly ReadOnlySpan<char> span;
	private readonly SearchValues<string> ch;

	public Enumerator GetEnumerator() => new(span, ch);

	public ref struct Enumerator
	{
		public Enumerator(ReadOnlySpan<char> span, SearchValues<string> ch)
		{
			this.span = span;
			this.ch = ch;
			Reset();
		}

		private readonly ReadOnlySpan<char> span;
		private readonly SearchValues<string> ch;
		private int _currentIndex;

		public readonly int Current => _currentIndex;

		public void Reset() => _currentIndex = -1;

		public bool MoveNext()
		{
			var startIndex = _currentIndex;
			var newRelativeIndex = span[(_currentIndex + 1)..].IndexOfAny(ch);
			if (newRelativeIndex == -1)
			{
				return false;
			}

			_currentIndex = startIndex + 1 + newRelativeIndex;
			return true;
		}
	}
}

public readonly ref struct IndexOfCharSpanCompareOptionsEnumerable
{
	public IndexOfCharSpanCompareOptionsEnumerable(ReadOnlySpan<char> span, ReadOnlySpan<char> ch, CompareInfo compareInfo, CompareOptions compareOptions)
	{
		this.span = span;
		this.ch = ch;
		this.compareInfo = compareInfo;
		this.compareOptions = compareOptions;
	}

	private readonly ReadOnlySpan<char> span;
	private readonly ReadOnlySpan<char> ch;
	private readonly CompareInfo compareInfo;
	private readonly CompareOptions compareOptions;

	public Enumerator GetEnumerator() => new(span, ch, compareInfo, compareOptions);

	public ref struct Enumerator
	{
		public Enumerator(ReadOnlySpan<char> span, ReadOnlySpan<char> ch, CompareInfo compareInfo, CompareOptions compareOptions)
		{
			this.span = span;
			this.ch = ch;
			this.compareInfo = compareInfo;
			this.compareOptions = compareOptions;
			Reset();
		}

		private readonly ReadOnlySpan<char> span;
		private readonly ReadOnlySpan<char> ch;
		private readonly CompareInfo compareInfo;
		private readonly CompareOptions compareOptions;
		private int _currentIndex;

		public int Current => _currentIndex;

		public void Reset() => _currentIndex = -1;

		public bool MoveNext()
		{
			var startIndex = _currentIndex;
			var newRelativeIndex = compareInfo.IndexOf(span[(_currentIndex + 1)..], ch, compareOptions);
			if (newRelativeIndex == -1)
			{
				return false;
			}

			_currentIndex = startIndex + 1 + newRelativeIndex;
			return true;
		}
	}
}