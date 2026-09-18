using System.Globalization;

namespace System;

public static class DateExtensions
{
	extension(DateOnly date)
	{
		public DateOnly FloorToYear() => new(date.Year, 1, 1);

		public DateOnly FloorToMonth() => new(date.Year, date.Month, 1);

		public DateOnly FloorToQuarter() => new(date.Year, ((date.Month - 1) / 3) * 3 + 1, 1);

		public DateOnly FloorToHalf() => new(date.Year, ((date.Month - 1) / 6) * 6 + 1, 1);

		public DateOnly FloorToWeek(DayOfWeek firstDayOfWeek = DayOfWeek.Monday) => date.AddDays(-((7 + (int)date.DayOfWeek - (int)firstDayOfWeek) % 7));


		public DateOnly CeilToYear() => date.FloorToYear().AddYears(1);

		public DateOnly CeilToMonth() => date.FloorToMonth().AddMonths(1);

		public DateOnly CeilToQuarter() => date.FloorToQuarter().AddMonths(3);

		public DateOnly CeilToHalf() => date.FloorToHalf().AddMonths(6);

		public DateOnly CeilToWeek(DayOfWeek firstDayOfWeek = DayOfWeek.Monday) => date.FloorToWeek(firstDayOfWeek).AddDays(7);
	}

	extension(DateTime dateTime)
	{
		public DateTime FloorToYear() => StartOfDay(dateTime.ToDateOnly().FloorToYear(), dateTime.Kind);

		public DateTime FloorToMonth() => StartOfDay(dateTime.ToDateOnly().FloorToMonth(), dateTime.Kind);

		public DateTime FloorToQuarter() => StartOfDay(dateTime.ToDateOnly().FloorToQuarter(), dateTime.Kind);

		public DateTime FloorToHalf() => StartOfDay(dateTime.ToDateOnly().FloorToHalf(), dateTime.Kind);

		public DateTime FloorToWeek(DayOfWeek firstDayOfWeek = DayOfWeek.Monday) => StartOfDay(dateTime.ToDateOnly().FloorToWeek(firstDayOfWeek), dateTime.Kind);


		public DateTime CeilToYear() => CeilToDate(dateTime, dateTime.FloorToYear(), dateTime.ToDateOnly().CeilToYear());

		public DateTime CeilToMonth() => CeilToDate(dateTime, dateTime.FloorToMonth(), dateTime.ToDateOnly().CeilToMonth());

		public DateTime CeilToQuarter() => CeilToDate(dateTime, dateTime.FloorToQuarter(), dateTime.ToDateOnly().CeilToQuarter());

		public DateTime CeilToHalf() => CeilToDate(dateTime, dateTime.FloorToHalf(), dateTime.ToDateOnly().CeilToHalf());

		public DateTime CeilToWeek(DayOfWeek firstDayOfWeek = DayOfWeek.Monday) => CeilToDate(dateTime, dateTime.FloorToWeek(firstDayOfWeek), dateTime.ToDateOnly().CeilToWeek(firstDayOfWeek));


		public DateTime Floor(TimeSpan unit) => new(dateTime.Ticks / unit.Ticks * unit.Ticks, dateTime.Kind);

		public DateTime Ceiling(TimeSpan unit) => new((dateTime.Ticks + unit.Ticks - 1) / unit.Ticks * unit.Ticks, dateTime.Kind);

		public DateTime Round(TimeSpan unit) => new((dateTime.Ticks + unit.Ticks / 2) / unit.Ticks * unit.Ticks, dateTime.Kind);
	}

	extension(DateTimeOffset dateTime)
	{
		public DateTimeOffset FloorToYear() => new(dateTime.DateTime.FloorToYear(), dateTime.Offset);

		public DateTimeOffset FloorToMonth() => new(dateTime.DateTime.FloorToMonth(), dateTime.Offset);

		public DateTimeOffset FloorToQuarter() => new(dateTime.DateTime.FloorToQuarter(), dateTime.Offset);

		public DateTimeOffset FloorToHalf() => new(dateTime.DateTime.FloorToHalf(), dateTime.Offset);

		public DateTimeOffset FloorToWeek(DayOfWeek firstDayOfWeek = DayOfWeek.Monday) => new(dateTime.DateTime.FloorToWeek(firstDayOfWeek), dateTime.Offset);


		public DateTimeOffset CeilToYear() => new(dateTime.DateTime.CeilToYear(), dateTime.Offset);

