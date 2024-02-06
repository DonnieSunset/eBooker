using BE;
using BE.MetaData;

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
        public void UpdateAuthors_MetaDataMissing_AuthorAdded()
        {
            var epubFile = @"C:\temp\EbookTestData\Special\Special_NoMetaData.epub";
            var ebookTempFileLocation = CreateLocalCopy(epubFile);

            var eBook = new eBook(ebookTempFileLocation);
            Assert.That(eBook.MetaData.Authors.Data.Item1, Is.Null, "Author1");
            Assert.That(eBook.MetaData.Authors.Data.Item2, Is.Null, "Author2");

            var newAuthor = new Author("McQuillington, Chuckle ");

            eBook.UpdateAuthors(newAuthor, null);
            eBook.Dispose();

            eBook = new eBook(ebookTempFileLocation);
            
            Assert.That(eBook.MetaData.Authors.Data.Item1, Is.Not.Null, "Author1");
            Assert.That(eBook.MetaData.Authors.Data.Item1.SortName, Is.EqualTo("McQuillington, Chuckle"));
            Assert.That(eBook.MetaData.Authors.Data.Item2, Is.Null, "Author2");
        }

        [Test]
        public void UpdateAuthors_TwoAuthorsAdded_AuthorAdded()
        {
            var epubFile = @"C:\temp\EbookTestData\Special\Special_StandardOpfCoverJpeg.epub";
            var ebookTempFileLocation = CreateLocalCopy(epubFile);

            var eBook = new eBook(ebookTempFileLocation);
            Assert.That(eBook.MetaData.Authors.Data.Item1, Is.Not.Null, "Author1");
            Assert.That(eBook.MetaData.Authors.Data.Item2, Is.Null, "Author2");

            var newAuthor1 = new Author("McQuillington, Chuckle ");
            var newAuthor2 = new Author("Stephen King");

            eBook.UpdateAuthors(newAuthor1, newAuthor2);
            eBook.Dispose();

            eBook = new eBook(ebookTempFileLocation);
            Assert.That(eBook.MetaData.Authors.Data.Item1, Is.Not.Null, "Author1");
            Assert.That(eBook.MetaData.Authors.Data.Item1.DisplayName, Is.EqualTo("Chuckle McQuillington"));
            Assert.That(eBook.MetaData.Authors.Data.Item2, Is.Not.Null, "Author2");
            Assert.That(eBook.MetaData.Authors.Data.Item2.DisplayName, Is.EqualTo("Stephen King"));
        }

        [Test]
        public void UpdateAuthors_TwoAuthorsExistingOnlyOneAdded_OtherAuthorRemoved()
        {
            var epubFile = @"C:\temp\EbookTestData\Special\Special_TwoAuthors.epub";
            var ebookTempFileLocation = CreateLocalCopy(epubFile);

            var eBook = new eBook(ebookTempFileLocation);
            Assert.That(eBook.MetaData.Authors.Data.Item1, Is.Not.Null, "Author1");
            Assert.That(eBook.MetaData.Authors.Data.Item2, Is.Not.Null, "Author2");

            var newAuthor1 = new Author("McQuillington, Chuckle ");

            eBook.UpdateAuthors(newAuthor1, null);
            eBook.Dispose();

            eBook = new eBook(ebookTempFileLocation);
            Assert.That(eBook.MetaData.Authors.Data.Item1, Is.Not.Null, "Author1");
            Assert.That(eBook.MetaData.Authors.Data.Item1.DisplayName, Is.EqualTo("Chuckle McQuillington"));
            Assert.That(eBook.MetaData.Authors.Data.Item2, Is.Null, "Author2");
        }

        [Test]
        public void UpdateAuthors_HappyPath()
        {
            foreach (var epubFile in GetListOfTestData())
            {
                var ebookTempFileLocation = CreateLocalCopy(epubFile);
                var eBook = new eBook(ebookTempFileLocation);

                var newAuthor = new Author("McQuillington, Chuckle ");
                eBook.UpdateAuthors(newAuthor, null);
                eBook.Dispose();

                eBook = new eBook(ebookTempFileLocation);
                Assert.That(eBook.MetaData.Authors.Data.Item1, Is.Not.Null, "Author1");
                Assert.That(eBook.MetaData.Authors.Data.Item1.SortName, Is.EqualTo("McQuillington, Chuckle"));
                Assert.That(eBook.MetaData.Authors.Data.Item2, Is.Null, "Author2");
            }
        }

        [Test]
        public void UpdateAuthors_NoAuthorSet_ThrowsException()
        {
            var epubFile = @"C:\temp\EbookTestData\Special\Special_StandardOpfCoverJpeg.epub";
            var ebookTempFileLocation = CreateLocalCopy(epubFile);
            var eBook = new eBook(ebookTempFileLocation);

            Assert.That(() => eBook.UpdateAuthors(null, null), Throws.TypeOf<EbookerException>());
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
            
            Assert.That(list.Count, Is.EqualTo(639));
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