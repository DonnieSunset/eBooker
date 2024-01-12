using System.IO.Compression;
using System.Xml.Linq;

namespace BE
{
    public class eBook(string fileLocation) : IDisposable
    {
        private string myFileLocation = fileLocation;

        private ZipArchive myZipArchive = null;
        private Cover myCover = new Cover();

        private ZipArchive ZipArchiveRead 
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

        private ZipArchive ZipArchiveUpdate
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

        public MemoryStream GetCover()
        {
            return myCover.GetImage(ZipArchiveRead);
        }

        public ZipArchiveEntry GetCoverArchiveEntry()
        {
            return myCover.GetCoverFileByOpf(ZipArchiveRead);
        }

        public void GetMetaInformation()
        { 
            throw new NotImplementedException();
        }

        public void UpdateMetaInformation()
        {
            throw new NotImplementedException();
        }


        public void UpdateCover(string coverFileLocation)
        {
            var opfFile = GetOpf(writable:true);

            /// cover shall be placed always at the same folder level as the opf
            /// such that we dont have to deal with relative paths
            string relativePathOfOpfFile = opfFile.FullName.Replace(opfFile.Name, string.Empty);
            string coverFileName = "cover.jpg";
            string coverLocationInsideArchive = relativePathOfOpfFile + coverFileName;

            using (var opfStream = opfFile.Open())
            {
                XDocument xmlDoc = XDocument.Load(opfStream);

                var metaDataCoverIDs = OpfModifier.RemoveCoverMetaEntries(xmlDoc);
                var existingCoverFiles = OpfModifier.RemoveCoverManifestEntries(xmlDoc, metaDataCoverIDs);
                OpfModifier.AddCoverEntry(xmlDoc, coverFileName);
                UpdateOpfInArchive(opfStream, xmlDoc);

                RemoveCoverFilesFromArchive(relativePathOfOpfFile, existingCoverFiles);
                WriteCoverFileToArchive(coverFileLocation, coverLocationInsideArchive);
            }

            //Important, otherwise it will not get saved
            ZipArchiveUpdate.Dispose();
        }

        public void Dispose()
        {
            myZipArchive?.Dispose();
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
                // there can also be multiple entries of the same file in a zip file, which is an error case, but should be handled 
                var foundEntries = ZipArchiveUpdate.Entries.Where(x => x.FullName.Equals(relativePath + zipCoverEntry));
                if (foundEntries.Count() == 0)
                {
                    throw new EbookerException($"Could not find referenced cover file with href <{zipCoverEntry}> in zipfile <{myFileLocation}>.");
                }

                foreach (var entry in foundEntries.ToArray())
                { 
                    entry.Delete();
                }
            }
        }

        // The returned object has a property "FullName" which contains the relative path
        // of the file, e.g. OEPBS/content.opf
        // and "Name", which would then be "content.opf".
        internal ZipArchiveEntry GetOpf(bool writable=false)
        {
            ZipArchive archive;
            ZipArchiveEntry? opfFile;
            try
            {
                archive = writable ? ZipArchiveUpdate : ZipArchiveRead;
                opfFile = archive?.Entries?.SingleOrDefault(
                   x => x.Name.EndsWith(".opf", StringComparison.CurrentCultureIgnoreCase));

                if (opfFile == null)
                {
                    throw new EbookerException($"Could not find opf file in ebook <{myFileLocation}>.");
                }

                return opfFile;
            }
            catch (InvalidOperationException ex) 
            {
                throw new EbookerException($"Seems there are multiple opf file in ebook <{myFileLocation}>. See inner exception for more details.", ex);
            }
        }
    }
}
