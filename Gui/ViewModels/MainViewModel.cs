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
            private StreamImageSource? imageSource = null;

            public string FileLocation { get; set; } = fileLoction;
            public string ImagePath { get; set; }
            public StreamImageSource ImageSource 
            {
                get
                {
                    if (imageSource == null)
                        imageSource = ConvertFromMemoryStream(reader.GetImage(FileLocation));

                    return imageSource;
                }
                set
                {
                    imageSource = value;
                }
            }

            private StreamImageSource ConvertFromMemoryStream(MemoryStream memoryStream)
            {
                return (StreamImageSource)StreamImageSource.FromStream(() =>
                {
                    // Ensure the MemoryStream is at the beginning
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return memoryStream;
                });
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
