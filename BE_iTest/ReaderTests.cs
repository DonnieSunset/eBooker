using BE;
using System.Text.RegularExpressions;

namespace BE_iTest
{
    public class ReaderTests
    {
        string testDataFolder = @"C:\temp\EbookTestData";

        [SetUp]
        public void Setup()
        {
        }

        [TestCase("<center><img src=\"Image00008.jpg\" style=\"width:100%;height:100%;\"/>", "Image00008.jpg")]
        public void GetImageSourceFromHtmlTest(string htmlString, string expectedMatch)
        {
            var reader = new Reader();
            var result = reader.GetImageSourceFromHtml(htmlString);

            Assert.That(result, Is.EqualTo(expectedMatch));
        }

        [Test]
        public void ReadCoverImage()
        {
            var reader = new Reader();
            var files = GetListOfTestData();

            Assert.That(files.Count, Is.EqualTo(639));

            foreach ( var file in files )
            {
                var imageMemoryStream = reader.GetImage(file);
                Assert.That(imageMemoryStream.Length, Is.GreaterThan(0), $"Image Memory Stream Size for <{file}>.");
            }
        }

        private List<string> GetListOfTestData()
        {
            var list = new List<string>();
            foreach (var fileLoction in Directory.GetFiles(testDataFolder, "*.epub", SearchOption.AllDirectories))
            {
                list.Add(fileLoction);
            }
            return list;
        }
    }
}