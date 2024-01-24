using System.IO.Compression;

namespace BE
{
    public class Cover
    {
        public MemoryStream GetImage(ZipArchiveEntry? coverEntry)
        {
            try
            {
                if (coverEntry != null)
                {
                    using (Stream jpgDeflateStream = coverEntry.Open())
                    {

                        MemoryStream memoryStream = new MemoryStream();
                        jpgDeflateStream.CopyTo(memoryStream);

                        return memoryStream;
                    }
                }
                else
                {
                    return new MemoryStream();
                }
            }
            catch
            {
                return new MemoryStream();
            }
        }

        public static MemoryStream? GetMemoryStreamFromFile(string fileLocation)
        {
            try
            {
                FileStream fileStream = new FileStream(fileLocation, FileMode.Open, FileAccess.Read);
                Byte[] byteArray = new Byte[fileStream.Length];
                fileStream.Read(byteArray, 0, byteArray.Length);
                fileStream.Close();

                return new MemoryStream(byteArray);
            }
            catch
            {
                return null;
            }
        }
    }
}
