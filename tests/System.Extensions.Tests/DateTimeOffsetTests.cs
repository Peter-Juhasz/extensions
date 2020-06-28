using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Extensions.Tests
{
    [TestClass]
    public class DateTimeOffsetTests
    {
        [TestMethod]
        public void DateTimeOffset_StartOfWeek_DayBefore() => 
            Assert.AreEqual(new DateTimeOffset(2020, 06, 22, 00, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 06, 28, 17, 18, 00, TimeSpan.Zero).StartOfWeek());

        [TestMethod]
        public void DateTimeOffset_StartOfWeek_Exactly() =>
            Assert.AreEqual(new DateTimeOffset(2020, 06, 22, 00, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 06, 22, 00, 00, 00, TimeSpan.Zero).StartOfWeek());

        [TestMethod]
        public void DateTimeOffset_Floor() =>
            Assert.AreEqual(new DateTimeOffset(2020, 06, 22, 18, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 06, 22, 18, 18, 00, TimeSpan.Zero).Floor(TimeSpan.FromHours(1)));

        [TestMethod]
        public void DateTimeOffset_Floor_Exactly() =>
            Assert.AreEqual(new DateTimeOffset(2020, 06, 22, 18, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 06, 22, 18, 00, 00, TimeSpan.Zero).Floor(TimeSpan.FromHours(1)));

        [TestMethod]
        public void DateTimeOffset_Round_Down() =>
            Assert.AreEqual(new DateTimeOffset(2020, 06, 22, 18, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 06, 22, 18, 29, 00, TimeSpan.Zero).Round(TimeSpan.FromHours(1)));

        [TestMethod]
        public void DateTimeOffset_Round_Up() =>
            Assert.AreEqual(new DateTimeOffset(2020, 06, 22, 19, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 06, 22, 18, 31, 00, TimeSpan.Zero).Round(TimeSpan.FromHours(1)));

        [TestMethod]
        public void DateTimeOffset_Round_Exactly() =>
            Assert.AreEqual(new DateTimeOffset(2020, 06, 22, 18, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 06, 22, 18, 00, 00, TimeSpan.Zero).Round(TimeSpan.FromHours(1)));

        [TestMethod]
        public void DateTimeOffset_Ceiling() =>
            Assert.AreEqual(new DateTimeOffset(2020, 06, 22, 18, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 06, 22, 17, 18, 00, TimeSpan.Zero).Ceiling(TimeSpan.FromHours(1)));

        [TestMethod]
        public void DateTimeOffset_Ceiling_Exactly() =>
            Assert.AreEqual(new DateTimeOffset(2020, 06, 22, 18, 00, 00, TimeSpan.Zero), new DateTimeOffset(2020, 06, 22, 18, 00, 00, TimeSpan.Zero).Ceiling(TimeSpan.FromHours(1)));
    }
}
