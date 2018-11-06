using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSoup;
using NSoup.Nodes;

namespace Test.Parser
{
    [TestClass]
    public class FixBugTest
    {
        [TestMethod]
        public void BugsFixed()
        {
            string convertImageToImg = "<body><image><svg><image /></svg></body>";
            string uncloseAnchors = "<a href='http://example.com/'>Link<p>Error link</a>";
            string tagInTextarea = "<textarea><p>Jsoup</p></textarea>";
            string supportNonAsciiTag = "<進捗推移グラフ>Yes</進捗推移グラフ><русский-тэг>Correct</<русский-тэг>";
            string relaxedBaseEntryMatchAndStrictExtendedMatch = "&amp &quot &reg &icy &hopf &icy; &hopf;";
            string unknowEmptyStyle = "<html><head><style /><meta name=foo></head><body>One</body></html>";


            Assert.AreEqual("<img />\n<svg>\n <image />\n</svg>", Sanitize(convertImageToImg));
            Assert.AreEqual("<a href=\"http://example.com/\">Link</a>\n<p><a href=\"http://example.com/\">Error link</a></p>", Sanitize(uncloseAnchors));
            Assert.AreEqual("<textarea>&lt;p&gt;Jsoup&lt;/p&gt;</textarea>", Sanitize(tagInTextarea));
            Assert.AreEqual("<進捗推移グラフ>\n Yes\n</進捗推移グラフ>\n<русский-тэг>\n Correct\n <!--<русский-тэг-->\n</русский-тэг>", Sanitize(supportNonAsciiTag));
            Assert.AreEqual("&amp; &quot; &reg; &amp;icy &amp;hopf и &#55349;&#56665;", Sanitize(relaxedBaseEntryMatchAndStrictExtendedMatch));
            Assert.AreEqual("<meta name=\"foo\" />One", Sanitize(unknowEmptyStyle));
        }

        [TestMethod]
        public void OutOfRange()
        {
            string cdata = "<![CDATA[]]";
            Document doc = NSoupClient.ParseBodyFragment(cdata, "http://localhost");
            Assert.AreEqual("]]", doc.Body.Html());

        }

        [TestMethod]
        public void UnknowEmptyNoFrame()
        {
            string unknowEmptyNoFrame = "<html><head><noframes /><meta name=foo></head><body>One</body></html>";
            Document doc = NSoupClient.ParseBodyFragment(unknowEmptyNoFrame, "http://localhost");
            Assert.IsTrue(!doc.Select("meta").IsEmpty);
            Assert.AreEqual("One", doc.Select("body").First.Text());

        }

        [TestMethod]
        public void UnknowEmptyBlocks()
        {
            string unknowEmptyBlocks = "<div id='1' /><script src='/foo' /><div id=2><img /><img></div><a id=3 /><i /><foo /><foo>One</foo> <hr /> hr text <hr> hr text two";
            Document doc = NSoupClient.ParseBodyFragment(unknowEmptyBlocks, "http://localhost");
            Element div1 = doc.GetElementById("1");
            Assert.IsTrue(doc.Select("hr").First.Children.IsEmpty);
            Assert.IsTrue(doc.Select("hr").Last.Children.IsEmpty);
            Assert.IsTrue(doc.Select("img").First.Children.IsEmpty);
            Assert.IsTrue(doc.Select("img").Last.Children.IsEmpty);
        }


        private static string Sanitize(string input)
        {
            Document doc = NSoupClient.ParseBodyFragment(input, "http://localhost");
            return doc.Body.Html();
        }
    }
}
