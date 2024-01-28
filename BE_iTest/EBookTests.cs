using BE;
using System.IO;
using System.IO.Compression;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace BE_iTest
{
    public class EBookTests
    {
        string testDataFolder = @"C:\temp\EbookTestData\Regular";
        string coverFileLocation = @"C:\temp\EbookTestData\Special\IMG_20170605_133025.jpg";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void SetCoverInOpf_OpfDoesNotExist_ThrowsException()
        {
            var eBook = new eBook(@"C:\temp\EbookTestData\Special\Special_NoOpf.epub");

            Assert.That(() => eBook.UpdateCover(coverFileLocation), Throws.InstanceOf<EbookerException>());
        }

        [Test]
        public void SetCoverInOpf_OpfExistsTwice_ThrowsException()
        {
            var eBook = new eBook(@"C:\temp\EbookTestData\Special\Special_MultipleOpf.epub");

            Assert.That(() => eBook.UpdateCover(coverFileLocation), Throws.InstanceOf<EbookerException>());
        }

        // add test for remove cover

        [Test]
        [Category("HappyPath")]
        public void UpdateCover_CoverEntryAndCoverFileAlreadyExist_CoverEntryAndFileAreOverwritten()
        {
            foreach (var epubFile in GetListOfTestData())
            {
                var ebookTempFileLocation = CreateLocalCopy(epubFile);

                var eBook = new eBook(ebookTempFileLocation);
                eBook.UpdateCover(coverFileLocation);
                eBook.Dispose();

                eBook = new eBook(ebookTempFileLocation);
                var newCover = eBook.GetCoverArchiveEntry();

                var sizeOfUpdatedCover = newCover.Length;
                var sizeOfCoverOnFileSystem = new FileInfo(coverFileLocation).Length;

                // This is a lazy "image comparison", but better than nothing
                Assert.That(sizeOfUpdatedCover, Is.EqualTo(sizeOfCoverOnFileSystem), 
                    $"Sizes of cover ({sizeOfUpdatedCover}) in updated ebook should be " +
                    $"the size of the cover file ({sizeOfCoverOnFileSystem}) on file system. " +
                    $"Original Ebook file: <{epubFile}>. Temp file: <{ebookTempFileLocation}>.");

                var entries = eBook.ZipArchiveRead.Entries.Select(x => x.FullName).ToList();
                Assert.That(entries.Count, Is.EqualTo(entries.Distinct().ToList().Count),
                    "There shall be no duplicate entries in the resulting archiv.");

                // delete file only in success case, leave it there for debugging in exception case
                eBook?.Dispose();
                File.Delete(ebookTempFileLocation);
            }
        }

        [Test]
        public void ReadAndWriteMetaData_MetaDataMissing()
        {
            var epubFile = @"C:\temp\EbookTestData\Special\Special_NoMetaData.epub";
            var ebookTempFileLocation = CreateLocalCopy(epubFile);

            var eBook = new eBook(ebookTempFileLocation);
            eBook.ReadMetaData();
            Assert.That(eBook.MetaData.Authors.Count, Is.EqualTo(0));

            string newAuthor = "McQuillington, Chuckle ";
            eBook.UpdateMetaInformation(newAuthor);
            eBook.Dispose();

            eBook = new eBook(ebookTempFileLocation);
            eBook.ReadMetaData();
            Assert.That(eBook.MetaData.Authors, Has.Exactly(1).EqualTo(newAuthor));
        }

        [Test]
        public void ReadAndWriteMetaData_MetaDataExisting()
        {
            foreach (var epubFile in GetListOfTestData())
            {
                var ebookTempFileLocation = CreateLocalCopy(epubFile);

                var eBook = new eBook(ebookTempFileLocation);
                eBook.ReadMetaData();

                string newAuthor = "McQuillington, Chuckle";
                eBook.UpdateMetaInformation(newAuthor);
                eBook.Dispose();

                eBook = new eBook(ebookTempFileLocation);
                eBook.ReadMetaData();
                Assert.That(eBook.MetaData.Authors, Has.Exactly(1).EqualTo(newAuthor));
            }
        }

        [TestCase("one/two/three", "one/two/three")]
        [TestCase("one\\two\\three\\", "one/two/three")]
        [TestCase("one/two/..", "one")]
        [TestCase("one/../three", "three")]
        [TestCase("OEBPS/images/../cover.jpeg", "OEBPS/cover.jpeg")]
        public void ResolveDirectoryJumps_SomeVariations_ContainsNoDirectoryJumps(string actualPath, string expectedPath)
        {
            var ebook = new eBook("");
            Assert.That(ebook.ResolveDirectoryJumps(actualPath), Is.EqualTo(expectedPath));
        }

        private List<string> GetListOfTestData()
        {
            var list = new List<string>();
            foreach (var fileLoction in Directory.GetFiles(testDataFolder, "*.epub", SearchOption.AllDirectories))
            {
                list.Add(fileLoction);
            }
            
            Assert.That(list.Count, Is.EqualTo(638));
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