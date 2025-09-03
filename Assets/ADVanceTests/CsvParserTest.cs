using System.Collections.Generic;
using ADVance.Utility;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace ADVance.Tests
{
    public class CSVParserTests
    {
        [Test]
        public void Parse_SimpleCsv_ReturnsFields()
        {
            var csv = "a,b,c";
            var result = CsvParser.Parse(csv);
            Assert.AreEqual(1, result.Count);
            CollectionAssert.AreEqual(new List<string> { "a", "b", "c" }, result[0]);
        }

        [Test]
        public void Parse_QuotedFieldWithComma_ReturnsCorrectFields()
        {
            var csv = "\"a,b\",c";
            var result = CsvParser.Parse(csv);
            Assert.AreEqual(1, result.Count);
            CollectionAssert.AreEqual(new List<string> { "a,b", "c" }, result[0]);
        }

        [Test]
        public void Parse_MultiLineCsv_ReturnsMultipleRows()
        {
            var csv = "a,b\nc,d";
            var result = CsvParser.Parse(csv);
            Assert.AreEqual(2, result.Count);
            CollectionAssert.AreEqual(new List<string> { "a", "b" }, result[0]);
            CollectionAssert.AreEqual(new List<string> { "c", "d" }, result[1]);
        }

        [Test]
        public void Parse_EmptyField_ReturnsEmptyString()
        {
            var csv = "a,,c";
            var result = CsvParser.Parse(csv);
            CollectionAssert.AreEqual(new List<string> { "a", "", "c" }, result[0]);
        }

        [Test]
        public void Parse_EscapedQuote_ReturnsQuoteInField()
        {
            var csv = "\"a\"\"b\",c";
            var result = CsvParser.Parse(csv);
            CollectionAssert.AreEqual(new List<string> { "a\"b", "c" }, result[0]);
        }
    }
}