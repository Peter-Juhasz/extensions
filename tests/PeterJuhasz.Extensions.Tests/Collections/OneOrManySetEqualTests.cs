namespace System.Extensions.Tests.Collections;

[TestClass]
public class OneOrManySetEqualTests
{
	[TestMethod]
	public void SetEqual_BothEmpty_ReturnsTrue()
	{
		Assert.IsTrue(OneOrMany<int>.Empty.SetEqual(OneOrMany<int>.Empty));
	}

	[TestMethod]
	public void SetEqual_EmptyVsOne_ReturnsFalse()
	{
		Assert.IsFalse(OneOrMany<int>.Empty.SetEqual(OneOrMany.Create(1)));
		Assert.IsFalse(OneOrMany.Create(1).SetEqual(OneOrMany<int>.Empty));
	}

	[TestMethod]
	public void SetEqual_EmptyVsMany_ReturnsFalse()
	{
		Assert.IsFalse(OneOrMany<int>.Empty.SetEqual(OneOrMany.Create([1, 2])));
	}

	[TestMethod]
	public void SetEqual_OneVsOne_SameValue_ReturnsTrue()
	{
		Assert.IsTrue(OneOrMany.Create(42).SetEqual(OneOrMany.Create(42)));
	}

	[TestMethod]
	public void SetEqual_OneVsOne_DifferentValue_ReturnsFalse()
	{
		// Regression: single-value instances share the Array.Empty<T>() backing array,
		// so the ReferenceEquals short-circuit must not run before the HasOne comparison.
		Assert.IsFalse(OneOrMany.Create(1).SetEqual(OneOrMany.Create(2)));
	}

	[TestMethod]
	public void SetEqual_OneVsOne_DifferentStringValue_ReturnsFalse()
	{
		Assert.IsFalse(OneOrMany.Create("a").SetEqual(OneOrMany.Create("b")));
		Assert.IsTrue(OneOrMany.Create("a").SetEqual(OneOrMany.Create("a")));
	}

	[TestMethod]
	public void SetEqual_OneVsMany_ReturnsFalse()
	{
		Assert.IsFalse(OneOrMany.Create(1).SetEqual(OneOrMany.Create([1, 2])));
	}

	[TestMethod]
	public void SetEqual_ManyVsMany_SameOrder_ReturnsTrue()
	{
		Assert.IsTrue(OneOrMany.Create([1, 2, 3]).SetEqual(OneOrMany.Create([1, 2, 3])));
	}

	[TestMethod]
	public void SetEqual_ManyVsMany_DifferentOrder_ReturnsTrue()
	{
		// Set semantics: order does not matter.
		Assert.IsTrue(OneOrMany.Create([1, 2, 3]).SetEqual(OneOrMany.Create([3, 1, 2])));
	}

	[TestMethod]
	public void SetEqual_ManyVsMany_DifferentElements_ReturnsFalse()
	{
		Assert.IsFalse(OneOrMany.Create([1, 2, 3]).SetEqual(OneOrMany.Create([1, 2, 4])));
	}

	[TestMethod]
	public void SetEqual_ManyVsMany_DifferentCount_ReturnsFalse()
	{
		Assert.IsFalse(OneOrMany.Create([1, 2, 3]).SetEqual(OneOrMany.Create([1, 2])));
	}

	[TestMethod]
	public void SetEqual_ManyVsMany_SameBackingArray_ReturnsTrue()
	{
		var array = new[] { 1, 2, 3 };
		Assert.IsTrue(OneOrMany.Create(array).SetEqual(OneOrMany.Create(array)));
	}

	[TestMethod]
	public void SetEqual_ManyVsMany_Strings_DifferentOrder_ReturnsTrue()
	{
		Assert.IsTrue(OneOrMany.Create(["a", "b"]).SetEqual(OneOrMany.Create(["b", "a"])));
	}

	[TestMethod]
	public void SetEqual_IsReflexive()
	{
		var empty = OneOrMany<int>.Empty;
		var one = OneOrMany.Create(7);
		var many = OneOrMany.Create([1, 2, 3]);

		Assert.IsTrue(empty.SetEqual(empty));
		Assert.IsTrue(one.SetEqual(one));
		Assert.IsTrue(many.SetEqual(many));
	}

	[TestMethod]
	public void SetEqual_IsSymmetric()
	{
		var a = OneOrMany.Create([1, 2, 3]);
		var b = OneOrMany.Create([3, 2, 1]);

		Assert.AreEqual(a.SetEqual(b), b.SetEqual(a));
		Assert.IsTrue(a.SetEqual(b));
	}
}
