using Gui.ViewModels;
using Microsoft.Maui.Controls;
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

        public MainPage()
        {
            InitializeComponent();

            MainViewModel myViewModel = (MainViewModel)this.BindingContext;

            // Workaround
            // Flexlayout cannot be bound to a data source in xaml
            // https://github.com/dotnet/maui/issues/7747
            foreach (var book in myViewModel.BookList)
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

                this.ImageFlexLayout.Children.Add(image);
            }
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
            var book = (Book)parentImage.BindingContext;
            
            this.EntryLocation.Text = book.FileLocation;
        }
    }
}
