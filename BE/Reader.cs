using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace BE
{
    // todo: change firstOrDefault -> singleOrDefault
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
                using var zipArchive = ZipFile.OpenRead(fileLoction);

                var jpgFile = IdentifyCoverFileWithFallbacks(zipArchive);
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

        private ZipArchiveEntry? IdentifyCoverFileWithFallbacks(ZipArchive zipArchive)
        {
            ZipArchiveEntry? result = IdentifyCoverFileByCoverJpeg(zipArchive);
            if (result == null)
            {
                result = IdentifyCoverFileByCoverHtml(zipArchive);
                if (result == null)
                {
                    result = IdentifyCoverFileBySingularJpeg(zipArchive);
                }
            }

            return result;
        }


        /// <summary>
        /// Easy case, there is only one jpeg file in the whole zip
        /// so we assume that must be the cover file, even if
        /// its name might be totally misleading.
        /// </summary>
        private ZipArchiveEntry? IdentifyCoverFileBySingularJpeg(ZipArchive zipArchive)
        {
            return zipArchive.Entries.SingleOrDefault(
                x =>
                x.Name.EndsWith("jpg", StringComparison.CurrentCultureIgnoreCase) ||
                x.Name.EndsWith("jpeg", StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Easy case, the cover file is in the named cover.jpg or cover.jpeg
        /// and its saved directly in the zip file.
        /// </summary>
        private ZipArchiveEntry? IdentifyCoverFileByCoverJpeg(ZipArchive zipArchive)
        {
            return zipArchive.Entries.SingleOrDefault(
                x => 
                (x.Name.EndsWith("jpg", StringComparison.CurrentCultureIgnoreCase) || 
                x.Name.EndsWith("jpeg", StringComparison.CurrentCultureIgnoreCase))
                &&
                x.Name.Contains("cover", StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// More difficult case, the cover file referenced by cover.html
        /// </summary>
        private ZipArchiveEntry? IdentifyCoverFileByCoverHtml(ZipArchive zipArchive)
        {
            var coverHtml = zipArchive.Entries.FirstOrDefault(
                x => 
                x.Name.EndsWith("cover.html", StringComparison.CurrentCultureIgnoreCase) ||
                x.Name.EndsWith("cover.xhtml", StringComparison.CurrentCultureIgnoreCase));

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
            var imageSourceRegexes = new List<string>
            {
                "<img src=\"(.+?)\"",
                "<image.*xlink:href=\"(.+?)\".*/>"
            };

            foreach (var regex in imageSourceRegexes)
            {
                Match result = Regex.Match(htmlString, regex);

                if (result.Success &&
                    result.Groups.Count == 2 &&
                    result.Groups[1].Success)
                {
                    return result.Groups[1].Value;
                }
            }

            return null;
        }
    }
}
