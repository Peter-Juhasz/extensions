using System.Text.Json;

namespace System.Extensions.Tests;

[TestClass]
public class ColorTests
{
	[TestMethod]
	public void FromHexString_AndToHexString_RoundTripsRgb()
	{
		var color = Color.FromHexString("#1a2b3c");

		Assert.AreEqual(new Color(0x1A, 0x2B, 0x3C), color);
		Assert.AreEqual("#1A2B3C", color.ToHexString());
	}

	[TestMethod]
	public void FromHexString_AndToHexString_RoundTripsRgba()
	{
		var color = Color.FromHexString("#1a2b3c4d");

		Assert.AreEqual(new Color(0x1A, 0x2B, 0x3C, 0x4D), color);
		Assert.AreEqual("#1A2B3C4D", color.ToHexString());
	}

	[TestMethod]
	public void FromHexString_ParsesWithoutHashPrefix()
	{
		Assert.AreEqual(new Color(0x1A, 0x2B, 0x3C), Color.FromHexString("1A2B3C"));
		Assert.AreEqual(new Color(0x1A, 0x2B, 0x3C, 0x4D), Color.FromHexString("1a2b3c4d"));
	}

	[TestMethod]
	public void FromHexString_ExpandsShortForms()
	{
		Assert.AreEqual(new Color(0x11, 0x22, 0x33), Color.FromHexString("#123"));
		Assert.AreEqual(new Color(0x11, 0x22, 0x33, 0x44), Color.FromHexString("#1234"));
	}

	[TestMethod]
	[DataRow("#1a2b3g")]
	[DataRow("#1a2b3c4z")]
	[DataRow("#12345")]
	public void FromHexString_ThrowsFormatExceptionForInvalidInput(string value)
	{
		Assert.ThrowsExactly<FormatException>(() => Color.FromHexString(value));
	}

	[TestMethod]
	public void ToHexString_FormatsFullyOpaqueAndTransparentColors()
	{
		Assert.AreEqual("#000000", new Color(0, 0, 0).ToHexString());
		Assert.AreEqual("#FFFFFF", new Color(0xFF, 0xFF, 0xFF).ToHexString());
		Assert.AreEqual("#FFFFFF00", new Color(0xFF, 0xFF, 0xFF, 0).ToHexString());
		Assert.AreEqual("#ABCDEFFE", new Color(0xAB, 0xCD, 0xEF, 0xFE).ToHexString());
	}

	[TestMethod]
	public void JsonSerializerExtensions_SerializesColorAsHexString()
	{
		var options = new JsonSerializerOptions();
		options.Converters.Add(new ColorJsonConverter());
		var json = JsonSerializer.Serialize(new Color(0x11, 0x22, 0x33), options);

		Assert.AreEqual("\"#112233\"", json);
	}
}
