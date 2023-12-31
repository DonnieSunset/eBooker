using BE;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Gui.ViewModels
{
    public class MainViewModel : BindableObject, INotifyPropertyChanged
    {
        string ebookFolder = @"P:\Ebooks\Romane";

        public class Book(string fileLoction, Reader reader)
        {
            private Reader reader = reader;

            public string ImagePath { get; set; }
            private StreamImageSource? imageSource = null;
            private MemoryStream? imageMemoryStream = null;

            public string FileLocation { get; set; } = fileLoction;

            public MemoryStream ImageMemoryStream 
            {
                get
                {
                    if (imageMemoryStream == null)
                        imageMemoryStream = reader.GetImage(FileLocation);

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

        private ObservableCollection<Book> _books = new();
        private Reader reader = new Reader();

        public ObservableCollection<Book> BookList
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
            //foreach (var memoryStream in reader.GetImages())
            //{
            //    BookList.Add(
            //        new Book 
            //        { 
            //            ImagePath = @"P:\Bilder\2011-02-14 09.29.43.jpg", 
            //            Source = ConvertFromMemoryStream(memoryStream),
            //        });
            //}

            foreach (var fileLoction in Directory.GetFiles(ebookFolder, "*", SearchOption.AllDirectories))
            { 
                BookList.Add(new Book(fileLoction, reader));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
