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
    internal class Cover
    {
        public MemoryStream GetImage(ZipArchive zipArchive)
        {
            try
            {
                //using var zipArchive = ZipFile.OpenRead(fileLoction);

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
                    if (result == null)
                    {
                        result = IdentfyCoverFileByOpf(zipArchive);
                    }
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
            try
            {
                return zipArchive.Entries.SingleOrDefault(
                    x =>
                    x.Name.EndsWith("jpg", StringComparison.CurrentCultureIgnoreCase) ||
                    x.Name.EndsWith("jpeg", StringComparison.CurrentCultureIgnoreCase));
            }
            catch 
            {
                // more than 1 results
                return null; 
            }
        }

        /// <summary>
        /// Easy case, the cover file is in the named cover.jpg or cover.jpeg
        /// and its saved directly in the zip file.
        /// </summary>
        private ZipArchiveEntry? IdentifyCoverFileByCoverJpeg(ZipArchive zipArchive)
        {
            try
            {
                return zipArchive.Entries.SingleOrDefault(
                    x =>
                    (x.Name.EndsWith("jpg", StringComparison.CurrentCultureIgnoreCase) ||
                    x.Name.EndsWith("jpeg", StringComparison.CurrentCultureIgnoreCase))
                    &&
                    x.Name.Contains("cover", StringComparison.CurrentCultureIgnoreCase));
            }
            catch
            {
                // more than 1 results
                return null;
            }
        }

        /// <summary>
        /// More difficult case, the cover file referenced by cover.html
        /// </summary>
        private ZipArchiveEntry? IdentifyCoverFileByCoverHtml(ZipArchive zipArchive)
        {
            var coverHtmlPages = zipArchive.Entries.Where(
                x =>
                x.Name.Equals("cover.html", StringComparison.CurrentCultureIgnoreCase) ||
                x.Name.Equals("cover.xhtml", StringComparison.CurrentCultureIgnoreCase) ||
                //x.Name.Equals("title.html", StringComparison.CurrentCultureIgnoreCase) ||
                //x.Name.Equals("title.xhtml", StringComparison.CurrentCultureIgnoreCase) ||
                x.Name.Equals("titlepage.html", StringComparison.CurrentCultureIgnoreCase) ||
                x.Name.Equals("titlepage.xhtml", StringComparison.CurrentCultureIgnoreCase));

            foreach (var coverHtml in coverHtmlPages)
            {
                var coverHtmlStream = coverHtml.Open();
                StreamReader reader = new StreamReader(coverHtmlStream);
                string htmlString = reader.ReadToEnd();

                var coverFile = GetImageSourceFromHtml(htmlString);
                if (coverFile != null)
                {
                    // sometimes the references look like
                    // <image width="487" height="800" xlink:href="../Images/cover.jpg"/>
                    // then we have to cut of the "../" from the path
                    coverFile = coverFile.Replace("../", "");

                    var coverJpg = zipArchive.Entries.FirstOrDefault(
                        x => x.FullName.EndsWith(coverFile, StringComparison.CurrentCultureIgnoreCase));

                    return coverJpg;
                }
            }

            return null;
        }

        private ZipArchiveEntry? IdentfyCoverFileByOpf(ZipArchive zipArchive)
        {
            var opfFile = zipArchive.Entries.FirstOrDefault(
               x => x.Name.Equals("content.opf", StringComparison.CurrentCultureIgnoreCase));

            // in some cases the opf files are named differently
            if (opfFile == null) 
            {
                opfFile = zipArchive.Entries.FirstOrDefault(
                   x => x.Name.EndsWith(".opf", StringComparison.CurrentCultureIgnoreCase));
            }

            if (opfFile != null)
            {
                var opfStream = opfFile.Open();
                XDocument xmlDoc1 = XDocument.Load(opfStream);
                var root = xmlDoc1.Root;
                var manifest = root?.Elements().SingleOrDefault(x => x.Name.LocalName == "manifest");

                var coverItem = manifest?.Elements().FirstOrDefault(x =>
                    // all media based items
                    x.Name.LocalName == "item" &&
                    x.Attribute("media-type") != null &&
                    x.Attribute("media-type").Value.Contains("image", StringComparison.CurrentCultureIgnoreCase) &&
                        // that have something with cover in the id attribute
                        (x.Attribute("id") != null &&
                        x.Attribute("id").Value.Contains("cover", StringComparison.CurrentCultureIgnoreCase) ||
                        // or in the properties attribute
                        (x.Attribute("properties") != null &&
                        x.Attribute("properties").Value.Contains("cover", StringComparison.CurrentCultureIgnoreCase)
                        )
                    )
                );

                string coverImageFileName = coverItem?.Attribute("href")?.Value;
                if (coverImageFileName != null)
                {
                    return zipArchive.Entries.FirstOrDefault(
                        x => x.FullName.EndsWith(coverImageFileName, StringComparison.CurrentCultureIgnoreCase));
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
                "<img.*src=\"(.+?)\"",
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
