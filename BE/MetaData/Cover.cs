using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BE.MetaData
{
    public class Cover : MetaDataBase<MemoryStream>
    {
        public void Read(ZipArchive archive, ZipArchiveEntry opfEntry)
        {
            using (var opfStream = opfEntry.Open())
            {
                XDocument xmlDoc = XDocument.Load(opfStream);

                string opfRelativePath = opfEntry.FullName.Replace(opfEntry.Name, string.Empty);
                string coverLocation = GetCoverArchiveLocation(xmlDoc, opfRelativePath);

                ZipArchiveEntry coverEntry = archive.Entries.FirstOrDefault(
                    x => x.FullName.Equals(coverLocation, StringComparison.OrdinalIgnoreCase));

                try
                {
                    if (coverEntry != null)
                    {
                        using (Stream jpgDeflateStream = coverEntry.Open())
                        {
                            MemoryStream memoryStream = new MemoryStream();
                            jpgDeflateStream.CopyTo(memoryStream);
                            this.Data = memoryStream;
                        }
                    }
                    else
                    {
                        this.Data = new MemoryStream();
                    }
                }
                catch
                {
                    this.Data = new MemoryStream();
                }
            }
        }

        public void Write(ZipArchive archive, ZipArchiveEntry opfEntry, string coverFileLocation)
        {
            using (var opfStream = opfEntry.Open())
            {
                // cover shall be placed always at the same folder level as the opf
                // such that we dont have to deal with relative paths
                string relativePathOfOpfFile = opfEntry.FullName.Replace(opfEntry.Name, string.Empty);
                string coverFileName = "cover.jpg";
                string coverLocationInsideArchive = relativePathOfOpfFile + coverFileName;

                XDocument xmlDoc = XDocument.Load(opfStream);

                var metaDataCoverIDs = RemoveCoverMetaEntries(xmlDoc);
                var existingCoverFiles = RemoveCoverManifestEntries(xmlDoc, metaDataCoverIDs);
                AddCoverEntry(xmlDoc, coverFileName);
                UpdateOpfInArchive(opfStream, xmlDoc);
                RemoveCoverFilesFromArchive(archive, relativePathOfOpfFile, existingCoverFiles);

                // sometimes the exact same cover file already exists even without being
                // referenced in the opf metadata
                TryRemoveCoverFilesFromArchive(archive, string.Empty, [coverLocationInsideArchive]);

                archive.CreateEntryFromFile(coverFileLocation, coverLocationInsideArchive);
            }
        }

        /// <summary>
        /// Internal for testing
        /// </summary>
        internal void AddCoverEntry(XDocument opfDocument, string coverHref)
        {
            var xmlRoot = opfDocument.Root;

            var xmlMetaData = xmlRoot?.Elements().SingleOrDefault(x => x.Name.LocalName == "metadata");
            xmlMetaData.Add(new XElement(xmlMetaData.GetDefaultNamespace() + "meta",
                new XAttribute("name", "cover"),
                new XAttribute("content", "coverID")
                ));

            var xmlManifest = xmlRoot?.Elements().SingleOrDefault(x => x.Name.LocalName == "manifest");
            xmlManifest.Add(new XElement(xmlManifest.GetDefaultNamespace() + "item",
                new XAttribute("href", coverHref),
                new XAttribute("id", "coverID"),
                new XAttribute("media-type", "image/jpeg")
                ));
        }

        private string? GetCoverArchiveLocation(XDocument opfDocument, string opfRelativePath)
        {
            string relativeCoverLink = string.Empty;

            List<string> opfMetaEntries = GetCoverMetaEntries(opfDocument);
            List<string> opfManifestEntries = GetCoverManifestEntries(opfDocument, opfMetaEntries);

            if (opfManifestEntries == null || opfManifestEntries.Count == 0)
            {
                return null;
            }
            else if (opfManifestEntries.Count > 1)
            {
                throw new EbookerException($"Could not identify unique cover entries. " +
                    $"Candidates are: {Environment.NewLine} {String.Join(Environment.NewLine, opfManifestEntries)}");
            }
            else
            {
                string coverLink = opfManifestEntries.Single();
                relativeCoverLink = opfRelativePath + coverLink;
            }

            return relativeCoverLink;
        }

        private List<string> GetCoverMetaEntries(XDocument opfDocument)
        {
            var coverEntries = IdentifyCoverMetaEntries(opfDocument);
            return coverEntries.Select(x => x.Attribute("content").Value).ToList();
        }

        private List<string> GetCoverManifestEntries(XDocument opfDocument, List<string> iDs)
        {
            var coverEntries = IdentifyCoverManifestEntries(opfDocument, iDs);
            return coverEntries.Select(x => x.Attribute("href").Value).ToList();
        }

        private void RemoveCoverFilesFromArchive(ZipArchive archive, string relativePath, List<string> zipCoverEntries)
        {
            foreach (string zipCoverEntry in zipCoverEntries)
            {
                string completePath = relativePath + zipCoverEntry;

                //remove relative entries, e.g. OEBPS/images/../cover.jpeg should result to OEBPS/cover.jpeg
                completePath = ResolveDirectoryJumps(completePath);

                // there can also be multiple entries of the same file in a zip file, which is an error case, but should be handled 
                var foundEntries = archive.Entries.Where(x => x.FullName.Equals(completePath));
                if (foundEntries.Count() == 0)
                {
                    throw new EbookerException($"Could not find referenced cover file with href <{completePath}>.");
                }

                foreach (var entry in foundEntries.ToArray())
                {
                    entry.Delete();
                }
            }
        }

        private bool TryRemoveCoverFilesFromArchive(ZipArchive archive, string relativePath, List<string> zipCoverEntries)
        {
            try
            {
                RemoveCoverFilesFromArchive(archive, relativePath, zipCoverEntries);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Internal for testing
        /// </summary>
        internal List<string> RemoveCoverMetaEntries(XDocument opfDocument)
        {
            var coverEntries = IdentifyCoverMetaEntries(opfDocument);

            var resultContentItems = coverEntries.Select(x => x.Attribute("content").Value).ToList();
            coverEntries.Remove();

            return resultContentItems;
        }

        /// <summary>
        /// Internal for testing
        /// </summary>
        internal List<string> RemoveCoverManifestEntries(XDocument opfDocument, List<string> iDs)
        {
            var coverEntries = IdentifyCoverManifestEntries(opfDocument, iDs);

            var resultContentItems = coverEntries.Select(x => x.Attribute("href").Value).ToList();
            coverEntries.Remove();

            return resultContentItems;
        }

        private IEnumerable<XElement?> IdentifyCoverMetaEntries(XDocument opfDocument)
        {
            var xmlRoot = opfDocument.Root;
            var xmlMetaData = xmlRoot?.Elements().SingleOrDefault(x => x.Name.LocalName == "metadata");

            var coverEntries = xmlMetaData?.Elements().Where(x =>
                x.Name.LocalName == "meta" &&
                x.Attribute("name") != null &&
                x.Attribute("name")!.Value.Equals("cover", StringComparison.OrdinalIgnoreCase)
                );

            return coverEntries;
        }

        private IEnumerable<XElement?> IdentifyCoverManifestEntries(XDocument opfDocument, List<string> iDs)
        {
            var xmlRoot = opfDocument.Root;
            var xmlMetaData = xmlRoot?.Elements().SingleOrDefault(x => x.Name.LocalName == "manifest");

            var coverEntries = xmlMetaData?.Elements().Where(x =>
                x.Name.LocalName == "item" &&
                x.Attribute("id") != null &&
                iDs.Contains(x.Attribute("id")!.Value)
                );

            return coverEntries;
        }

        private string ResolveDirectoryJumps(string inputPath)
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
    }
}
