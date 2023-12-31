using Gui.ViewModels;
using Microsoft.Maui.Controls;

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

            // Workaround because of
            // https://github.com/dotnet/maui/issues/7747
            foreach (var item in myViewModel.ImageItems)
            {
                var image = new Image
                {
                    Source = item.Source,
                    WidthRequest = 200,
                    HeightRequest = 300,
                    Margin = new Thickness(10),

                    Shadow = shadowBlack,
                };

                var clickRecognizer = new PointerGestureRecognizer();
                clickRecognizer.PointerPressed += (sender, @event) => DisplayMetaInformation(sender);
                clickRecognizer.PointerEntered += (sender, @event) => ChangeToBlueShadow(sender);
                clickRecognizer.PointerExited += (sender, @event) => ChangeToBlackShadow(sender);
                image.GestureRecognizers.Add(clickRecognizer);

                this.ImageFlexLayout.Children.Add(image);
            }
        }

        private void ChangeToBlackShadow(object image)
        {
            var parentImage = (Image)image;
            parentImage.Shadow = shadowBlack;
        }

        private void ChangeToBlueShadow(object image)
        {
            var parentImage = (Image)image;
            parentImage.Shadow = shadowBlue;
        }

        private void DisplayMetaInformation(object image)
        { 
        
        }
    }
}
