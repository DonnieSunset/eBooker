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
    public class Cover
    {
        public MemoryStream GetImage(ZipArchive zipArchive)
        {
            try
            {
                //using var zipArchive = ZipFile.OpenRead(fileLoction);

                var jpgFile = GetCoverFileByOpf(zipArchive);
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

        public static MemoryStream GetMemoryStreamFromFile(string fileLocation)
        {
            try
            {
                FileStream fileStream = new FileStream(fileLocation, FileMode.Open, FileAccess.Read);
                Byte[] byteArray = new Byte[fileStream.Length];
                fileStream.Read(byteArray, 0, byteArray.Length);
                fileStream.Close();

                return new MemoryStream(byteArray);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //private ZipArchiveEntry? IdentifyCoverFileWithFallbacks(ZipArchive zipArchive)
        //{
        //    //ZipArchiveEntry? result = IdentifyCoverFileByCoverJpeg(zipArchive);
        //    //if (result == null)
        //    //{
        //    //    result = IdentifyCoverFileByCoverHtml(zipArchive);
        //    //    if (result == null)
        //    //    {
        //    //        result = IdentifyCoverFileBySingularJpeg(zipArchive);
        //    //        if (result == null)
        //    //        {
        //    //            result = IdentfyCoverFileByOpf(zipArchive);
        //    //        }
        //    //    }
        //    //}

        //    ZipArchiveEntry? result = IdentfyCoverFileByOpf(zipArchive);

        //    return result;
        //}


        ///// <summary>
        ///// Easy case, there is only one jpeg file in the whole zip
        ///// so we assume that must be the cover file, even if
        ///// its name might be totally misleading.
        ///// </summary>
        //private ZipArchiveEntry? IdentifyCoverFileBySingularJpeg(ZipArchive zipArchive)
        //{
        //    try
        //    {
        //        return zipArchive.Entries.SingleOrDefault(
        //            x =>
        //            x.Name.EndsWith("jpg", StringComparison.CurrentCultureIgnoreCase) ||
        //            x.Name.EndsWith("jpeg", StringComparison.CurrentCultureIgnoreCase));
        //    }
        //    catch 
        //    {
        //        // more than 1 results
        //        return null; 
        //    }
        //}

        ///// <summary>
        ///// Easy case, the cover file is in the named cover.jpg or cover.jpeg
        ///// and its saved directly in the zip file.
        ///// </summary>
        //private ZipArchiveEntry? IdentifyCoverFileByCoverJpeg(ZipArchive zipArchive)
        //{
        //    try
        //    {
        //        return zipArchive.Entries.SingleOrDefault(
        //            x =>
        //            (x.Name.EndsWith("jpg", StringComparison.CurrentCultureIgnoreCase) ||
        //            x.Name.EndsWith("jpeg", StringComparison.CurrentCultureIgnoreCase))
        //            &&
        //            x.Name.Contains("cover", StringComparison.CurrentCultureIgnoreCase));
        //    }
        //    catch
        //    {
        //        // more than 1 results
        //        return null;
        //    }
        //}

        ///// <summary>
        ///// More difficult case, the cover file referenced by cover.html
        ///// </summary>
        //private ZipArchiveEntry? IdentifyCoverFileByCoverHtml(ZipArchive zipArchive)
        //{
        //    var coverHtmlPages = zipArchive.Entries.Where(
        //        x =>
        //        x.Name.Equals("cover.html", StringComparison.CurrentCultureIgnoreCase) ||
        //        x.Name.Equals("cover.xhtml", StringComparison.CurrentCultureIgnoreCase) ||
        //        //x.Name.Equals("title.html", StringComparison.CurrentCultureIgnoreCase) ||
        //        //x.Name.Equals("title.xhtml", StringComparison.CurrentCultureIgnoreCase) ||
        //        x.Name.Equals("titlepage.html", StringComparison.CurrentCultureIgnoreCase) ||
        //        x.Name.Equals("titlepage.xhtml", StringComparison.CurrentCultureIgnoreCase));

        //    foreach (var coverHtml in coverHtmlPages)
        //    {
        //        var coverHtmlStream = coverHtml.Open();
        //        StreamReader reader = new StreamReader(coverHtmlStream);
        //        string htmlString = reader.ReadToEnd();

        //        var coverFile = GetImageSourceFromHtml(htmlString);
        //        if (coverFile != null)
        //        {
        //            // sometimes the references look like
        //            // <image width="487" height="800" xlink:href="../Images/cover.jpg"/>
        //            // then we have to cut of the "../" from the path
        //            coverFile = coverFile.Replace("../", "");

        //            var coverJpg = zipArchive.Entries.FirstOrDefault(
        //                x => x.FullName.EndsWith(coverFile, StringComparison.CurrentCultureIgnoreCase));

        //            return coverJpg;
        //        }
        //    }

        //    return null;
        //}

        // <package ...>
        //  <metadata ...>
        //    <meta name="cover" content="cover"/>
        //  </metadata>
        //  <manifest>
        //    <item href="cover.jpg" id="cover" media-type="image/jpeg"/>
        //  </manifest>
        // </package>
        internal ZipArchiveEntry? GetCoverFileByOpf(ZipArchive zipArchive)
        {
            var opfFile = zipArchive.Entries.FirstOrDefault(
                   x => x.Name.EndsWith(".opf", StringComparison.CurrentCultureIgnoreCase));

            if (opfFile != null)
            {
                var opfStream = opfFile.Open();
                XDocument xmlDoc = XDocument.Load(opfStream);
                var xmlRoot = xmlDoc.Root;

                string coverLinkId = GetCoverEntryFromMetaDataSection(xmlDoc);
                var coverLink = GetCoverLinkFromMetaDataSection(xmlDoc, coverLinkId);

                if (coverLink != null)
                {
                    return zipArchive.Entries.FirstOrDefault(
                        x => x.FullName.EndsWith(coverLink, StringComparison.CurrentCultureIgnoreCase));
                }
            }

            return null;
        }

        string GetCoverEntryFromMetaDataSection(XDocument opfDoc)
        {
            var xmlRoot = opfDoc.Root;
            try
            {
                var xmlMetaData = xmlRoot?.Elements().SingleOrDefault(x => x.Name.LocalName == "metadata");
                var coverLinkElement = xmlMetaData?.Elements().SingleOrDefault(x =>
                    x.Name.LocalName == "meta" &&
                    x.Attributes("name").Count() == 1 &&
                    x.Attribute("name").Value.Equals("cover", StringComparison.CurrentCultureIgnoreCase)
                    );
                var coverLinkId = coverLinkElement?.Attribute("content").Value;
                return coverLinkId;
            }
            catch (InvalidOperationException ex)
            {
                throw new EbookerException($"Ebook seems to contain more than one cover entry in its meta data.", ex);
            }
        }

        //string DeleteCoverEntriesFromMetaDataSection(XDocument opfDoc)
        //{
        //    var xmlRoot = opfDoc.Root;
        //    var xmlMetaData = xmlRoot?.Elements().SingleOrDefault(x => x.Name.LocalName == "metadata");
        //    xmlMetaData?.Elements().Where(x =>
        //        x.Name.LocalName == "meta" &&
        //        x.Attributes("name").Count() == 1 &&
        //        x.Attribute("name").Value.Equals("cover", StringComparison.CurrentCultureIgnoreCase)
        //        ).Remove();
                
        //}


        string GetCoverLinkFromMetaDataSection(XDocument opfDoc, string coverLinkId)
        {
            var xmlRoot = opfDoc.Root;
            var xmlManifest = xmlRoot?.Elements().SingleOrDefault(x => x.Name.LocalName == "manifest");
            var coverElement = xmlManifest?.Elements().SingleOrDefault(x =>
                x.Name.LocalName == "item" &&
                x.Attributes("id").Count() == 1 &&
                x.Attribute("id").Value == coverLinkId &&
                x.Attributes("media-type").Count() == 1 &&
                x.Attribute("media-type").Value == "image/jpeg"
                );
            var coverLink = coverElement?.Attribute("href").Value;

            return coverLink;
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
