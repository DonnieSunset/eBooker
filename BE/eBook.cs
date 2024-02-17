using BE.MetaData;
using System.IO.Compression;

namespace BE
{
    public class eBook(string fileLocation) : IDisposable
    {
        private string _fileLocation = fileLocation;

        private ZipArchive _zipArchive = null;

        private MetaDataRecord _metaData = new MetaDataRecord();

        internal ZipArchive ZipArchiveRead
        {
            get
            {
                if (_zipArchive == null)
                {
                    _zipArchive = ZipFile.OpenRead(_fileLocation);
                }

                return _zipArchive;
            }
        }

        internal ZipArchive ZipArchiveUpdate
        {
            get
            {
                if (_zipArchive == null)
                {
                    _zipArchive = ZipFile.Open(_fileLocation, ZipArchiveMode.Update);
                }
                else if (_zipArchive.Mode != ZipArchiveMode.Update)
                {
                    Dispose();
                    _zipArchive = ZipFile.Open(_fileLocation, ZipArchiveMode.Update);
                }

                return _zipArchive;
            }
        }

        private ZipArchiveEntry? _opfEntry = null;
        public ZipArchiveEntry OpfEntryRead
        {
            get
            {
                if (_opfEntry == null)
                {
                    _opfEntry = ReloadOpfFromArchive(ZipArchiveRead);
                }

                return _opfEntry;
            }
        }

        public ZipArchiveEntry OpfEntryUpdate
        {
            get
            {
                if (_opfEntry == null || _opfEntry.Archive.Mode != ZipArchiveMode.Update)
                {
                    _opfEntry = ReloadOpfFromArchive(ZipArchiveUpdate);
                }

                return _opfEntry;
            }
        }

        public MemoryStream? GetCover()
        {
            if (_metaData.Cover == null)
            {
                this._metaData.Cover = new Cover();
                this._metaData.Cover.Read(ZipArchiveRead, OpfEntryRead);
            }

            return _metaData.Cover.Data;
        }

        public Tuple<Author, Author> GetAuthors()
        {
            if (_metaData.Authors == null)
            {
                _metaData.Authors = new Authors();
                _metaData.Authors.Read(OpfEntryRead);
            }

            return _metaData.Authors.Data;
        }

        public string? GetTitle()
        {
            if (_metaData.Title == null)
            {
                _metaData.Title = new Title();
                _metaData.Title.Read(OpfEntryRead);
            }

            return _metaData.Title.Data;
        }

        public void UpdateCover(string coverFileLocation)
        {
            if (_metaData.Cover == null)
            {
                this._metaData.Cover = new Cover();
            }
            
            this._metaData.Cover.Write(ZipArchiveUpdate, OpfEntryUpdate, coverFileLocation);
            Dispose();
        }

        public void UpdateAuthors(Author? author1, Author? author2)
        {
            if (_metaData.Authors == null)
            {
                this._metaData.Authors = new Authors();
            }
            _metaData.Authors.Write(OpfEntryUpdate, new Tuple<Author?, Author?>(author1, author2));
            Dispose();
        }

        public void UpdateTitle(string title)
        {
            if (_metaData.Title == null)
            {
                this._metaData.Title = new Title();
            }
            _metaData.Title.Write(OpfEntryUpdate, title);
            Dispose();
        }


        public void Dispose()
        {
            _opfEntry = null;
            _zipArchive?.Dispose();
            _zipArchive = null;
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
                _opfEntry = zipArchive.Entries?.SingleOrDefault(
                   x => x.Name.EndsWith(".opf", StringComparison.CurrentCultureIgnoreCase));

                if (_opfEntry == null)
                {
                    throw new EbookerException($"Could not find opf file in ebook <{_fileLocation}>.");
                }

                return _opfEntry;
            }
            catch (InvalidOperationException ex)
            {
                throw new EbookerException($"Seems there are multiple opf file in ebook <{_fileLocation}>. See inner exception for more details.", ex);
            }
        }
    }
}
