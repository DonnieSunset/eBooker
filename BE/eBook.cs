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

        public void UpdateCover(string jpegFileLocation)
        { }

        public void GetMetaInformation()
        { 
        }

        public void UpdateMetaInformation()
        { }


        public void SetCover(string coverFileLocation)
        {
            var opfFile = GetFromArchiveOpf(writable:true);

            WriteCoverMetaDataInOpf(opfFile);

            WriteCoverFileToArchive(coverFileLocation);

            //Important, otherwise it will not get saved
            ZipArchiveUpdate.Dispose();
        }

        public void Dispose()
        {
            myZipArchive?.Dispose();
        }


        // We have to write the following into the opf:
        // <package ...>
        //  <metadata ...>
        //    <meta name="cover" content="cover"/>
        //  </metadata>
        //  <manifest>
        //    <item href="cover.jpg" id="cover" media-type="image/jpeg"/>
        //  </manifest>
        // </package>
        private void WriteCoverMetaDataInOpf(ZipArchiveEntry opfFile)
        {
            var cover = new Cover();

            //this is the new logic
            if (cover.HasCoverEntryInOpf(opfFile))
            {
                cover.DeleteCoverEntryInOpf(opfFile);
                cover.AddCoverEntryInpf(opfFile);
            }

            using (var opfStream = opfFile.Open())
            {
                XDocument xmlDoc = XDocument.Load(opfStream);
                var xmlRoot = xmlDoc.Root;

                var xmlMetaData = xmlRoot?.Elements().SingleOrDefault(x => x.Name.LocalName == "metadata");
                xmlMetaData.Add(new XElement(xmlMetaData.GetDefaultNamespace() + "meta",
                    new XAttribute("name", "cover"),
                    new XAttribute("content", "cover")
                    ));

                var xmlManifest = xmlRoot?.Elements().SingleOrDefault(x => x.Name.LocalName == "manifest");
                xmlManifest.Add(new XElement(xmlManifest.GetDefaultNamespace() + "item",
                    new XAttribute("href", "cover.jpg"),
                    new XAttribute("id", "cover"),
                    new XAttribute("media-type", "image/jpeg")
                    ));

                //write it back
                opfStream.Position = 0;
                using (StreamWriter writer = new StreamWriter(opfStream))
                {
                    writer.Write(xmlDoc);
                }
            }

            // todo: add relative path to cover file
            // todo: overwrite existing tags
        }

        public void WriteCoverFileToArchive(string coverFileLocation)
        {
            ZipArchiveUpdate.CreateEntryFromFile(coverFileLocation, "cover.jpg");

            // todo: add relative path to cover file
        }

        // The returned object has a property "FullName" which contains the relative path
        // of the file, e.g. OEPBS/content.opf
        // and "Name", which would then be "content.opf".
        internal ZipArchiveEntry GetFromArchiveOpf(bool writable=false)
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
