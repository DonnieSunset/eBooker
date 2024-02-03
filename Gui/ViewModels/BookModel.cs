using BE;

namespace Gui.ViewModels
{
    public class BookModel(string fileLoction)
    {
        private eBook eBook = new eBook(fileLoction);

        private StreamImageSource? imageSource = null;
        private MemoryStream? imageMemoryStream = null;

        public string FileLocation { get; set; } = fileLoction;

        public MemoryStream ImageMemoryStream
        {
            get
            {
                if (imageMemoryStream == null)
                    imageMemoryStream = eBook.GetCover();

                return imageMemoryStream;
            }
        }

        public StreamImageSource ImageSource
        {
            get
            {
                if (imageSource == null)
                    imageSource = ConvertFromMemoryStream(ImageMemoryStream);

                return imageSource;
            }
            set
            {
                imageSource = value;
            }
        }

        public StreamImageSource ConvertFromMemoryStream(MemoryStream memoryStream)
        {
            memoryStream.Position = 0;
            MemoryStream copied = new MemoryStream();
            memoryStream.CopyTo(copied);
            copied.Position = 0;
            memoryStream.Position = 0;

            return (StreamImageSource)StreamImageSource.FromStream(() => { return copied; });
        }

        public void UpdateCover(string coverFileLocation)
        {
            eBook.UpdateCover(coverFileLocation);
        }

        public string GetAuthor1()
        {
            return (eBook.MetaData.Author1 != null)
                ? eBook.MetaData.Author1.DisplayName
                : string.Empty;
        }

        public string GetAuthor2()
        {
            return (eBook.MetaData.Author2 != null)
                ? eBook.MetaData.Author2.DisplayName
                : string.Empty;
        }
    }
}
