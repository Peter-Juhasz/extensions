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
	public void JsonSerializerExtensions_SerializesColorAsHexString()
	{
		var options = new JsonSerializerOptions();
		options.Converters.Add(new ColorJsonConverter());
		var json = JsonSerializer.Serialize(new Color(0x11, 0x22, 0x33), options);

		Assert.AreEqual("\"#112233\"", json);
	}
}
