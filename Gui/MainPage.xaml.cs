using Gui.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Reflection;

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
            DisplayVersion();
            
            var configuration = MauiProgram.Services.GetService<IConfiguration>();
            var stores = configuration.Get<AppSettings>();

            myViewModel = (MainViewModel)this.BindingContext;

            PickerEbookStores.ItemsSource = stores.EBookStores;
            PickerEbookStores.SelectedIndexChanged += async (object? sender, EventArgs e) => { await PickerEbookStores_RebuildBookListAndFlexLayoutAsync(sender, e); };
            PickerEbookStores.SelectedIndex = 0;  // trigger the initial rebuild

            // Click on the image thumbnail on the right panel
            var imageThumbClickRecognizer = new PointerGestureRecognizer();
            imageThumbClickRecognizer.PointerReleased += (sender, @event) => ClickOnImageThumb(sender!);
            this.ImageThumb.GestureRecognizers.Add(imageThumbClickRecognizer);
            
            // Click on the save button on the right panel
            //var saveButtonClickRecognizer = new PointerGestureRecognizer();
            //saveButtonClickRecognizer.PointerReleased += (sender, @event) => ClickOnSaveChangesButton(sender!);
            //this.ButtonSaveChanges.GestureRecognizers.Add(saveButtonClickRecognizer);
            this.ButtonSaveChanges.Clicked += ClickOnSaveChangesButton;
        }

        private async Task PickerEbookStores_RebuildBookListAndFlexLayoutAsync(object? sender, EventArgs e)
        {
            PickerEbookStores.IsEnabled = false;
            var sw = new Stopwatch();
            sw.Start();

            // Workaround
            // Flexlayout cannot be bound to a data source in xaml
            // https://github.com/dotnet/maui/issues/7747

            myViewModel.StoreLocation = PickerEbookStores.SelectedItem as string;
            ImageFlexLayout.Clear();
            myViewModel.ResetBookList();

            double act = 0d;
            foreach (var epubFile in myViewModel.EpubFileList)
            {
                Image image = null;
                await Task.Run(() =>
                {
                    var bookModel = myViewModel.AddBookModelFrom(epubFile);
                    image = CreateImageFromBookModel(bookModel);
                });
                ImageFlexLayout.Add(image);
                ProgressBar.Progress = act++ / myViewModel.EpubFileList.Count;
                ProgressBarLabel.Text = $"{(int)(ProgressBar.Progress*100)}% ({act} of {myViewModel.EpubFileList.Count} ebooks loaded)";
            }

            ProgressBarLabel.Text = $"Loading of {myViewModel.EpubFileList.Count} ebooks took {sw.Elapsed.TotalSeconds} sec.";
            PickerEbookStores.IsEnabled = true;
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
            clickRecognizer.PointerPressed += (sender, @event) => UpdateRightPane(sender!);
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

        private void UpdateRightPaneThumbnail(BookModel bookModel)
        {
            ImageThumb.Source = bookModel.ImageMemoryStream?.Length == 0
                // at the time of implementation, this worked only with png format
                // and only with embedded resource as build action (not MauiImage)
                ? ImageSource.FromResource("Gui.Resources.Images.no_cover.png", Assembly.GetCallingAssembly())
                : bookModel.ConvertFromMemoryStream(bookModel.ImageMemoryStream);
            ImageThumb.BindingContext = bookModel;

            ButtonSaveChanges.BindingContext = bookModel;

            EntryLocation.Text = bookModel.FileLocation;

            ButtonSaveChanges.IsEnabled = false;
            myViewModel.AuthorsChanged = false;
            myViewModel.ImageChanged = false;
            myViewModel.ImageChangedFileLocation = string.Empty;
        }

        private void UpdateRightPaneMetaData(BookModel bookModel)
        {
            EntryAuthor1.TextChanged -= EntryAuthor_TextChanged;
            EntryAuthor2.TextChanged -= EntryAuthor_TextChanged;

            EntryAuthor1.Text = bookModel.GetAuthor1();
            EntryAuthor2.Text = bookModel.GetAuthor2();

            EntryAuthor1.TextChanged += EntryAuthor_TextChanged;
            EntryAuthor2.TextChanged += EntryAuthor_TextChanged;
        }

        private void UpdateRightPane(object sender)
        {
            var parentImage = (Image)sender;
            var bookModel = (BookModel)parentImage.BindingContext;

            UpdateRightPaneThumbnail(bookModel);
            UpdateRightPaneMetaData(bookModel);
        }

        private void EntryAuthor_TextChanged(object? sender, TextChangedEventArgs e)
        {
            myViewModel.AuthorsChanged = true;
            ButtonSaveChanges.IsEnabled = true;
        }

        private async void ClickOnImageThumb(object sender)
        {
            try
            {
                var parentImage = (Image)sender;
                var book = (BookModel)parentImage.BindingContext;

                var pickOptions = new PickOptions() { FileTypes = FilePickerFileType.Jpeg };
                var result = await FilePicker.Default.PickAsync(pickOptions);
                if (result != null)
                {
                    var memStream = BE.Cover.GetMemoryStreamFromFile(result.FullPath);
                    ImageThumb.Source = book.ConvertFromMemoryStream(memStream);

                    myViewModel.ImageChanged = true;
                    myViewModel.ImageChangedFileLocation = result.FullPath;
                    ButtonSaveChanges.IsEnabled = true;
                }
            }
            catch
            {
                myViewModel.AuthorsChanged = false;
                myViewModel.ImageChanged = false;
                myViewModel.ImageChangedFileLocation = string.Empty;
                ButtonSaveChanges.IsEnabled = false;
            }
        }

        private async void ClickOnSaveChangesButton(object sender, EventArgs e)
        {
            try
            {
                var saveChangesButton = (Button)sender;
                var bookModel = (BookModel)saveChangesButton.BindingContext;

                if (myViewModel.ImageChanged == true && !String.IsNullOrEmpty(myViewModel.ImageChangedFileLocation))
                {
                    bookModel.UpdateCover(myViewModel.ImageChangedFileLocation);
                    
                    var outdatedThumbnail = ImageFlexLayout.Children.Single(image => ((Image)image).BindingContext == bookModel);
                    var outdatedThumbnailIndex = ImageFlexLayout.Children.IndexOf(outdatedThumbnail);
                    
                    // we have to reset all image streams, so creating a new viewmodel is the cleanest way
                    bookModel = new BookModel(bookModel.FileLocation); 
                    
                    var newThumbNail = CreateImageFromBookModel(bookModel);
                    ImageFlexLayout.Children[outdatedThumbnailIndex] = newThumbNail;
                }

                if (myViewModel.AuthorsChanged == true)
                {
                    bookModel.UpdateAuthors(EntryAuthor1.Text, EntryAuthor2.Text);
                    UpdateRightPaneMetaData(bookModel);
                }
            }
            finally
            {
                myViewModel.AuthorsChanged = false;
                myViewModel.ImageChanged = false;
                myViewModel.ImageChangedFileLocation = string.Empty;
                ButtonSaveChanges.IsEnabled = false;
            }
        }

        private void DisplayVersion()
        {
            Version? version = Assembly.GetEntryAssembly()?.GetName().Version;
            if (version != null)
            {
                string versionString = $"Version: {version.Major}.{version.Minor}.{version.Build}";
                LabelVersion.Text = versionString;
            }
        }
    }
}
