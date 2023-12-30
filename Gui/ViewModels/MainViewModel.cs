using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Gui.ViewModels
{
    public class MainViewModel : BindableObject, INotifyPropertyChanged
    {
        private ObservableCollection<ImageItem> _imageItems;

        public ObservableCollection<ImageItem> ImageItems
        {
            get => _imageItems;
            set
            {
                _imageItems = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            // Hardcoded image paths, replace these with your actual image paths
            ImageItems = new ObservableCollection<ImageItem>
            {
                new ImageItem { ImagePath = @"P:\Bilder\2011-02-14 09.29.43.jpg" },
                new ImageItem { ImagePath = @"P:\Bilder\2011-02-14 09.29.43.jpg" },
                new ImageItem { ImagePath = @"P:\Bilder\2011-02-14 09.29.43.jpg" },
                new ImageItem { ImagePath = @"P:\Bilder\2011-02-14 09.29.43.jpg" },
                new ImageItem { ImagePath = @"P:\Bilder\2011-02-14 09.29.43.jpg" },
                new ImageItem { ImagePath = @"P:\Bilder\2011-02-14 09.29.43.jpg" },
                new ImageItem { ImagePath = @"P:\Bilder\2011-02-14 09.29.43.jpg" },
                // Add more image paths as needed

            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ImageItem
    {
        public string ImagePath { get; set; }
    }
}
