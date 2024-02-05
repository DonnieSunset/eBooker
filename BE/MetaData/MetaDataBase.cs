using System.Xml.Linq;

namespace BE.MetaData
{
    public class MetaDataBase
    {
        protected (XElement, XNamespace, XNamespace) GetMetaDataSection(XDocument opfDocument)
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

        protected void UpdateOpfInArchive(Stream opfStream, XDocument xmlDoc)
        {
            opfStream.Position = 0;
            using (StreamWriter writer = new StreamWriter(opfStream))
            {
                writer.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine);
                writer.Write(xmlDoc);
                opfStream.SetLength(opfStream.Position);
            }
        }
    }
}
