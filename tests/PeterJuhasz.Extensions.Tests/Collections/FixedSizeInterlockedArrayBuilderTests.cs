using System.Collections.Concurrent;

namespace System.Extensions.Tests.Collections;

[TestClass]
public class FixedSizeInterlockedArrayBuilderTests
{
	[TestMethod]
	public void MultiPage()
	{
		var builder = new FixedSizeInterlockedArrayBuilder<int>(16);
		Assert.AreEqual(16, builder.Capacity);
		builder.Add(1);
		builder.Add(2);
		builder.Add(3);
		builder.Add(4);
		builder.Add(5);
		Assert.AreEqual(5, builder.Count);
		Assert.AreSequenceEqual(new[] { 1, 2, 3, 4, 5 }, builder.ToArray());
	}

	[TestMethod]
	public void Concurrency()
	{
		var builder = new FixedSizeInterlockedArrayBuilder<int>(128);
		var max = 100;
		Parallel.For(0, max, i => builder.Add(i + 1));
		Assert.AreEqual(max, builder.Count);
		var array = builder.ToArray();
		Assert.DoesNotContain(0, array);
		for (var i = 1; i <= max; i++)
		{
			Assert.Contains(i, array);
		}
	}
}