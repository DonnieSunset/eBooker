using System.IO.Compression;

namespace BE
{
    public class eBook(string fileLocation)
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

        public void UpdateCover(string jpegFileLocation)
        { }

        public void GetMetaInformation()
        { 
        }

        public void UpdateMetaInformation()
        { }
    }
}
