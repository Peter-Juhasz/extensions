namespace System.Extensions.Tests;

[TestClass]
public class InlineUriBuilderTests
{
	[TestMethod]
	public void Test()
	{
		var uri = InlineUriBuilder.CreateFromHost("test.example.dev")
			.AppendPath($"api/organizations/{"abc"}/numbers/{2}/do")
			.AppendQueryString("token", "123")
			.AppendFragment("fragment")
			.ToString();
		Assert.AreEqual("https://test.example.dev/api/organizations/abc/numbers/2/do?token=123#fragment", uri);
	}

	[TestMethod]
	public void TestSecretsListWithTypeFilter()
	{
		var type = "OpenAI.ApiKey";
		var uri = InlineUriBuilder.CreateFromPath("api")
			.AppendPath("secrets")
			.AppendQueryString(nameof(type), type)
			.ToString();
		Assert.AreEqual("api/secrets?type=OpenAI.ApiKey", uri);
	}

	[TestMethod]
	public void TestSecretsListWithoutTypeFilter()
	{
		string? type = null;
		var uri = InlineUriBuilder.CreateFromPath("api")
			.AppendPath("secrets")
			.AppendQueryString(nameof(type), type)
			.ToString();
		Assert.AreEqual("api/secrets", uri);
	}
}