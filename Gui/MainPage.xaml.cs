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

            // Workaround because of https://github.com/dotnet/maui/issues/7747
            foreach (var item in myViewModel.ImageItems)
            {
                var image = new Image
                {
                    Source = item.Source,
                    WidthRequest = 200,
                    HeightRequest = 300,
                    Margin = new Thickness(10),
                    Shadow = new Shadow()
                    { 
                        Brush = Brush.Black,
                        Offset = new Point(20, 20),
                        Radius = 40,
                        Opacity = 0.8f
                    }
                };

                this.ImageFlexLayout.Children.Add(image);
            }
        }
    }
}
