using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            List<string> resultContentItems = new();

            var xmlRoot = opfDocument.Root;
            var xmlMetaData = xmlRoot?.Elements().SingleOrDefault(x => x.Name.LocalName == "metadata");

            var coverEntries = xmlMetaData?.Elements().Where(x =>
                x.Name.LocalName == "meta" &&
                x.Attribute("name") != null &&
                x.Attribute("name").Value.Equals("cover", StringComparison.InvariantCultureIgnoreCase)
                );

            resultContentItems = coverEntries.Select(x => x.Attribute("content").Value).ToList();
            coverEntries.Remove();

            return resultContentItems;
        }

        public static void RemoveCoverManifestEntries(XDocument opfDocument, List<string> iDs)
        {
            List<string> resultContentItems = new();

            var xmlRoot = opfDocument.Root;
            var xmlMetaData = xmlRoot?.Elements().SingleOrDefault(x => x.Name.LocalName == "manifest");

            var coverEntries = xmlMetaData?.Elements().Where(x =>
                x.Name.LocalName == "item" &&
                x.Attribute("id") != null &&
                iDs.Contains(x.Attribute("id").Value)
                );
            
            coverEntries.Remove();
        }
    }
}
