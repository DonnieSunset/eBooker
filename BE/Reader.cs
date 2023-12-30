using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace BE
{
    public class Reader
    {

        public MemoryStream GetImage()
        {
            string ebookFolder = @"P:\Ebooks\Romane";
            string fileName = @"C:\temp\Steinbeck, John - 1932 - Eine Handvoll Gold.epub";

            //foreach (var fileName in Directory.GetFiles(ebookFolder, "*", SearchOption.AllDirectories))
            //{

            using var zipFile = ZipFile.OpenRead(fileName);

            var jpgFile = zipFile.Entries.SingleOrDefault(x => x.Name.EndsWith("cover.jpg", StringComparison.CurrentCultureIgnoreCase) || x.Name.EndsWith("cover.jpeg", StringComparison.CurrentCultureIgnoreCase));

            MemoryStream decompressedMemoryStream = new MemoryStream();

            if (jpgFile != null)
            {
                using (Stream jpgDeflateStream = jpgFile.Open())
                {

                    MemoryStream memoryStream = new MemoryStream();
                    jpgDeflateStream.CopyTo(memoryStream);

                    return memoryStream;

                }
            }
            else
            {
                throw new Exception("something went wrong.");
            }

            //}
        }
    }
}
