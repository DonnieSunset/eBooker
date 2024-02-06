using BE;
using BE.MetaData;

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

        public void UpdateAuthors(string? author1, string? author2)
        {
            Author? authorObj1 = !String.IsNullOrEmpty(author1) ?
                new Author(author1) :
                null;

            Author? authorObj2 = !String.IsNullOrEmpty(author2) ?
                new Author(author2) :
                null;

            eBook.UpdateAuthors(authorObj1, authorObj2);
        }

        public string GetAuthor1()
        {
            return (eBook.MetaData.Authors?.Data?.Item1 != null)
                ? eBook.MetaData.Authors.Data.Item1.DisplayName
                : string.Empty;
        }

        public string GetAuthor2()
        {
            return (eBook.MetaData.Authors?.Data?.Item2 != null)
                ? eBook.MetaData.Authors.Data.Item2.DisplayName
                : string.Empty;
        }
    }
}
