// cooperative cancellation cannot interrupt a spinning loop, so a hang regression needs the non-cooperative timeout
#pragma warning disable MSTEST0045

namespace System.Extensions.Tests;

[TestClass]
public class ValueUriTests
{
	[TestMethod]
	public void Test()
	{
		var uri = "https://test.example.dev:443/api/organizations/abc/numbers/2/do?token=123&b=hello#fragment".ToValueUri();
		Assert.AreEqual("https", uri.SchemeSpan.ToString());
		Assert.AreEqual("https", uri.Scheme);
		Assert.AreEqual("test.example.dev", uri.HostSpan.ToString());
		Assert.AreEqual("test.example.dev", uri.Host);
		Assert.AreEqual(443, uri.Port);
		Assert.AreEqual("/api/organizations/abc/numbers/2/do", uri.PathSpan.ToString());
		Assert.AreEqual(6, uri.PathSegmentCount);
		Assert.IsTrue(uri.TryGetPathSegmentSpan(0, out var segment0));
		Assert.AreEqual("api", segment0.ToString());
		Assert.IsTrue(uri.IsPathSegment(0, "api"));
		Assert.IsTrue(uri.TryGetPathSegmentSpan(1, out var segment1));
		Assert.AreEqual("organizations", segment1.ToString());
		Assert.IsTrue(uri.TryGetPathSegmentSpan(2, out var segment2));
		Assert.AreEqual("abc", segment2.ToString());
		Assert.IsTrue(uri.TryGetPathSegmentSpan(3, out var segment3));
		Assert.AreEqual("numbers", segment3.ToString());
		Assert.IsTrue(uri.TryGetPathSegmentSpan(4, out var segment4));
		Assert.AreEqual("2", segment4.ToString());
		Assert.IsTrue(uri.TryGetPathSegmentSpan(5, out var segment5));
		Assert.IsFalse(uri.TryGetPathSegmentSpan(6, out _));
		Assert.AreEqual("do", segment5.ToString());
		Assert.AreEqual("123", uri.GetQueryValue("token"));
		Assert.AreEqual(123, uri.GetQueryValue<int>("token"));
		Assert.AreEqual("hello", uri.GetQueryValue("b"));
		Assert.AreEqual("?token=123&b=hello", uri.QuerySpan.ToString());
		Assert.AreEqual("#fragment", uri.FragmentSpan.ToString());
	}

	[TestMethod]
	[Timeout(1000)]
	public void TryGetQueryValue_NameIsPrefixOfLaterParameter_ReturnsFalse()
	{
		var uri = "https://www.facebook.com/photo.php?fbid=1&idx=2".ToValueUri();
		Assert.IsFalse(uri.TryGetQueryValue("id", out _));
	}

	[TestMethod]
	[Timeout(1000)]
	public void TryGetQueryValue_PrefixedParametersBeforeMatch_ReturnsMatchingValue()
	{
		var uri = "https://www.facebook.com/photo.php?fbid=1&idx=2&id=3".ToValueUri();
		Assert.IsTrue(uri.TryGetQueryValue("id", out var value));
		Assert.AreEqual("3", value);
	}

	[TestMethod]
	[Timeout(1000)]
	public void TryGetQueryValue_SkipsPrefixedParameter_ReturnsDecodedValue()
	{
		var uri = "https://l.facebook.com/l.php?utm_source=x&u=https%3A%2F%2Fexample.com".ToValueUri();
		Assert.IsTrue(uri.TryGetQueryValue("u", out var value));
		Assert.AreEqual("https://example.com", value);
	}

	[TestMethod]
	[Timeout(1000)]
	public void TryGetQueryValue_OverlappingOccurrence_ReturnsFalse()
	{
		var uri = "https://example.com/?aaa=1".ToValueUri();
		Assert.IsFalse(uri.TryGetQueryValue("aa", out _));
	}

	[TestMethod]
	[Timeout(1000)]
	public void TryGetQueryValue_EmptyName_ReturnsFalse()
	{
		var uri = "https://example.com/?a=1".ToValueUri();
		Assert.IsFalse(uri.TryGetQueryValue("", out _));
	}

	[TestMethod]
	[Timeout(1000)]
	public void TryGetQueryValue_CaseDiffersAfterSkippedOccurrence_ReturnsValue()
	{
		var uri = "https://example.com/?aid=1&ID=2".ToValueUri();
		Assert.IsTrue(uri.TryGetQueryValue("id", out var value));
		Assert.AreEqual("2", value);
	}

	[TestMethod]
	[Timeout(1000)]
	public void ContainsAnyQuery_NamesArePrefixesOfParameter_ReturnsFalse()
	{
		var uri = "https://www.youtube.com/watch?time_continue=5".ToValueUri();
		Assert.IsFalse(uri.ContainsAnyQuery("t", "si"));
	}

	[TestMethod]
	[Timeout(1000)]
	public void GetFragmentValueSpan_NameIsPrefixOfLaterParameter_ReturnsEmpty()
	{
		var uri = "https://example.com/#fbid=1&idx=2".ToValueUri();
		Assert.IsTrue(uri.GetFragmentValueSpan("id").IsEmpty);
	}

	[TestMethod]
	[Timeout(1000)]
	public void GetFragmentValueSpan_PrefixedParametersBeforeMatch_ReturnsMatchingValue()
	{
		var uri = "https://example.com/#fbid=1&idx=2&id=3".ToValueUri();
		Assert.AreEqual("3", uri.GetFragmentValueSpan("id").ToString());
	}

	[TestMethod]
	[Timeout(1000)]
	public void TryGetFragmentQueryValue_QueryAndFragmentShareName_ReturnsFragmentValue()
	{
		var uri = "https://example.com/?a=query#a=fragment".ToValueUri();
		Assert.IsTrue(uri.TryGetFragmentQueryValue("a", out var value));
		Assert.AreEqual("fragment", value);
	}
}