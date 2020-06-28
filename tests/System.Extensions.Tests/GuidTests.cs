using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Extensions.Tests
{
    [TestClass]
    public class GuidTests
    {
        [TestMethod]
        public void Guid_TreatEmptyAsNull_Empty() => 
            Assert.AreEqual(null, Guid.Empty.TreatEmptyAsNull());
    }
}
