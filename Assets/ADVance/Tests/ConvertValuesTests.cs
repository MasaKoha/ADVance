using ADVance.Utility;
using NUnit.Framework;

namespace ADVance.Tests
{
    public class ConvertValuesTests
    {
        [TestCase("42", 42)]
        [TestCase("-7", -7)]
        public void ToInt_Valid_ReturnsParsed(string input, int expected)
        {
            Assert.AreEqual(expected, input.ToInt());
        }

        [Test]
        public void ToInt_Invalid_ReturnsDefault()
        {
            Assert.AreEqual(5, "invalid".ToInt(5));
        }

        [Test]
        public void ToFloat_Invalid_ReturnsDefault()
        {
            Assert.AreEqual(1.5f, "nope".ToFloat(1.5f));
        }

        [Test]
        public void ToBool_Valid_ReturnsParsed()
        {
            Assert.IsTrue("true".ToBool());
            Assert.IsFalse("false".ToBool(true));
        }

        [Test]
        public void ToULong_Invalid_ReturnsDefault()
        {
            Assert.AreEqual(9UL, "bad".ToULong(9UL));
        }

        [Test]
        public void ToInt_ObjectValue_ReturnsParsed()
        {
            var value = "7";
            Assert.AreEqual(7, value.ToInt());
        }

        [Test]
        public void ToBool_ObjectValue_ReturnsParsed()
        {
            var value = "true";
            Assert.IsTrue(value.ToBool());
        }

        [Test]
        public void ToDouble_ObjectNull_ReturnsDefault()
        {
            var value = "";
            Assert.AreEqual(2.5d, value.ToDouble(2.5d));
        }
    }
}