		public DateTimeOffset CeilToMonth() => new(dateTime.DateTime.CeilToMonth(), dateTime.Offset);

		public DateTimeOffset CeilToQuarter() => new(dateTime.DateTime.CeilToQuarter(), dateTime.Offset);

		public DateTimeOffset CeilToHalf() => new(dateTime.DateTime.CeilToHalf(), dateTime.Offset);

		public DateTimeOffset CeilToWeek(DayOfWeek firstDayOfWeek = DayOfWeek.Monday) => new(dateTime.DateTime.CeilToWeek(firstDayOfWeek), dateTime.Offset);


		public DateTimeOffset Floor(TimeSpan unit) => new(dateTime.DateTime.Floor(unit), dateTime.Offset);

		public DateTimeOffset Ceiling(TimeSpan unit) => new(dateTime.DateTime.Ceiling(unit), dateTime.Offset);

		public DateTimeOffset Round(TimeSpan unit) => new(dateTime.DateTime.Round(unit), dateTime.Offset);
	}

	private static DateTime StartOfDay(DateOnly date, DateTimeKind kind) => date.ToDateTime(TimeOnly.MinValue, kind);

	private static DateTime CeilToDate(DateTime value, DateTime floor, DateOnly ceil) => value == floor ? value : StartOfDay(ceil, value.Kind);

	public static int DifferenceInYears(DateOnly date, DateOnly today)
	{
		var age = today.Year - date.Year;
		if (today.Month < date.Month || (today.Month == date.Month && today.Day < date.Day))
		{
			age--;
		}
		return age;
	}

	public static DateTimeOffset Min(DateTimeOffset a, DateTimeOffset b) => a < b ? a : b;

	public static DateTimeOffset? Min(DateTimeOffset? a, DateTimeOffset? b) =>
		a.HasValue && b.HasValue
			? Min(a.Value, b.Value)
			: a ?? b;

	public static DateOnly Min(DateOnly a, DateOnly b) => a < b ? a : b;

	public static DateTimeOffset Max(DateTimeOffset a, DateTimeOffset b) => a > b ? a : b;

	public static DateTimeOffset? Max(DateTimeOffset? a, DateTimeOffset? b) =>
		a.HasValue && b.HasValue
			? Max(a.Value, b.Value)
			: a ?? b;

	public static DateOnly Max(DateOnly a, DateOnly b) => a > b ? a : b;

	public static TimeSpan Max(TimeSpan a, TimeSpan b) => a > b ? a : b;


	extension(DateTimeOffset value)
	{
		public bool IsStale(DateTimeOffset now, TimeSpan staleness) => value < now - staleness;

		public bool IsStale(TimeProvider timeProvider, TimeSpan staleness) => value.IsStale(timeProvider.GetLocalNow(), staleness);
	}

	extension(DateTime dateTime)
	{
		public DateOnly ToDateOnly() => DateOnly.FromDateTime(dateTime);
		public TimeOnly ToTimeOnly() => TimeOnly.FromDateTime(dateTime);
		public MonthOnly ToMonthOnly() => MonthOnly.FromDateTime(dateTime);
	}

	extension(DateOnly dateTime)
	{
		public MonthOnly ToMonthOnly() => MonthOnly.FromDateOnly(dateTime);
	}

	extension(DateTime date)
	{
		public string ToIso8601() => date.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
	}

	extension(DateTimeOffset date)
	{
		public string ToIso8601() => date.UtcDateTime.ToIso8601();

		public string ToRfc3339() => System.Xml.XmlConvert.ToString(date);
	}

	extension(TimeSpan timeSpan)
	{
		public string ToTimeZoneDisplayString() => String.Create(6, timeSpan, (span, ts) =>
		{
			span[0] = ts < TimeSpan.Zero ? '-' : '+';
			var (div, rem) = Math.DivRem(Math.Abs(ts.Hours), 10);
			span[1] = (char)('0' + div);
			span[2] = (char)('0' + rem);
			span[3] = ':';
			(div, rem) = Math.DivRem(Math.Abs(ts.Minutes), 10);
			span[4] = (char)('0' + div);
			span[5] = (char)('0' + rem);
		});
	}

	extension(DateTimeOffset dateTime)
	{
		public DateTimeOffset ToTimeZone(TimeZoneInfo destination) => TimeZoneInfo.ConvertTime(dateTime, destination);


		public DateTimeOffset TrimMilliseconds() => new(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Offset);
	}
}