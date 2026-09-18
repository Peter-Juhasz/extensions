using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace System;

[JsonConverter(typeof(MonthOnlyJsonConverter))]
public readonly struct MonthOnly :
	IEquatable<MonthOnly>,
	IComparable<MonthOnly>,
	IParsable<MonthOnly>,
	ISpanParsable<MonthOnly>,
	IFormattable,
	ISpanFormattable
{
	public MonthOnly(int year, int month)
		: this(year * 12 + (month - 1))
	{
		ArgumentOutOfRangeException.ThrowIfNegative(year);
		ArgumentOutOfRangeException.ThrowIfLessThan(month, 1);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(month, 12);
	}
	private MonthOnly(int value)
	{
		Value = value;
	}

	private readonly int Value;

	public int Year => Value / 12;
	public int Month => (Value % 12) + 1;

	public void Deconstruct(out int year, out int month)
	{
		(year, month) = Math.DivRem(Value, 12);
		month++;
	}

	public static MonthOnly FromDateTime(DateTime dateTime) => new(dateTime.Year, dateTime.Month);
	public static MonthOnly FromDateOnly(DateOnly dateTime) => new(dateTime.Year, dateTime.Month);

	public static MonthOnly ThisMonth => FromDateTime(DateTime.Now);


	public readonly MonthOnly AddMonths(int months) => new(Value + months);

	public readonly MonthOnly AddYears(int years) => AddMonths(years * 12);


	public static MonthOnly Parse(string value, IFormatProvider? formatProvider = null) => Parse(value.AsSpan(), formatProvider);

	public static MonthOnly Parse(ReadOnlySpan<char> value, IFormatProvider? formatProvider = null)
	{
		if (!TryParse(value, formatProvider, out MonthOnly monthOnly))
		{
			throw new FormatException($"Invalid MonthOnly format: '{value}'");
		}

		return monthOnly;
	}

	public static bool TryParse(string? value, IFormatProvider? formatProvider, out MonthOnly monthOnly)
	{
		ArgumentNullException.ThrowIfNull(value);
		return TryParse(value.AsSpan(), formatProvider, out monthOnly);
	}

	public static bool TryParse(ReadOnlySpan<char> value, IFormatProvider? formatProvider, out MonthOnly monthOnly)
	{
		if (value.Length is not (6 or 7) || value[4] != '-')
		{
			monthOnly = default;
			return false;
		}

		if (!int.TryParse(value[..4], out int year) ||
			!int.TryParse(value[5..], out int month))
		{
			monthOnly = default;
			return false;
		}

		if (month < 1 || month > 12)
		{
			monthOnly = default;
			return false;
		}

		monthOnly = new(year, month);
		return true;
	}


	public readonly int CompareTo(MonthOnly other)
	{
		int yearComparison = Year.CompareTo(other.Year);
		if (yearComparison != 0)
			return yearComparison;

		return Month.CompareTo(other.Month);
	}


	public readonly DateTime ToDateTime() => new(Year, Month, 1);


	public static bool operator <(MonthOnly left, MonthOnly right) => left.CompareTo(right) < 0;

	public static bool operator >(MonthOnly left, MonthOnly right) => left.CompareTo(right) > 0;

	public static bool operator <=(MonthOnly left, MonthOnly right) => left.CompareTo(right) <= 0;

	public static bool operator >=(MonthOnly left, MonthOnly right) => left.CompareTo(right) >= 0;

	public static bool operator ==(MonthOnly left, MonthOnly right) => left.Value == right.Value;

	public static bool operator !=(MonthOnly left, MonthOnly right) => left.Value != right.Value;

	public readonly override int GetHashCode() => Value.GetHashCode();

	public readonly override bool Equals(object? obj) => obj is MonthOnly other && this == other;

	public readonly bool Equals(MonthOnly other) => this == other;


	public readonly string ToString(string? format, IFormatProvider? formatProvider) => ToString();

	private static CacheItem? ThisMonthToString;

	public readonly override string ToString()
	{
		if (this == ThisMonth)
		{
			if (ThisMonthToString == null)
			{
				ThisMonthToString = new(this, ToStringCore());
			}

			var tmts = ThisMonthToString;
			if (tmts.Value == this)
			{
				return tmts.String;
			}
		}

		return ToStringCore();
	}

	private string ToStringCore() => String.Create<MonthOnly>(7, this, (destination, instance) =>
	{
		instance.TryFormat(destination, out _, default, default);
	});

	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
	{
		if (destination.Length < 7)
		{
			charsWritten = 0;
			return false;
		}

		var (year, month) = this;
		year.TryFormat(destination, out _, "0000", provider);
		destination[4] = '-';
		month.TryFormat(destination[5..], out _, "00", provider);
		charsWritten = 7;
		return true;
	}

	private sealed record class CacheItem(MonthOnly Value, string String);
}

public sealed class MonthOnlyJsonConverter : JsonConverter<MonthOnly>
{
	public override MonthOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var str = reader.GetString() ?? throw new JsonException("Expected non-null string for MonthOnly.");
		return MonthOnly.Parse(str);
	}

	public override void Write(Utf8JsonWriter writer, MonthOnly value, JsonSerializerOptions options)
	{
		Span<char> destination = stackalloc char[7];
		value.TryFormat(destination, out _, default, default);
		writer.WriteStringValue(destination);
	}

	public override MonthOnly ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => Read(ref reader, typeToConvert, options);

	public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] MonthOnly value, JsonSerializerOptions options) => Write(writer, value, options);
}

public static partial class Extensions
{
	extension(MonthOnly monthOnly)
	{
		public string ToYearString() => monthOnly.Year switch
		{
			2025 => "2025",
			2026 => "2026",
			2027 => "2027",
			_ => monthOnly.Year.ToStringInvariantCached(),
		};

		public string ToMonthString()
		{
			if (monthOnly.Month <= 12)
			{
				return Below13IntD2Strings[monthOnly.Month];
			}

			return monthOnly.Month.ToStringInvariant();
		}

		public int DaysInMonth()
		{
			var (year, month) = monthOnly;
			return DateTime.DaysInMonth(year, month);
		}
	}

	private static readonly string[] Below13IntD2Strings =
	[
		"00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12"
	];

	extension(DateOnly monthOnly)
	{
		public string ToYearString() => monthOnly.Year switch
		{
			2025 => "2025",
			2026 => "2026",
			2027 => "2027",
			_ => monthOnly.Year.ToStringInvariantCached(),
		};

		public string ToMonthString() => Below13IntD2Strings[monthOnly.Month];
	}

	extension(DateOnly dateOnly)
	{
		public string ToDayString()
		{
			if (dateOnly.Day <= 12)
			{
				return Below13IntD2Strings[dateOnly.Day];
			}

			return dateOnly.Day.ToStringInvariant();
		}
	}
}
