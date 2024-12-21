using clef_inspect.Model;
using System.Text.Json.Nodes;

namespace ndu.ClefInspect.Tests.Model
{
    [TestClass]
    public class TextFilterTests
    {
        private readonly ClefLine testLogLine1;
        private readonly ClefLine testLogLine2;
        private readonly ClefLine testLogLine3;


        public TextFilterTests()
        {
            JsonObject? test1 = JsonNode.Parse("{\"@t\":\"2024-12-05T15:03:51.1732124Z\",\"@m\":\"log in kv scope\",\"@i\":\"7e0ac33d\",\"Scope\":[{\"3\":\"a\",\"6\":\"b\",\"12\":\"c\"}]}") as JsonObject;
            JsonObject? test2 = JsonNode.Parse("{\"@t\":\"2024-12-18T20:02:44.2677859Z\",\"@m\":\"my, message is \\\"halihalo\\\"\",\"@i\":\"ea85fb10\",\"@l\":\"Debug\",\"message\":\"halihalo\",\"SourceContext\":\"test\",\"name\":\"hallo\",\"Scope\":[\"test hallo\"],\"ThreadId\":1}") as JsonObject;
            JsonObject? test3 = JsonNode.Parse("{\"@t\":\"2024-12-18T20:04:02.9613371Z\",\"@mt\":\"my message is {message}\",\"@l\":\"Debug\",\"message\":\"halihalo\",\"SourceContext\":\"test\",\"name\":\"hallo\",\"Scope\":[\"test hallo\"],\"ThreadId\":1}") as JsonObject;
            testLogLine1 = new(1, test1);
            testLogLine2 = new(1, test2);
            testLogLine3 = new(1, test3);
        }
        [TestMethod]
        public void Accept_AcceptAllEmpty()
        {
            Assert.IsTrue(new TextFilter(null).AcceptsAll);
            Assert.IsTrue(new TextFilter("").AcceptsAll);
            Assert.IsTrue(new TextFilter("   , ").AcceptsAll);
        }

        [TestMethod]
        public void Accept_IsCaseInsensitive()
        {
            TextFilter textFilter = new TextFilter("HalIhaLo");
            Assert.IsTrue(textFilter.Create().Accept(testLogLine2));
            Assert.IsTrue(textFilter.Create().Accept(testLogLine3));
        }
        [TestMethod]
        public void Accept_Trims_Whitespaces()
        {
            TextFilter textFilter = new TextFilter("a ,b, kv  ");
            Assert.IsTrue(textFilter.Create().Accept(testLogLine1));
        }
        [TestMethod]
        public void Accept_Handles_Quotes1()
        {
            TextFilter textFilter = new TextFilter("my, message");
            Assert.IsTrue(textFilter.Create().Accept(testLogLine2));
        }
        [TestMethod]
        public void Accept_Handles_Quotes2()
        {
            TextFilter textFilter = new TextFilter("\"my, message\"");
            Assert.IsTrue(textFilter.Create().Accept(testLogLine2));
        }

        [TestMethod]
        public void Accept_Handles_Quotes3()
        {
            TextFilter textFilter = new TextFilter("\"messag \"");
            Assert.IsFalse(textFilter.Create().Accept(testLogLine2));
            Assert.IsFalse(textFilter.Create().Accept(testLogLine3));
        }

        [TestMethod]
        public void Accept_Uses_Formatted_Message()
        {
            TextFilter textFilter = new TextFilter("\"halihalo\"");
            Assert.IsTrue(textFilter.Create().Accept(testLogLine2));
            Assert.IsTrue(textFilter.Create().Accept(testLogLine3));
        }
        [TestMethod]
        public void Accept_IgnoresData1()
        {
            TextFilter textFilter = new TextFilter("Debug");
            Assert.IsFalse(textFilter.Create().Accept(testLogLine1));
        }
        [TestMethod]
        public void Accept_IgnoresData2()
        {
            TextFilter textFilter = new TextFilter("test");
            Assert.IsFalse(textFilter.Create().Accept(testLogLine2));
        }
    }
}
