using BE;
using System.IO;
using System.IO.Compression;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace BE_iTest
{
    public class ReaderTests
    {
        string testDataFolder = @"C:\temp\EbookTestData";
        string coverFileLocation = @"C:\temp\EbookTestData\Special\IMG_20170605_133025.jpg";

        [SetUp]
        public void Setup()
        {
        }

        //[TestCase("<center><img src=\"Image00008.jpg\" style=\"width:100%;height:100%;\"/>", "Image00008.jpg")]
        //public void GetImageSourceFromHtmlTest(string htmlString, string expectedMatch)
        //{
        //    var reader = new Cover();
        //    var result = reader.GetImageSourceFromHtml(htmlString);

        //    Assert.That(result, Is.EqualTo(expectedMatch));
        //}

        [Test]
        public void ReadCoverImage()
        {
            var files = GetListOfTestData();
            Assert.That(files.Count, Is.EqualTo(638));

            foreach ( var file in files )
            {
                var eBook = new eBook(file);
                var imageMemoryStream = eBook.GetCover();
                Assert.That(imageMemoryStream.Length, Is.GreaterThan(0), $"Image Memory Stream Size for <{file}>.");
            }
        }

        [Test]
        public void SetCoverInOpf_OpfDoesNotExist_ThrowsException()
        {
            var eBook = new eBook(@"C:\temp\EbookTestData\Special\Special_NoOpf.epub");

            Assert.That(() => eBook.SetCover(coverFileLocation), Throws.InstanceOf<EbookerException>());
        }

        [Test]
        public void SetCoverInOpf_OpfExistsTwice_ThrowsException()
        {
            var eBook = new eBook(@"C:\temp\EbookTestData\Special\Special_MultipleOpf.epub");

            Assert.That(() => eBook.SetCover(coverFileLocation), Throws.InstanceOf<EbookerException>());
        }

        [Test]
        public void SetCoverInOpf_CoverEntryAndCoverFileAlreadyExist_CoverEntryAndFileAreOverwritten()
        {
            var ebookTempFileLocation = CreateLocalCopy(@"C:\temp\EbookTestData\Special\Special_StandardOpfCoverJpeg.epub");
            eBook eBook = null;

            try
            {
                eBook = new eBook(ebookTempFileLocation);
                eBook.SetCover(coverFileLocation);
                eBook.Dispose();

                //momentan noch problem: wenn schon existiert macht er einfach ein doppeltes entry

                eBook = new eBook(ebookTempFileLocation);
                var newCover = eBook.GetCoverArchiveEntry();
                
                var sizeOfUpdatedCover = newCover.Length;
                var sizeOfCoverOnFileSystem = new FileInfo(coverFileLocation).Length;

                // This is a lazy "image comparison", but better than nothing
                Assert.That(sizeOfUpdatedCover, Is.EqualTo(sizeOfCoverOnFileSystem), $"Sizes of cover in updated ebook ({sizeOfUpdatedCover}) should be the size of the cover file on file system ({sizeOfCoverOnFileSystem}).");
            }
            finally
            {
                eBook?.Dispose();
                File.Delete(ebookTempFileLocation);
            }
        }

        [Test]
        public void HowWritingWorks()
        {
            var myFileLocation = @"C:\temp\Conrad, Joseph - Das Herz der Finsternis_mod.epub";
            var eBook = new eBook(myFileLocation);

            var opfFile = eBook.GetFromArchiveOpf();

            XDocument xmlDoc1;
            using (var opfStream = opfFile.Open())
            {
                xmlDoc1 = XDocument.Load(opfStream);
                var root = xmlDoc1.Root;
                var manifest = root?.Elements().SingleOrDefault(x => x.Name.LocalName == "manifest");

                //add an additional item
                manifest.Add(new XElement(manifest.GetDefaultNamespace() + "Stefan2",
                    new XAttribute("FirstName", "Stef"),
                    new XAttribute("SurName", "Holpp")
                    ));
            }

            using (var opfStream = opfFile.Open())
            { 
                //write it back
                //opfStream.SetLength(0);
                using (StreamWriter writer = new StreamWriter(opfStream))
                {
                    writer.Write(xmlDoc1); //entry contents "baz123"
                }
            }

            //Important, otherwise it will not get saved
            //myZipArchive.Dispose();
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

        private string CreateLocalCopy(string originalFileLocation)
        {
            var localCopy = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".epub");
            File.Copy(originalFileLocation, localCopy);

            return localCopy;
        }
    }
}