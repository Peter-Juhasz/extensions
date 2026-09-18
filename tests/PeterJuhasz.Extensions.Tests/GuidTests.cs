namespace System.Extensions.Tests;

[TestClass]
public class GuidTests
{
	[TestMethod]
	public void Guid_TreatEmptyAsNull_Empty() =>
		Assert.IsNull(Guid.Empty.NullIfEmpty());
}
