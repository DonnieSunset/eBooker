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
