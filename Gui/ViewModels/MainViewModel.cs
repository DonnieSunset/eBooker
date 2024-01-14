using Microsoft.Maui.Devices.Sensors;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Gui.ViewModels
{
    public class MainViewModel : BindableObject, INotifyPropertyChanged
    {
        #region Debug

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

        #endregion

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

        public string StoreLocation { get; set; }

        private List<string> epubFileList = null;
        public List<string> EpubFileList
        {
            get
            {
                if (epubFileList == null)
                {
                    epubFileList = Directory.GetFiles(StoreLocation, "*.epub", SearchOption.AllDirectories).ToList();
                }
                return epubFileList;
            }
            set { epubFileList = value; }
        }

        public void ResetBookList()
        {
            EpubFileList = null;
            BookList.Clear();
        }

        public BookModel AddBookModelFrom(string epubFile)
        {
            var book = new BookModel(epubFile);
            BookList.Add(book);
            return book;
        }

        public IEnumerable<BookModel> ReloadBookList()
        {
            if (!Directory.Exists(StoreLocation))
            {
                throw new Exception($"Directory for ebook story <{StoreLocation}> does not exist.");
            }

            EpubFileList = null;
            BookList.Clear();

            foreach (var fileLoctaion in EpubFileList)
            {
                var book = new BookModel(fileLoctaion);
                BookList.Add(book);
                yield return book;
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
