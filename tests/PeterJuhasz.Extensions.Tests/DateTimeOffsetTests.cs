namespace System.Extensions.Tests;

[TestClass]
public class DateTimeOffsetTests
{
	[TestMethod]
	public void DateTimeOffset_FloorToWeek_DayBefore() =>
		Assert.AreEqual(new DateTimeOffset(2020, 06, 22, 00, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 06, 28, 17, 18, 00, TimeSpan.Zero).FloorToWeek());

	[TestMethod]
	public void DateTimeOffset_FloorToWeek_Exactly() =>
		Assert.AreEqual(new DateTimeOffset(2020, 06, 22, 00, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 06, 22, 00, 00, 00, TimeSpan.Zero).FloorToWeek());

	[TestMethod]
	public void DateTimeOffset_FloorToWeek_PreservesOffset() =>
		Assert.AreEqual(new DateTimeOffset(2020, 06, 22, 00, 00, 00, TimeSpan.FromHours(2)), new DateTimeOffset(2020, 06, 24, 17, 18, 00, TimeSpan.FromHours(2)).FloorToWeek());

	[TestMethod]
	public void DateTimeOffset_CeilToWeek() =>
		Assert.AreEqual(new DateTimeOffset(2020, 06, 29, 00, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 06, 28, 17, 18, 00, TimeSpan.Zero).CeilToWeek());

	[TestMethod]
	public void DateTimeOffset_CeilToWeek_Exactly() =>
		Assert.AreEqual(new DateTimeOffset(2020, 06, 22, 00, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 06, 22, 00, 00, 00, TimeSpan.Zero).CeilToWeek());

	[TestMethod]
	public void DateTimeOffset_FloorToQuarter() =>
		Assert.AreEqual(new DateTimeOffset(2020, 10, 01, 00, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 11, 15, 17, 18, 00, TimeSpan.Zero).FloorToQuarter());

	[TestMethod]
	public void DateTimeOffset_CeilToQuarter_LastQuarter() =>
		Assert.AreEqual(new DateTimeOffset(2021, 01, 01, 00, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 12, 15, 17, 18, 00, TimeSpan.Zero).CeilToQuarter());

	[TestMethod]
	public void DateTimeOffset_CeilToMonth_Exactly() =>
		Assert.AreEqual(new DateTimeOffset(2020, 06, 01, 00, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 06, 01, 00, 00, 00, TimeSpan.Zero).CeilToMonth());

	[TestMethod]
	public void DateTimeOffset_Floor() =>
		Assert.AreEqual(new DateTimeOffset(2020, 06, 22, 18, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 06, 22, 18, 18, 00, TimeSpan.Zero).Floor(TimeSpan.FromHours(1)));

	[TestMethod]
	public void DateTimeOffset_Floor_Exactly() =>
		Assert.AreEqual(new DateTimeOffset(2020, 06, 22, 18, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 06, 22, 18, 00, 00, TimeSpan.Zero).Floor(TimeSpan.FromHours(1)));

	[TestMethod]
	public void DateTimeOffset_Floor_PreservesOffset() =>
		Assert.AreEqual(new DateTimeOffset(2020, 06, 22, 18, 00, 00, TimeSpan.FromHours(2)), new DateTimeOffset(2020, 06, 22, 18, 18, 00, TimeSpan.FromHours(2)).Floor(TimeSpan.FromHours(1)));

	[TestMethod]
	public void DateTimeOffset_Round_Down() =>
		Assert.AreEqual(new DateTimeOffset(2020, 06, 22, 18, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 06, 22, 18, 29, 00, TimeSpan.Zero).Round(TimeSpan.FromHours(1)));

	[TestMethod]
	public void DateTimeOffset_Round_Up() =>
		Assert.AreEqual(new DateTimeOffset(2020, 06, 22, 19, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 06, 22, 18, 31, 00, TimeSpan.Zero).Round(TimeSpan.FromHours(1)));

	[TestMethod]
	public void DateTimeOffset_Round_Exactly() =>
		Assert.AreEqual(new DateTimeOffset(2020, 06, 22, 18, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 06, 22, 18, 00, 00, TimeSpan.Zero).Round(TimeSpan.FromHours(1)));

	[TestMethod]
	public void DateTimeOffset_Ceiling() =>
		Assert.AreEqual(new DateTimeOffset(2020, 06, 22, 18, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 06, 22, 17, 18, 00, TimeSpan.Zero).Ceiling(TimeSpan.FromHours(1)));

	[TestMethod]
	public void DateTimeOffset_Ceiling_Exactly() =>
		Assert.AreEqual(new DateTimeOffset(2020, 06, 22, 18, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 06, 22, 18, 00, 00, TimeSpan.Zero).Ceiling(TimeSpan.FromHours(1)));

	[TestMethod]
	public void DateTime_Round_Up_PreservesKind()
	{
		var result = new DateTime(2020, 06, 22, 18, 31, 00, DateTimeKind.Utc).Round(TimeSpan.FromHours(1));
		Assert.AreEqual(new DateTime(2020, 06, 22, 19, 00, 00, DateTimeKind.Utc), result);
		Assert.AreEqual(DateTimeKind.Utc, result.Kind);
	}

	[TestMethod]
	public void DateTime_CeilToYear() =>
		Assert.AreEqual(new DateTime(2021, 01, 01), new DateTime(2020, 01, 01, 00, 00, 01).CeilToYear());
}
