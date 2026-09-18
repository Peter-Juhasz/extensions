namespace System.Extensions.Tests.Collections;

[TestClass]
public class EnumerableTests
{
	[TestMethod]
	public void GroupByUntilChanged()
	{
		var numbers = new[] { 1, 1, 2, 2, 4, 3, 3 };
		var grouped = numbers.GroupByUntilChangedRanges(x => x).ToArray();
		Assert.HasCount(4, grouped);
		Assert.AreEqual(0, grouped[0].GetOffsetAndLength(numbers.Length).Offset);
		Assert.AreEqual(2, grouped[0].GetOffsetAndLength(numbers.Length).Length);
		Assert.AreEqual(2, grouped[1].GetOffsetAndLength(numbers.Length).Offset);
		Assert.AreEqual(2, grouped[1].GetOffsetAndLength(numbers.Length).Length);
		Assert.AreEqual(4, grouped[2].GetOffsetAndLength(numbers.Length).Offset);
		Assert.AreEqual(1, grouped[2].GetOffsetAndLength(numbers.Length).Length);
		Assert.AreEqual(5, grouped[3].GetOffsetAndLength(numbers.Length).Offset);
		Assert.AreEqual(2, grouped[3].GetOffsetAndLength(numbers.Length).Length);
	}

	[TestMethod]
	public void GroupByUntilChanged_Empty()
	{
		var numbers = new int[] { };
		var grouped = numbers.GroupByUntilChangedRanges(x => x).ToArray();
		Assert.IsEmpty(grouped);
	}

	[TestMethod]
	public void GroupByUntilChanged_Single()
	{
		var numbers = new int[] { 1, 1 };
		var grouped = numbers.GroupByUntilChangedRanges(x => x).ToArray();
		Assert.HasCount(1, grouped);
		Assert.AreEqual(0, grouped[0].GetOffsetAndLength(numbers.Length).Offset);
		Assert.AreEqual(2, grouped[0].GetOffsetAndLength(numbers.Length).Length);
	}
}
