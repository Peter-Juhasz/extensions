using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Extensions.Tests
{
    [TestClass]
    public class UriTests
    {
        [TestMethod]
        public void Uri_AppendQueryString_First() => 
            Assert.AreEqual(new Uri("https://example.org/?a=b"), new Uri("https://example.org").AppendQueryString("a", "b"));

        [TestMethod]
        public void Uri_AppendQueryString_First_Null() =>
            Assert.AreEqual(new Uri("https://example.org"), new Uri("https://example.org").AppendQueryString("a", null));

        [TestMethod]
        public void Uri_AppendQueryString_Second() =>
            Assert.AreEqual(new Uri("https://example.org/?a=b&c=d"), new Uri("https://example.org/?a=b").AppendQueryString("c", "d"));

        [TestMethod]
        public void Uri_AppendQueryString_Second_Null() =>
            Assert.AreEqual(new Uri("https://example.org/?a=b"), new Uri("https://example.org/?a=b").AppendQueryString("b", null));

        [TestMethod]
        public void Uri_AppendQueryString_Encode() =>
            Assert.AreEqual(new Uri("https://example.org/?a=%2F%25%26%3D%23%3F"), new Uri("https://example.org").AppendQueryString("a", "/%&=#?"));
    }
}
