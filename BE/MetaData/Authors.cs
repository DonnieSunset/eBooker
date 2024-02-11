using System.IO.Compression;
using System.Xml.Linq;

namespace BE.MetaData
{
    public class Authors : MetaDataBase<Tuple<Author?, Author?>>
    {
        public void Read(ZipArchiveEntry opfEntry)
        {
            using (var opfStream = opfEntry.Open())
            {
                XDocument xmlDoc = XDocument.Load(opfStream);
                base.Data = Get(xmlDoc);
            }
        }

        public void Write(ZipArchiveEntry opfEntry, Tuple<Author?, Author?> data)
        {
            if (opfEntry.Archive.Mode != ZipArchiveMode.Update)
            {
                throw new EbookerException($"For writing, zipfile must be in update mode, but was in mode <{opfEntry.Archive.Mode}>");
            }

            if (data.Item1 == null || string.IsNullOrEmpty(data.Item1.DisplayName))
            {
                throw new EbookerException($"{nameof(data)} must contain a least one valid author.");
            }

            using (var opfStream = opfEntry.Open())
            {
                XDocument xmlDoc = XDocument.Load(opfStream);

                Set(xmlDoc, data);
                UpdateOpfInArchive(opfStream, xmlDoc);
            }

            Read(opfEntry);
        }

        public Tuple<Author?, Author?> Get(XDocument opfDocument)
        {
            var (xmlMetaData, opfNamespace, _) = GetMetaDataSection(opfDocument);
            var authors = GetEntries(xmlMetaData, opfNamespace)
                .Select(x => x.Value)
                .ToList();

            if (authors.Count > 1)
            {
                return new Tuple<Author?, Author?>(new Author(authors[0]), new Author(authors[1]));
            }
            else if (authors.Count == 1)
            {
                return new Tuple<Author?, Author?>(new Author(authors[0]), null);
            }
            else
            {
                return new Tuple<Author?, Author?>(null, null);
            }
        }

        public void Set(XDocument opfDocument, Tuple<Author?, Author?> authors)
        {
            var (xmlMetaData, opfNamespace, dcNamespace) = GetMetaDataSection(opfDocument);

            foreach (var existingEntry in GetEntries(xmlMetaData, opfNamespace))
            {
                existingEntry.Remove();
            }

            foreach (Author? author in new List<Author?> { authors.Item1, authors.Item2 })
            {
                if (author != null)
                {
                    var newElement = new XElement(dcNamespace + "creator",
                                new XAttribute(opfNamespace + "role", "aut"),
                                new XAttribute(opfNamespace + "file-as", author.SortName),
                                author.DisplayName);

                    xmlMetaData.Add(newElement);
                }
            }
        }

        private List<XElement> GetEntries(XElement xmlMetaData, XNamespace opfNamespace)
        {
            return xmlMetaData.Elements().Where(x =>
                    x.Name.LocalName == "creator" &&
                    x.Attribute(opfNamespace + "role") != null &&
                    x.Attribute(opfNamespace + "role")!.Value.Equals("aut", StringComparison.OrdinalIgnoreCase)
                ).ToList();
        }
    }
}
