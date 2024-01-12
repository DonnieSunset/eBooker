using BE;
using Gui.ViewModels;
using Microsoft.Maui.Controls;
using System.Windows.Input;
using static Gui.ViewModels.MainViewModel;

namespace Gui
{
    public partial class MainPage : ContentPage
    {
        private Shadow shadowBlack = new Shadow()
        {
            Brush = Brush.Black,
            Offset = new Point(15, 15),
            Radius = 30,
            Opacity = 0.8f
        };

        private Shadow shadowBlue = new Shadow()
        {
            Brush = Brush.Navy,
            Offset = new Point(15, 15),
            Radius = 30,
            Opacity = 0.8f
        };

        private MainViewModel myViewModel = null;

        public MainPage()
        {
            InitializeComponent();

             myViewModel = (MainViewModel)this.BindingContext;

            // Workaround
            // Flexlayout cannot be bound to a data source in xaml
            // https://github.com/dotnet/maui/issues/7747
            foreach (var book in myViewModel.BookList)
            {
                var image = CreateImageFromBookModel(book);
                ImageFlexLayout.Children.Add(image);
            }

            // Click on the image thumbnail on the right panel
            var imageThumbClickRecognizer = new PointerGestureRecognizer();
            imageThumbClickRecognizer.PointerReleased += (sender, @event) => ClickOnImageThumb(sender!);
            this.ImageThumb.GestureRecognizers.Add(imageThumbClickRecognizer);
            //this.ImageThumb.

            // Click on the save button on the right panel
            //var saveButtonClickRecognizer = new PointerGestureRecognizer();
            //saveButtonClickRecognizer.PointerReleased += (sender, @event) => ClickOnSaveChangesButton(sender!);
            //this.ButtonSaveChanges.GestureRecognizers.Add(saveButtonClickRecognizer);
            this.ButtonSaveChanges.Clicked += ClickOnSaveChangesButton;
        }

        private Image CreateImageFromBookModel(BookModel book)
        {
            var image = new Image
            {
                BindingContext = book,
                Source = book.ImageSource,

                WidthRequest = 200,
                HeightRequest = 300,
                Margin = new Thickness(10),
                Shadow = shadowBlack,
            };

            var clickRecognizer = new PointerGestureRecognizer();
            clickRecognizer.PointerPressed += (sender, @event) => DisplayMetaInformation(sender!);
            clickRecognizer.PointerEntered += (sender, @event) => ChangeToBlueShadow(sender!);
            clickRecognizer.PointerExited += (sender, @event) => ChangeToBlackShadow(sender!);
            image.GestureRecognizers.Add(clickRecognizer);

            return image;
        }

        private void ChangeToBlackShadow(object sender)
        {
            var parentImage = (Image)sender;
            parentImage.Shadow = shadowBlack;
        }

        private void ChangeToBlueShadow(object sender)
        {
            var parentImage = (Image)sender;
            parentImage.Shadow = shadowBlue;
        }

        private void DisplayMetaInformation(object sender)
        {
            var parentImage = (Image)sender;
            var book = (BookModel)parentImage.BindingContext;

            ImageThumb.Source = book.ConvertFromMemoryStream(book.ImageMemoryStream);
            ImageThumb.BindingContext = book;

            ButtonSaveChanges.BindingContext = book;

            EntryLocation.Text = book.FileLocation;

            ButtonSaveChanges.IsEnabled = false;
            myViewModel.ImageChanged = false;
            myViewModel.ImageChangedFileLocation = string.Empty;
        }

        private async void ClickOnImageThumb(object sender)
        {
            try
            {
                var parentImage = (Image)sender;
                var book = (BookModel)parentImage.BindingContext;

                var pickOptions = new PickOptions() { FileTypes = FilePickerFileType.Jpeg };
                var result = await FilePicker.Default.PickAsync(pickOptions);

                var memStream = BE.Cover.GetMemoryStreamFromFile(result.FullPath);
                ImageThumb.Source = book.ConvertFromMemoryStream(memStream);

                myViewModel.ImageChanged = true;
                myViewModel.ImageChangedFileLocation = result.FullPath;
                ButtonSaveChanges.IsEnabled = true;
            }
            catch
            {
                myViewModel.ImageChanged = false;
                myViewModel.ImageChangedFileLocation = string.Empty;
                ButtonSaveChanges.IsEnabled = false;
            }
        }

        public async void ClickOnSaveChangesButton(object sender, EventArgs e)
        {
            try
            {
                var saveChangesButton = (Button)sender;
                var book = (BookModel)saveChangesButton.BindingContext;

                if (myViewModel.ImageChanged == true && !String.IsNullOrEmpty(myViewModel.ImageChangedFileLocation))
                {
                    book.UpdateCover(myViewModel.ImageChangedFileLocation);
                    
                    var outdatedThumbnail = ImageFlexLayout.Children.Single(image => ((Image)image).BindingContext == book);
                    var outdatedThumbnailIndex = ImageFlexLayout.Children.IndexOf(outdatedThumbnail);
                    
                    // we have to reset all image streams, so creating a new viewmodel is the cleanest way
                    book = new BookModel(book.FileLocation); 
                    
                    var newThumbNail = CreateImageFromBookModel(book);
                    ImageFlexLayout.Children[outdatedThumbnailIndex] = newThumbNail;
                }
            }
            finally
            {
                myViewModel.ImageChanged = false;
                myViewModel.ImageChangedFileLocation = string.Empty;
                ButtonSaveChanges.IsEnabled = false;
            }
        }
    }
}
