using BE;
using System.Xml.Linq;

namespace BE_iTest
{
    [TestFixture]
    internal class OpfModifierTests
    {
        [Test]
        public void RemoveCoverMetaEntries_MultipleCoverEntries_AllCoverEntriesRemoved()
        {
            string opfString =
                """
                <package xmlns="http://www.idpf.org/2007/opf" version="2.0">
                  <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                    <dc:title>someTitle</dc:title>
                    <meta name="cover" content="cover1" />
                    <meta name="Cover" content="cover2" />
                    <meta name="Author" content="someAuthor" />
                  </metadata>
                  <manifest>
                    <item href="cover.jpg" id="cover1" media-type="image/jpeg" />
                  </manifest>
                </package>
                """;

            string opfStringExpectedResult =
                """
                <package xmlns="http://www.idpf.org/2007/opf" version="2.0">
                  <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                    <dc:title>someTitle</dc:title>
                    <meta name="Author" content="someAuthor" />
                  </metadata>
                  <manifest>
                    <item href="cover.jpg" id="cover1" media-type="image/jpeg" />
                  </manifest>
                </package>
                """;

            XDocument opfDoc = XDocument.Parse(opfString);

            var coverContentIDs = OpfModifier.RemoveCoverMetaEntries(opfDoc);

            Assert.That(coverContentIDs, Is.EquivalentTo(new List<string>() { "cover1", "cover2" }));
            Assert.That(opfDoc.ToString(), Is.EqualTo(opfStringExpectedResult));
        }
    }
}
