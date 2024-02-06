using System.IO.Compression;
using System.Xml.Linq;

namespace BE.MetaData
{
    public interface IMetaData<T>
    {
        void Read(ZipArchiveEntry opfEntry);

        void Write(ZipArchiveEntry opfEntry, T data);

        T Get(XDocument opfDocument);

        void Set(XDocument opfDocument, T data);
    }
}
