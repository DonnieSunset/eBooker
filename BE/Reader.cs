using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace BE
{
    // todo: change firstOrDefault -> singleOrDefault
    public class Reader
    {
        internal const string ImageSourceRegex = "<img src=\"(.+?)\"";

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
                using var zipArchive = ZipFile.OpenRead(fileLoction);

                var jpgFile = IdentifyCoverFile(zipArchive);
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

        private ZipArchiveEntry? IdentifyCoverFile(ZipArchive zipArchive)
        {
            ZipArchiveEntry? result = IdentifyCoverFileByCoverJpeg(zipArchive);
            if (result == null)
            {
                result = IdentifyCoverFileByCoverHtml(zipArchive);
            }

            return result;
        }


        /// <summary>
        /// Easiest case, the cover file is in the named cover.jpg or cover.jpeg
        /// and its saved directly in the zip file.
        /// </summary>
        private ZipArchiveEntry? IdentifyCoverFileByCoverJpeg(ZipArchive zipArchive)
        {
            return zipArchive.Entries.FirstOrDefault(
                x => x.Name.EndsWith("cover.jpg", StringComparison.CurrentCultureIgnoreCase) || 
                x.Name.EndsWith("cover.jpeg", StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// More difficult case, the cover file referenced by cover.html
        /// </summary>
        private ZipArchiveEntry? IdentifyCoverFileByCoverHtml(ZipArchive zipArchive)
        {
            var coverHtml = zipArchive.Entries.FirstOrDefault(
                x => x.Name.EndsWith("cover.html", StringComparison.CurrentCultureIgnoreCase));
            if (coverHtml != null)
            {
                var coverHtmlStream = coverHtml.Open();
                StreamReader reader = new StreamReader(coverHtmlStream);
                string htmlString = reader.ReadToEnd();

                var coverFile = GetImageSourceFromHtml(htmlString);
                if (coverFile != null)
                {
                    var coverJpg = zipArchive.Entries.FirstOrDefault(
                        x => x.Name.EndsWith(coverFile, StringComparison.CurrentCultureIgnoreCase));

                    return coverJpg;
                }
            }

            return null;
        }

        /// <summary>
        /// Sometimes the cover image is embedded in a html file like this:
        /// 
        /// <head>
        /// <title>Cover</title>
        /// <meta http-equiv="Content-Type" content="text/html; charset=utf-8" /></head>
        /// <body>
        /// <center><img src = "Image00008.jpg" style="width:100%;height:100%;" />
        /// </center>
        /// </body>
        /// </html>
        ///
        /// Then we have to extract "Image00008.jpg" from it
        /// </summary>
        /// <param name="htmlString">The html string</param>
        /// <returns>The cover image file if existing, null otherwise.</returns>
        internal string? GetImageSourceFromHtml(string htmlString)
        {
            Match result = Regex.Match(htmlString, Reader.ImageSourceRegex);

            if (result.Success &&
                result.Groups.Count == 2 &&
                result.Groups[1].Success)
            { 
                return result.Groups[1].Value;
            }

            return null;
        }
    }
}
