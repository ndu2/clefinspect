using ndu.ClefInspect.Model;
using System.Text.Json.Nodes;

namespace ndu.ClefInspect.Tests.Model
{
    [TestClass]
    public class TextFilterParserTests
    {
        [TestMethod]
        public void Parse_Null()
        {
            List<string> result = TextFilterParser.Parse(null);
            Assert.AreEqual(0, result.Count);
        }
        [TestMethod]
        public void Parse_Empty()
        {
            List<string> result = TextFilterParser.Parse("");
            Assert.AreEqual(0, result.Count);
        }
        [TestMethod]
        public void Parse_QuotedEmptyString()
        {
            List<string> result = TextFilterParser.Parse("\"\"");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("\"", result[0]);
        }
        [TestMethod]
        public void Parse_Text()
        {
            List<string> result = TextFilterParser.Parse("\"\"asdf");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("\"asdf", result[0]);
        }
        [TestMethod]
        public void Parse_Text2()
        {
            List<string> result = TextFilterParser.Parse("asdf\"\"asdf");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("asdf\"\"asdf", result[0]);
        }
        [TestMethod]
        public void Parse_TextTrim()
        {
            List<string> result = TextFilterParser.Parse(" asdf ");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("asdf", result[0]);
        }
        [TestMethod]
        public void Parse_QuotedText()
        {
            List<string> result = TextFilterParser.Parse("\"asdfasdf\"");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("asdfasdf", result[0]);
        }
        [TestMethod]
        public void Parse_QuotedText1()
        {
            List<string> result = TextFilterParser.Parse("\" asdfasdf\"");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(" asdfasdf", result[0]);
        }
        [TestMethod]
        public void Parse_QuotedText2()
        {
            List<string> result = TextFilterParser.Parse("\"asdfasdf \"");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("asdfasdf ", result[0]);
        }
        [TestMethod]
        public void Parse_QuoteInQuote()
        {
            List<string> result = TextFilterParser.Parse("\"asdf\"\"asdf \"");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("asdf\"asdf ", result[0]);
        }
        [TestMethod]
        public void Parse_DelimText()
        {
            List<string> result = TextFilterParser.Parse("asdf , basd");
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("asdf", result[0]);
            Assert.AreEqual("basd", result[1]);
        }
        [TestMethod]
        public void Parse_DelimText2()
        {
            List<string> result = TextFilterParser.Parse(" , ");
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("", result[0]);
            Assert.AreEqual("", result[1]);
        }
        [TestMethod]
        public void Parse_QuotedDelimText()
        {
            List<string> result = TextFilterParser.Parse("\"as,df \", basd");
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("as,df ", result[0]);
            Assert.AreEqual("basd", result[1]);
        }
        [TestMethod]
        public void Parse_QuotedDelimText2()
        {
            List<string> result = TextFilterParser.Parse(" xyz   ,\"as,df \", basd");
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("xyz", result[0]);
            Assert.AreEqual("as,df ", result[1]);
            Assert.AreEqual("basd", result[2]);
        }
        [TestMethod]
        public void Parse_QuotedDelimText3()
        {
            List<string> result = TextFilterParser.Parse("\"asdf \",\" b,asd\"");
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("asdf ", result[0]);
            Assert.AreEqual(" b,asd", result[1]);
        }
        [TestMethod]
        public void Parse_QuotedDelimText4()
        {
            List<string> result = TextFilterParser.Parse("\"as,df \",");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("as,df ", result[0]);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Parse_InvalidQuote_ThrowsException()
        {
            List<string> result = TextFilterParser.Parse("\"");
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Parse_InvalidQuote2_ThrowsException()
        {
            List<string> result = TextFilterParser.Parse("\"2132\",\"");
        }
    }
}
