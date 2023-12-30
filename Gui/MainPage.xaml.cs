using Gui.ViewModels;
using Microsoft.Maui.Controls;

namespace Gui
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            MainViewModel myViewModel = (MainViewModel)this.BindingContext;

            var shadowBlack = new Shadow()
            {
                Brush = Brush.Black,
                Offset = new Point(15, 15),
                Radius = 30,
                Opacity = 0.8f
            };
            var shadowBlue = new Shadow()
            {
                Brush = Brush.Navy,
                Offset = new Point(15, 15),
                Radius = 30,
                Opacity = 0.8f
            };

            var myThickness = new Thickness(10);

            // Workaround because of https://github.com/dotnet/maui/issues/7747
            foreach (var item in myViewModel.ImageItems)
            {
                var image = new Image
                {
                    Source = item.Source,
                    WidthRequest = 200,
                    HeightRequest = 300,
                    Margin = myThickness,

                    Shadow = shadowBlack,
                };

                var clickRecognizer = new PointerGestureRecognizer();
                clickRecognizer.PointerPressed += (s, e) =>
                {
                    throw new Exception("Huuhuuuuuuu");
                };
                clickRecognizer.PointerEntered += (s, e) =>
                {
                    var parentImage = (Image)s;
                    parentImage.Shadow = shadowBlue;
                };
                clickRecognizer.PointerExited += (s, e) =>
                {
                    var parentImage = (Image)s;
                    parentImage.Shadow = shadowBlack;
                };
                image.GestureRecognizers.Add(clickRecognizer);

                this.ImageFlexLayout.Children.Add(image);
            }
        }
    }
}
