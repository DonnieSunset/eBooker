using BE.MetaData;
using System.IO.Compression;

namespace BE
{
    public class eBook(string fileLocation) : IDisposable
    {
        private string myFileLocation = fileLocation;

        private ZipArchive myZipArchive = null;

        private MetaDataRecord MetaData = new MetaDataRecord();

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
                    Dispose();
                    myZipArchive = ZipFile.Open(myFileLocation, ZipArchiveMode.Update);
                }

                return myZipArchive;
            }
        }

        private ZipArchiveEntry? myOpfEntry = null;
        public ZipArchiveEntry OpfEntryRead
        {
            get
            {
                if (myOpfEntry == null)
                {
                    myOpfEntry = ReloadOpfFromArchive(ZipArchiveRead);
                }

                return myOpfEntry;
            }
        }

        public ZipArchiveEntry OpfEntryUpdate
        {
            get
            {
                if (myOpfEntry == null || myOpfEntry.Archive.Mode != ZipArchiveMode.Update)
                {
                    myOpfEntry = ReloadOpfFromArchive(ZipArchiveUpdate);
                }

                return myOpfEntry;
            }
        }

        public MemoryStream? GetCover()
        {
            if (MetaData.Cover == null)
            {
                this.MetaData.Cover = new Cover();
                this.MetaData.Cover.Read(ZipArchiveRead, OpfEntryRead);
            }

            return MetaData.Cover.Data;
        }

        public Tuple<Author, Author> GetAuthors()
        {
            if (MetaData.Authors == null)
            {
                MetaData.Authors = new Authors();
                MetaData.Authors.Read(OpfEntryRead);
            }

            return MetaData.Authors.Data;
        }

        public void UpdateCover(string coverFileLocation)
        {
            if (MetaData.Cover == null)
            {
                this.MetaData.Cover = new Cover();
            }
            
            this.MetaData.Cover.Write(ZipArchiveUpdate, OpfEntryUpdate, coverFileLocation);
            Dispose();
        }

        public void UpdateAuthors(Author? author1, Author? author2)
        {
            if (MetaData.Authors == null)
            {
                this.MetaData.Authors = new Authors();
            }
            MetaData.Authors.Write(OpfEntryUpdate, new Tuple<Author?, Author?>(author1, author2));
            Dispose();
        }


        public void Dispose()
        {
            myOpfEntry = null;
            myZipArchive?.Dispose();
            myZipArchive = null;
        }

        // todo: remove duplicate code
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
