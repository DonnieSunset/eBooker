using System.IO.Compression;
using System.Xml.Linq;

namespace BE
{
    public class eBook(string fileLocation) : IDisposable
    {
        private string myFileLocation = fileLocation;

        private ZipArchive myZipArchive = null;
        private Cover myCover = new Cover();

        public record MetaDataRecord
        {
            public Author? Author1 { get; set; } = null;
            public Author? Author2 { get; set; } = null;
        }

        private MetaDataRecord? myMetaData = null;           

        public MetaDataRecord MetaData 
        {
            get
            { 
                if (myMetaData == null)
                {
                    this.ReadMetaData();
                }
                return myMetaData!;
            }
        }

        internal ZipArchive ZipArchiveRead 
        {
            get
            { 
                if (myZipArchive == null)
                {
                    myZipArchive = ZipFile.OpenRead(myFileLocation);
                }
                
                return myZipArchive;
            }
        }

        internal ZipArchive ZipArchiveUpdate
        {
            get
            {
                if (myZipArchive == null)
                {
                    myZipArchive = ZipFile.Open(myFileLocation, ZipArchiveMode.Update);
                }
                else if (myZipArchive.Mode != ZipArchiveMode.Update)
                { 
                    myZipArchive.Dispose();
                    myZipArchive = ZipFile.Open(myFileLocation, ZipArchiveMode.Update);
                }
                
                return myZipArchive;
            }
        }

        private ZipArchiveEntry? myOpfEntry = null;
        private string? myOpfRelativePath = null;
        public ZipArchiveEntry OpfEntryRead
        { 
            get
            {
                if (myOpfEntry == null)
                {
                    myOpfEntry = ReloadOpfFromArchive(ZipArchiveRead);
                    myOpfRelativePath = myOpfEntry.FullName.Replace(myOpfEntry.Name, string.Empty);
                }
                
                return myOpfEntry;
            }
        }

        public ZipArchiveEntry OpfEntryUpdate
        {
            get
            {
                if (myOpfEntry == null || myZipArchive.Mode != ZipArchiveMode.Update)
                {
                    myOpfEntry = ReloadOpfFromArchive(ZipArchiveUpdate);
                    myOpfRelativePath = myOpfEntry.FullName.Replace(myOpfEntry.Name, string.Empty);
                }

                return myOpfEntry;
            }
        }


        public MemoryStream GetCover()
        {
            return myCover.GetImage(GetCoverArchiveEntry());
        }

        public ZipArchiveEntry? GetCoverArchiveEntry()
        {
            var opfFile = OpfEntryRead;
            using (var opfStream = opfFile.Open())
            {
                XDocument opfXmlDoc = XDocument.Load(opfStream);
                List<string> opfMetaEntries = OpfModifier.GetCoverMetaEntries(opfXmlDoc);
                List<string> opfManifestEntries = OpfModifier.GetCoverManifestEntries(opfXmlDoc, opfMetaEntries);

                if (opfManifestEntries == null || opfManifestEntries.Count == 0)
                {
                    return null;
                }
                else if (opfManifestEntries.Count > 1)
                {
                    throw new EbookerException($"Could not identify unique cover entry for ebook <{myFileLocation}>. " +
                        $"Candidates are: {Environment.NewLine} {String.Join(Environment.NewLine, opfManifestEntries)}");
                }
                else
                {
                    string coverLink = opfManifestEntries.Single();
                    string relativeCoverLink = myOpfRelativePath + coverLink;
                    return ZipArchiveRead.Entries.FirstOrDefault(
                        x => x.FullName.Equals(relativeCoverLink, StringComparison.CurrentCultureIgnoreCase));
                }
            }
        }

        public void UpdateCover(string coverFileLocation)
        {
            using (var opfStream = OpfEntryUpdate.Open())
            {
                // cover shall be placed always at the same folder level as the opf
                // such that we dont have to deal with relative paths
                string relativePathOfOpfFile = myOpfRelativePath;
                string coverFileName = "cover.jpg";
                string coverLocationInsideArchive = relativePathOfOpfFile + coverFileName;

                XDocument xmlDoc = XDocument.Load(opfStream);

                var metaDataCoverIDs = OpfModifier.RemoveCoverMetaEntries(xmlDoc);
                var existingCoverFiles = OpfModifier.RemoveCoverManifestEntries(xmlDoc, metaDataCoverIDs);
                OpfModifier.AddCoverEntry(xmlDoc, coverFileName);
                UpdateOpfInArchive(opfStream, xmlDoc);

                RemoveCoverFilesFromArchive(relativePathOfOpfFile, existingCoverFiles);
                
                // sometimes the exact same cover file already exists even without being
                // referenced in the opf metadata
                TryRemoveCoverFilesFromArchive(string.Empty, [coverLocationInsideArchive]);    
                
                WriteCoverFileToArchive(coverFileLocation, coverLocationInsideArchive);
            }

            //Important, otherwise it will not get saved
            ZipArchiveUpdate.Dispose();
            Dispose();
        }

