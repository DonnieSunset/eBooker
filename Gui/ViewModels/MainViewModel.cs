using BE;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Gui.ViewModels
{
    public class MainViewModel : BindableObject, INotifyPropertyChanged
    {
        string ebookFolder = @"C:\temp\EbookTestData\Romane";

        public class BookModel(string fileLoction)
        {
            public string ImagePath { get; set; }
            private StreamImageSource? imageSource = null;
            private MemoryStream? imageMemoryStream = null;

            public string FileLocation { get; set; } = fileLoction;
            private eBook eBook = new eBook(fileLoction);

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
        }

        private ObservableCollection<BookModel> _books = new();

        public ObservableCollection<BookModel> BookList
        {
            get => _books;
            set
            {
                _books = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            foreach (var fileLoctaion in Directory.GetFiles(ebookFolder, "*.epub", SearchOption.AllDirectories))
            { 
                BookList.Add(new BookModel(fileLoctaion));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
