using System.IO.Compression;
using System.Xml.Linq;

namespace BE.MetaData
{
    public class Title : MetaDataBase<string>
    {
        public void Read(ZipArchiveEntry opfEntry)
        {
            using (var opfStream = opfEntry.Open())
            {
                XDocument xmlDoc = XDocument.Load(opfStream);
                base.Data = Get(xmlDoc);
            }
        }

        public void Write(ZipArchiveEntry opfEntry, string data)
        {
            if (opfEntry.Archive.Mode != ZipArchiveMode.Update)
            {
                throw new EbookerException($"For writing, zipfile must be in update mode, but was in mode <{opfEntry.Archive.Mode}>");
            }

            if (string.IsNullOrEmpty(data))
            {
                throw new EbookerException($"{nameof(data)} must not be empty.");
            }

            using (var opfStream = opfEntry.Open())
            {
                XDocument xmlDoc = XDocument.Load(opfStream);

                Set(xmlDoc, data);
                UpdateOpfInArchive(opfStream, xmlDoc);
            }

            Read(opfEntry);
        }

        public string? Get(XDocument opfDocument)
        {
            var (xmlMetaData, opfNamespace, _) = GetMetaDataSection(opfDocument);
            var titles = GetEntries(xmlMetaData, opfNamespace);

            if (titles.Count > 1)
            {
                throw new EbookerException($"Multiple titles found on opfDocument: {Environment.NewLine}{opfDocument.ToString()}.");
            }
            else if (titles.Count < 1)
            {
                return null;
            }
            else
            {
                return titles[0].Value;
            }
        }

        public void Set(XDocument opfDocument, string title)
        {
            var (xmlMetaData, opfNamespace, dcNamespace) = GetMetaDataSection(opfDocument);

            foreach (var existingEntry in GetEntries(xmlMetaData, opfNamespace))
            {
                existingEntry.Remove();
            }

            if (title != null)
            {
                var newElement = new XElement(dcNamespace + "title",
                            title);

                xmlMetaData.Add(newElement);
            }
        }

        private List<XElement> GetEntries(XElement xmlMetaData, XNamespace opfNamespace)
        {
            return xmlMetaData.Elements().Where(x =>
                    x.Name.LocalName == "title"
                ).ToList();
        }
    }
}