        public void UpdateAuthors(Author author1, Author author2)
        {
            using (var opfStream = OpfEntryUpdate.Open())
            {
                XDocument xmlDoc = XDocument.Load(opfStream);

                if (author1 == null || String.IsNullOrEmpty(author1.DisplayName))
                {
                    throw new EbookerException($"{nameof(author1)} must be set to valid author.");
                }

                OpfModifier.SetAuthors(xmlDoc, author1, author2);
                UpdateOpfInArchive(opfStream, xmlDoc);
            }

            //Important, otherwise it will not get saved
            ZipArchiveUpdate.Dispose();
            this.Dispose();
        }

        private void ReadMetaData()
        {
            myMetaData = new MetaDataRecord();

            using (var opfStream = OpfEntryRead.Open())
            {
                XDocument xmlDoc = XDocument.Load(opfStream);

                var authors = OpfModifier.GetAuthors(xmlDoc);
                if (authors.Count > 0)
                {
                    myMetaData.Author1 = new Author(authors[0]);
                }
                if (authors.Count > 1) 
                {
                    myMetaData.Author2 = new Author(authors[1]);
                }
            }
        }

        public void Dispose()
        {
            myZipArchive?.Dispose();
            myZipArchive = null;
        }

        private void UpdateOpfInArchive(Stream opfStream, XDocument xmlDoc)
        {
            opfStream.Position = 0;
            using (StreamWriter writer = new StreamWriter(opfStream))
            {
                writer.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine);
                writer.Write(xmlDoc);
                opfStream.SetLength(opfStream.Position);
            }
        }

        private void WriteCoverFileToArchive(string coverFileLocation, string coverFileLocationInArchive)
        {
            ZipArchiveUpdate.CreateEntryFromFile(coverFileLocation, coverFileLocationInArchive);
        }

        private void RemoveCoverFilesFromArchive(string relativePath, List<string> zipCoverEntries)
        {
            foreach (string zipCoverEntry in zipCoverEntries)
            {
                string completePath = relativePath + zipCoverEntry;

                //remove relative entries, e.g. OEBPS/images/../cover.jpeg should result to OEBPS/cover.jpeg
                completePath = ResolveDirectoryJumps(completePath);

                // there can also be multiple entries of the same file in a zip file, which is an error case, but should be handled 
                var foundEntries = ZipArchiveUpdate.Entries.Where(x => x.FullName.Equals(completePath));
                if (foundEntries.Count() == 0)
                {
                    throw new EbookerException($"Could not find referenced cover file with href <{completePath}> in zipfile <{myFileLocation}>.");
                }

                foreach (var entry in foundEntries.ToArray())
                { 
                    entry.Delete();
                }
            }
        }

        private bool TryRemoveCoverFilesFromArchive(string relativePath, List<string> zipCoverEntries)
        {
            try
            {
                RemoveCoverFilesFromArchive(relativePath, zipCoverEntries);
                return true;
            }
            catch 
            {
                return false;
            }
        }

        internal string ResolveDirectoryJumps(string inputPath)
        {
            if (inputPath.StartsWith(".."))
                throw new EbookerException($"Path inside an archive cannot start with <..>");

            if (!inputPath.EndsWith("/")) inputPath += "/"; //otherwise last component would be treated as file

            string relativePathWithEnvironment = Path.GetFullPath(inputPath);
            string absolutePathWithEnvironment = Path.GetDirectoryName(relativePathWithEnvironment);
            string reducedPath = absolutePathWithEnvironment.Replace(Environment.CurrentDirectory, "");

            reducedPath = reducedPath.Replace("\\", "/");
            reducedPath = reducedPath.TrimStart('/');
            reducedPath = reducedPath.TrimEnd('/');

            return reducedPath;
        }

        private ZipArchiveEntry ReloadOpfFromArchive(ZipArchive zipArchive)
        {
            try
            {
                myOpfEntry = zipArchive.Entries?.SingleOrDefault(
                   x => x.Name.EndsWith(".opf", StringComparison.CurrentCultureIgnoreCase));

                if (myOpfEntry == null)
                {
                    throw new EbookerException($"Could not find opf file in ebook <{myFileLocation}>.");
                }

                return myOpfEntry;
            }
            catch (InvalidOperationException ex)
            {
                throw new EbookerException($"Seems there are multiple opf file in ebook <{myFileLocation}>. See inner exception for more details.", ex);
            }
        }
    }
}
