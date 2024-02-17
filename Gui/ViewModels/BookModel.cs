using BE;
using BE.MetaData;

namespace Gui.ViewModels
{
    public class BookModel(string fileLoction)
    {
        private eBook _eBook = new eBook(fileLoction);

        public string FileLocation { get; set; } = fileLoction;

        public StreamImageSource ConvertFromMemoryStream(MemoryStream memoryStream)
        {
            memoryStream.Position = 0;
            MemoryStream copied = new MemoryStream();
            memoryStream.CopyTo(copied);
            copied.Position = 0;
            memoryStream.Position = 0;

            return (StreamImageSource)StreamImageSource.FromStream(() => { return copied; });
        }

        public string GetAuthor1()
        {
            Author author1 = _eBook.GetAuthors().Item1;

            return (author1 != null)
                ? author1.DisplayName
                : string.Empty;
        }

        public string GetAuthor2()
        {
            Author author2 = _eBook.GetAuthors().Item2;

            return (author2 != null)
                ? author2.DisplayName
                : string.Empty;
        }

        public void UpdateAuthors(string? author1, string? author2)
        {
            Author? authorObj1 = !string.IsNullOrEmpty(author1) ?
                new Author(author1) :
                null;

            Author? authorObj2 = !string.IsNullOrEmpty(author2) ?
                new Author(author2) :
                null;

            _eBook.UpdateAuthors(authorObj1, authorObj2);
        }

        public StreamImageSource? GetCover()
        {
            return (_eBook.GetCover() != null)
                ? ConvertFromMemoryStream(_eBook.GetCover())
                : null;
        }

        public void UpdateCover(string coverFileLocation)
        {
            _eBook.UpdateCover(coverFileLocation);
        }

        public string GetTitle()
        {
            string? title = _eBook.GetTitle();

            return (title != null)
                ? title
                : string.Empty;
        }

        public void UpdateTitle(string title)
        {
            _eBook.UpdateTitle(title);
        }
    }
}
