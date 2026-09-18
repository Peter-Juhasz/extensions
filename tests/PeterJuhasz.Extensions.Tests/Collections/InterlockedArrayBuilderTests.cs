using System.Collections.Concurrent;

namespace System.Extensions.Tests.Collections;

[TestClass]
public class MultiPageInterlockedArrayBuilderTests
{
	[TestMethod]
	public void SinglePage()
	{
		var builder = new MultiPageInterlockedArrayBuilder<int>();
		Assert.AreEqual(0, builder.Capacity);
		builder.Add(1);
		builder.Add(2);
		builder.Add(3);
		Assert.AreEqual(3, builder.Count);
		Assert.AreSequenceEqual(new[] { 1, 2, 3 }, builder.ToArray());
	}

	[TestMethod]
	public void MultiPage()
	{
		var builder = new MultiPageInterlockedArrayBuilder<int>(pageSize: 2);
		builder.Add(1);
		builder.Add(2);
		builder.Add(3);
		builder.Add(4);
		builder.Add(5);
		Assert.AreEqual(5, builder.Count);
		Assert.AreEqual(6, builder.Capacity);
		Assert.AreSequenceEqual(new[] { 1, 2, 3, 4, 5 }, builder.ToArray());
	}

	[TestMethod]
	public void Concurrency()
	{
		var builder = new MultiPageInterlockedArrayBuilder<int>();
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
