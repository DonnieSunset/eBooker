using BE;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Gui.ViewModels
{
    public class MainViewModel : BindableObject, INotifyPropertyChanged
    {
        private bool _imageChanged = false;
        public bool ImageChanged 
        {
            get { return _imageChanged; }
            set
            {
                _imageChanged = value;
                OnPropertyChanged(nameof(ImageChanged));
            }
        }

        public string ImageChangedFileLocation = string.Empty;

        public string ImageChangedDebugText
        {
            get { return $"Image changed: {ImageChanged}"; }
        }

        public bool MetaDataChanged { get; set; } = false;

        public void SetupBookList(string location)
        {
            if (!Directory.Exists(location))
            {
                throw new Exception($"Directory for ebook story <{location}> does not exist.");
            }

            BookList.Clear();
            foreach (var fileLoctaion in Directory.GetFiles(location, "*.epub", SearchOption.AllDirectories))
            {
                BookList.Add(new BookModel(fileLoctaion));
            }
        }

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName == nameof(ImageChanged))
            {
                OnPropertyChanged(nameof(ImageChangedDebugText));
            }
        }
    }
}
