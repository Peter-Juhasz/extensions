using System.Collections;

namespace System.Extensions.Tests.Collections;

[TestClass]
public class RangeMarkerMapTests
{
	[TestMethod]
	public void NoOverlap_Ordered()
	{
		var map = new RangeMarkerMap(100, 2);
		map.Add(new Range(Index.FromStart(0), Index.FromStart(10)));
		Assert.AreEqual(1, map.Count);
		Assert.AreEqual(10, map.MarkedLength);
		Assert.IsTrue(map[0] is { Start.Value: 0, End.Value: 10 });

		map.Add(new Range(Index.FromStart(25), Index.FromStart(37)));
		Assert.AreEqual(2, map.Count);
		Assert.AreEqual(22, map.MarkedLength);
		Assert.IsTrue(map[0] is { Start.Value: 0, End.Value: 10 });
		Assert.IsTrue(map[1] is { Start.Value: 25, End.Value: 37 });

		map.Add(new Range(Index.FromStart(40), Index.FromStart(43)));
		Assert.AreEqual(3, map.Count);
		Assert.AreEqual(25, map.MarkedLength);
		Assert.IsTrue(map[0] is { Start.Value: 0, End.Value: 10 });
		Assert.IsTrue(map[1] is { Start.Value: 25, End.Value: 37 });
		Assert.IsTrue(map[2] is { Start.Value: 40, End.Value: 43 });
	}

	[TestMethod]
	public void NoOverlap_Unordered()
	{
		var map = new RangeMarkerMap(100, 2);
		map.Add(new Range(Index.FromStart(25), Index.FromStart(37)));
		Assert.AreEqual(1, map.Count);
		Assert.AreEqual(12, map.MarkedLength);
		Assert.IsTrue(map[0] is { Start.Value: 25, End.Value: 37 });

		map.Add(new Range(Index.FromStart(40), Index.FromStart(43)));
		Assert.AreEqual(2, map.Count);
		Assert.AreEqual(15, map.MarkedLength);
		Assert.IsTrue(map[0] is { Start.Value: 25, End.Value: 37 });
		Assert.IsTrue(map[1] is { Start.Value: 40, End.Value: 43 });

		map.Add(new Range(Index.FromStart(0), Index.FromStart(10)));
		Assert.AreEqual(3, map.Count);
		Assert.AreEqual(25, map.MarkedLength);
		Assert.IsTrue(map[0] is { Start.Value: 0, End.Value: 10 });
		Assert.IsTrue(map[1] is { Start.Value: 25, End.Value: 37 });
		Assert.IsTrue(map[2] is { Start.Value: 40, End.Value: 43 });
	}

	[TestMethod]
	public void Overlap()
	{
		var map = new RangeMarkerMap(100, 2);
		map.Add(new Range(Index.FromStart(0), Index.FromStart(10)));
		Assert.AreEqual(1, map.Count);
		Assert.AreEqual(10, map.MarkedLength);
		Assert.IsTrue(map[0] is { Start.Value: 0, End.Value: 10 });

		map.Add(new Range(Index.FromStart(5), Index.FromStart(13)));
		Assert.AreEqual(1, map.Count);
		Assert.AreEqual(13, map.MarkedLength);
		Assert.IsTrue(map[0] is { Start.Value: 0, End.Value: 13 });
	}

	[TestMethod]
	public void OverlapsWithNext()
	{
		var map = new RangeMarkerMap(100, 2);
		map.Add(new Range(Index.FromStart(0), Index.FromStart(10)));
		Assert.AreEqual(1, map.Count);
		Assert.AreEqual(10, map.MarkedLength);
		Assert.IsTrue(map[0] is { Start.Value: 0, End.Value: 10 });

		map.Add(new Range(Index.FromStart(30), Index.FromStart(40)));
		Assert.AreEqual(2, map.Count);
		Assert.AreEqual(20, map.MarkedLength);
		Assert.IsTrue(map[0] is { Start.Value: 0, End.Value: 10 });
		Assert.IsTrue(map[1] is { Start.Value: 30, End.Value: 40 });

		map.Add(new Range(Index.FromStart(25), Index.FromStart(35)));
		Assert.AreEqual(2, map.Count);
		Assert.AreEqual(25, map.MarkedLength);
		Assert.IsTrue(map[0] is { Start.Value: 0, End.Value: 10 });
		Assert.IsTrue(map[1] is { Start.Value: 25, End.Value: 40 });
	}

