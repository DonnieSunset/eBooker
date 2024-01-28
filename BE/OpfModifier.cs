using System.Xml.Linq;

namespace BE
{
    // Structure of an Opf file
    // 
    // <package ...>
    //  <metadata ...>
    //    <meta name="cover" content="cover"/>
    //  </metadata>
    //  <manifest>
    //    <item href="cover.jpg" id="cover" media-type="image/jpeg"/>
    //  </manifest>
    // </package>
    public static class OpfModifier
    {
        public static List<string> RemoveCoverMetaEntries(XDocument opfDocument)
        {
            var coverEntries = IdentifyCoverMetaEntries(opfDocument);

            var resultContentItems = coverEntries.Select(x => x.Attribute("content").Value).ToList();
            coverEntries.Remove();

            return resultContentItems;
        }

        public static List<string> GetCoverMetaEntries(XDocument opfDocument)
        {
            var coverEntries = IdentifyCoverMetaEntries(opfDocument);
            return coverEntries.Select(x => x.Attribute("content").Value).ToList();
        }

        public static List<string> RemoveCoverManifestEntries(XDocument opfDocument, List<string> iDs)
        {
            var coverEntries = IdentifyCoverManifestEntries(opfDocument, iDs);

            var resultContentItems = coverEntries.Select(x => x.Attribute("href").Value).ToList();
            coverEntries.Remove();

            return resultContentItems;
        }

        public static List<string> GetCoverManifestEntries(XDocument opfDocument, List<string> iDs)
        {
            var coverEntries = IdentifyCoverManifestEntries(opfDocument, iDs);
            return coverEntries.Select(x => x.Attribute("href").Value).ToList();
        }

        public static void AddCoverEntry(XDocument opfDocument, string coverHref)
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

        public static List<string> GetAuthors(XDocument opfDocument)
        {
            var (xmlMetaData, opfNamespace, _) = GetMetaDataSection(opfDocument);
            return GetAuthorEntries(xmlMetaData, opfNamespace)
                .Select(x => x.Value)
                .ToList();
        }

        /// <summary>
        /// Set the author in the meta data section of a opf file
        /// </summary>
        /// <exception cref="EbookerException">Thrown if more than one author entry is already existing.</exception>
        /// <remarks>
        /// Format of the author entry:
        /// <dc:creator opf:role="aut" opf:file-as="Kristof, Agota">Kristof, Agota</dc:creator>
        /// </remarks>
        public static void SetAuthor(XDocument opfDocument, string author)
        {
            var (xmlMetaData, opfNamespace, dcNamespace) = GetMetaDataSection(opfDocument);

            foreach (var existingEntry in GetAuthorEntries(xmlMetaData, opfNamespace))
            { 
                existingEntry.Remove();
            }

            var newElement = new XElement(dcNamespace + "creator",
                        new XAttribute(opfNamespace + "role", "aut"),
                        new XAttribute(opfNamespace + "file-as", author),
                        author);

            xmlMetaData.Add(newElement);
        }

        private static (XElement, XNamespace, XNamespace) GetMetaDataSection(XDocument opfDocument)
        {
            var xmlRoot = opfDocument.Root;
            if (xmlRoot == null)
            {
                throw new EbookerException("Root element in opf document not found.");
            }

            var xmlMetaData = xmlRoot.Elements().SingleOrDefault(x => x.Name.LocalName == "metadata");
            if (xmlMetaData == null)
            {
                throw new EbookerException("Metadata section in opf document not found.");
            }

            XNamespace? opfNamespace = xmlMetaData.GetNamespaceOfPrefix("opf");
            if (opfNamespace == null)
            {
                opfNamespace = @"http://www.idpf.org/2007/opf";
            }

            XNamespace? dcNamespace = xmlMetaData.GetNamespaceOfPrefix("dc");
            if (dcNamespace == null)
            {
                dcNamespace = @"http://purl.org/dc/elements/1.1/";
            }

            return (xmlMetaData, opfNamespace, dcNamespace);
        }

        private static IEnumerable<XElement> GetAuthorEntries(XElement xmlMetaData, XNamespace opfNamespace) 
        {
            return xmlMetaData.Elements().Where(x =>
                    x.Name.LocalName == "creator" &&
                    x.Attribute(opfNamespace + "role") != null &&
                    x.Attribute(opfNamespace + "role")!.Value.Equals("aut", StringComparison.OrdinalIgnoreCase)
                );        }

        private static IEnumerable<XElement?> IdentifyCoverMetaEntries(XDocument opfDocument)
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

        private static IEnumerable<XElement?> IdentifyCoverManifestEntries(XDocument opfDocument, List<string> iDs)
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
    }
}
