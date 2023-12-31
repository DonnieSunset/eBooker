using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace BE
{
    public class Reader
    {

        //public IEnumerable<MemoryStream> GetImages()
        //{
        //    string ebookFolder = @"P:\Ebooks\Romane";
        //    IEnumerable<MemoryStream> result = new List<MemoryStream>();

        //    foreach (var fileLoction in Directory.GetFiles(ebookFolder, "*", SearchOption.AllDirectories))
        //    {
        //        yield return GetImage(fileLoction);
        //    }
        //}

        public MemoryStream GetImage(string fileLoction)
        {
            try
            {
                using var zipFile = ZipFile.OpenRead(fileLoction);

                var jpgFile = zipFile.Entries.FirstOrDefault(x => x.Name.EndsWith("cover.jpg", StringComparison.CurrentCultureIgnoreCase) || x.Name.EndsWith("cover.jpeg", StringComparison.CurrentCultureIgnoreCase));

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
                    return new MemoryStream();
                }
            }
            catch
            {
                return new MemoryStream();
            }
        }
    }
}