	[TestMethod]
	public void OverlapsWithNextConsecutive()
	{
		var map = new RangeMarkerMap(100, 2);
		map.Add(new Range(Index.FromStart(0), Index.FromStart(10)));
		Assert.AreEqual(1, map.Count);
		Assert.AreEqual(10, map.MarkedLength);
		Assert.IsTrue(map[0] is { Start.Value: 0, End.Value: 10 });

		map.Add(new Range(Index.FromStart(10), Index.FromStart(20)));
		Assert.AreEqual(1, map.Count);
		Assert.AreEqual(20, map.MarkedLength);
		Assert.IsTrue(map[0] is { Start.Value: 0, End.Value: 20 });

		map.Add(new Range(Index.FromStart(20), Index.FromStart(30)));
		Assert.AreEqual(1, map.Count);
		Assert.AreEqual(30, map.MarkedLength);
		Assert.IsTrue(map[0] is { Start.Value: 0, End.Value: 30 });
	}

	[TestMethod]
	public void OverlapsConsecutiveCollapses()
	{
		var map = new RangeMarkerMap(100, 2);
		map.Add(new Range(Index.FromStart(0), Index.FromStart(10)));
		Assert.AreEqual(1, map.Count);
		Assert.AreEqual(10, map.MarkedLength);
		Assert.IsTrue(map[0] is { Start.Value: 0, End.Value: 10 });

		map.Add(new Range(Index.FromStart(20), Index.FromStart(30)));
		Assert.AreEqual(2, map.Count);
		Assert.AreEqual(20, map.MarkedLength);
		Assert.IsTrue(map[0] is { Start.Value: 0, End.Value: 10 });
		Assert.IsTrue(map[1] is { Start.Value: 20, End.Value: 30 });

		map.Add(new Range(Index.FromStart(10), Index.FromStart(20)));
		Assert.AreEqual(1, map.Count);
		Assert.AreEqual(30, map.MarkedLength);
		Assert.IsTrue(map[0] is { Start.Value: 0, End.Value: 30 });
	}

	[TestMethod]
	public void OverlapsWithNextMany()
	{
		var map = new RangeMarkerMap(100, 2);
		map.Add(new Range(Index.FromStart(0), Index.FromStart(10)));
		Assert.AreEqual(1, map.Count);
		Assert.AreEqual(10, map.MarkedLength);
		Assert.IsTrue(map[0] is { Start.Value: 0, End.Value: 10 });

		map.Add(new Range(Index.FromStart(30), Index.FromStart(35)));
		Assert.AreEqual(2, map.Count);
		Assert.AreEqual(15, map.MarkedLength);
		Assert.IsTrue(map[0] is { Start.Value: 0, End.Value: 10 });
		Assert.IsTrue(map[1] is { Start.Value: 30, End.Value: 35 });

		map.Add(new Range(Index.FromStart(40), Index.FromStart(50)));
		Assert.AreEqual(3, map.Count);
		Assert.AreEqual(25, map.MarkedLength);
		Assert.IsTrue(map[0] is { Start.Value: 0, End.Value: 10 });
		Assert.IsTrue(map[1] is { Start.Value: 30, End.Value: 35 });
		Assert.IsTrue(map[2] is { Start.Value: 40, End.Value: 50 });

		map.Add(new Range(Index.FromStart(15), Index.FromStart(60)));
		Assert.AreEqual(2, map.Count);
		Assert.AreEqual(55, map.MarkedLength);
		Assert.IsTrue(map[0] is { Start.Value: 0, End.Value: 10 });
		Assert.IsTrue(map[1] is { Start.Value: 15, End.Value: 60 });
	}
}