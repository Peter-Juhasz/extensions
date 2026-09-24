namespace PeterJuhasz.Extensions.Tests;

[TestClass]
public class MonthOnlyTests
{
	[TestMethod]
	public void Constructor_ValidValues_SetsProperties()
	{
		var m = new MonthOnly(2024, 6);
		Assert.AreEqual(2024, m.Year);
		Assert.AreEqual(6, m.Month);
	}

	[TestMethod]
	[DataRow(-1, 1)]
	[DataRow(2024, 0)]
	[DataRow(2024, 13)]
	public void Constructor_InvalidValues_Throws(int year, int month)
	{
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new MonthOnly(year, month));
	}

	[TestMethod]
	public void Parse_ValidString_ReturnsMonthOnly()
	{
		var m = MonthOnly.Parse("2024-06");
		Assert.AreEqual(2024, m.Year);
		Assert.AreEqual(6, m.Month);
	}

	[TestMethod]
	[DataRow("")]
	[DataRow("202406")]
	[DataRow("2024-00")]
	[DataRow("2024-13")]
	[DataRow("abcd-ef")]
	public void Parse_InvalidString_Throws(string input)
	{
		Assert.ThrowsExactly<FormatException>(() => MonthOnly.Parse(input));
	}

	[TestMethod]
	public void TryParse_ValidString_ReturnsTrue()
	{
		Assert.IsTrue(MonthOnly.TryParse("2024-06", null, out var m));
		Assert.AreEqual(2024, m.Year);
		Assert.AreEqual(6, m.Month);
	}

	[TestMethod]
	[DataRow("")]
	[DataRow("202406")]
	[DataRow("2024-00")]
	[DataRow("2024-13")]
	[DataRow("abcd-ef")]
	public void TryParse_InvalidString_ReturnsFalse(string input)
	{
		Assert.IsFalse(MonthOnly.TryParse(input, null, out var _));
	}

	[TestMethod]
	public void ToString_ReturnsExpectedFormat()
	{
		var m = new MonthOnly(2024, 6);
		Assert.AreEqual("2024-06", m.ToString());
	}

	[TestMethod]
	public void ComparisonOperators_WorkCorrectly()
	{
		var a = new MonthOnly(2024, 5);
		var b = new MonthOnly(2024, 6);
		Assert.IsTrue(a < b);
		Assert.IsTrue(b > a);
		Assert.IsTrue(a <= b);
		Assert.IsTrue(b >= a);
		Assert.IsTrue(a != b);
		Assert.IsTrue(b == new MonthOnly(2024, 6));
	}

	[TestMethod]
	public void AddMonths_AddYears_WorkCorrectly()
	{
		var m = new MonthOnly(2024, 6);
		Assert.AreEqual(new MonthOnly(2024, 7), m.AddMonths(1));
		Assert.AreEqual(new MonthOnly(2025, 6), m.AddYears(1));
	}

	[TestMethod]
	public void ExtensionMethods_WorkCorrectly()
	{
		var m = new MonthOnly(2024, 6);
		Assert.AreEqual("2024", m.ToYearString());
		Assert.AreEqual("06", m.ToMonthString());
		Assert.AreEqual(30, m.DaysInMonth()); // June has 30 days
	}
}
