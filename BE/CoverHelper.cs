using System.IO.Compression;

namespace BE
{
    public class CoverHelper
    {


        public static MemoryStream? GetMemoryStreamFromFile(string fileLocation)
        {
            try
            {
                FileStream fileStream = new FileStream(fileLocation, FileMode.Open, FileAccess.Read);
                Byte[] byteArray = new Byte[fileStream.Length];
                fileStream.Read(byteArray, 0, byteArray.Length);
                fileStream.Close();

                return new MemoryStream(byteArray);
            }
            catch
            {
                return null;
            }
        }
    }
}
