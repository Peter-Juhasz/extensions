namespace System.Extensions.Tests.Collections;

[TestClass]
public class SmallDictionaryTests
{
	[TestMethod]
	public void KeysAndValues_AfterResize_ContainOnlyAddedItems()
	{
		var dictionary = new SmallDictionary<string, int>(capacity: 1);
		dictionary.Add("a", 1);
		dictionary.Add("b", 2);
		dictionary.Add("c", 3);

		Assert.AreSequenceEqual(["a", "b", "c"], dictionary.Keys);
		Assert.AreSequenceEqual([1, 2, 3], dictionary.Values);
	}

	[TestMethod]
	public void KeysAndValues_AfterRemove_ContainOnlyRemainingItems()
	{
		var dictionary = new SmallDictionary<string, int>(capacity: 2);
		dictionary.Add("a", 1);
		dictionary.Add("b", 2);

		dictionary.Remove("a");

		Assert.AreSequenceEqual(["b"], dictionary.Keys);
		Assert.AreSequenceEqual([2], dictionary.Values);
	}

	[TestMethod]
	public void KeysAndValues_WithUnusedCapacity_ContainOnlyAddedItems()
	{
		var dictionary = new SmallDictionary<string, int>(capacity: 4);
		dictionary.Add("a", 1);

		Assert.AreSequenceEqual(["a"], dictionary.Keys);
		Assert.AreSequenceEqual([1], dictionary.Values);
	}

	[TestMethod]
	public void KeysAndValues_WhenEmpty_AreEmpty()
	{
		var dictionary = new SmallDictionary<string, int>();

		Assert.IsEmpty(dictionary.Keys);
		Assert.IsEmpty(dictionary.Values);
	}

	[TestMethod]
	public void ReadOnlyKeysAndValues_WithUnusedCapacity_ContainOnlyAddedItems()
	{
		IReadOnlyDictionary<string, int> dictionary = new SmallDictionary<string, int>(capacity: 4) { ["a"] = 1 };

		Assert.AreSequenceEqual(["a"], dictionary.Keys);
		Assert.AreSequenceEqual([1], dictionary.Values);
	}
}
