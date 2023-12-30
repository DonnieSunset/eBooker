using BE;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Gui.ViewModels
{
    public class MainViewModel : BindableObject, INotifyPropertyChanged
    {
        public class ImageItem
        {
            public string ImagePath { get; set; }
            public StreamImageSource Source { get; set; }
        }

        private ObservableCollection<ImageItem> _imageItems;
        private Reader reader = new Reader();

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
            //MemoryStream memoryStream = reader.GetImage();
            //MemoryStream memoryStream2 = reader.GetImage();

            //StreamImageSource streamImageSource = (StreamImageSource)StreamImageSource.FromStream(() =>
            //{
            //    memoryStream.Seek(0, SeekOrigin.Begin); // Ensure the MemoryStream is at the beginning
            //    return memoryStream;
            //});

            //StreamImageSource streamImageSource2 = (StreamImageSource)StreamImageSource.FromStream(() =>
            //{
            //    memoryStream2.Seek(0, SeekOrigin.Begin); // Ensure the MemoryStream is at the beginning
            //    return memoryStream2;
            //});

            ////StreamImageSource imageSource = (StreamImageSource)ImageSource.FromStream(() => { return bla; });

            //// Hardcoded image paths, replace these with your actual image paths
            ImageItems = new ObservableCollection<ImageItem>
            {
                //new ImageItem { ImagePath = @"P:\Bilder\2011-02-14 09.29.43.jpg", Source = streamImageSource },
                //new ImageItem { ImagePath = @"P:\Bilder\2011-02-14 09.29.43.jpg", Source = streamImageSource2 },
                //new ImageItem { ImagePath = @"P:\Bilder\2011-02-14 09.29.43.jpg", Source = streamImageSource },
                //new ImageItem { ImagePath = @"P:\Bilder\2011-02-14 09.29.43.jpg", Source = streamImageSource },
                //new ImageItem { ImagePath = @"P:\Bilder\2011-02-14 09.29.43.jpg", Source = streamImageSource },
                //new ImageItem { ImagePath = @"P:\Bilder\2011-02-14 09.29.43.jpg", Source = streamImageSource },
                //new ImageItem { ImagePath = @"P:\Bilder\2011-02-14 09.29.43.jpg", Source = streamImageSource },
                // Add more image paths as needed

            };

            foreach (var memoryStream3 in reader.GetImages())
            {
                StreamImageSource streamImageSource3 = (StreamImageSource)StreamImageSource.FromStream(() =>
                {
                    memoryStream3.Seek(0, SeekOrigin.Begin); // Ensure the MemoryStream is at the beginning
                    return memoryStream3;
                });

                ImageItems.Add(new ImageItem { ImagePath = @"P:\Bilder\2011-02-14 09.29.43.jpg", Source = streamImageSource3 });
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}
