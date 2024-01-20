using BE;
using GuiAvalonia.Models;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows.Input;

namespace GuiAvalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
#pragma warning disable CA1822 // Mark members as static
        public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static

        public ICommand BuyMusicCommand { get; }

        private CancellationTokenSource? _cancellationTokenSource;
        public string StoreLocation { get; set; }

        public MainWindowViewModel()
        {
            BuyMusicCommand = ReactiveCommand.Create(() =>
            {
                // Code here will be executed when the button is clicked.
            });

            StoreLocation = @"C:\temp\EbookTestData\Romane";
            LoadAlbums();
            LoadCovers();
        }

        private string? _searchText;
        private bool _isBusy;

        public string? SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }

        private AlbumViewModel? _selectedAlbum;

        public ObservableCollection<AlbumViewModel> SearchResults { get; } = new();

        public AlbumViewModel? SelectedAlbum
        {
            get => _selectedAlbum;
            set => this.RaiseAndSetIfChanged(ref _selectedAlbum, value);
        }

        private async void LoadCovers()
        {
            foreach (var album in SearchResults.ToList())
            {
                await album.LoadCover();
            }
        }

        private void LoadAlbums()
        {
            foreach (var epubFile in EpubFileList)
            {
                var myEbook = new eBook(epubFile);
                SearchResults.Add(new AlbumViewModel(new Album("A1", "A2", @"P:\Downloads\71rJrY05MYL._SY466_.jpg")));
            }
        }

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

        //private async void DoSearch(string s)
        //{
        //    IsBusy = true;
        //    SearchResults.Clear();

        //    _cancellationTokenSource?.Cancel();
        //    _cancellationTokenSource = new CancellationTokenSource();
        //    var cancellationToken = _cancellationTokenSource.Token;

        //    //if (!string.IsNullOrWhiteSpace(s))
        //    //{
        //    //var albums = await Album.SearchAsync(s);

        //    //foreach (var album in albums)
        //    //{
        //    //    var vm = new AlbumViewModel(album);

        //    //    SearchResults.Add(vm);
        //    //}

        //    eBook eBook1 = new eBook(@"C:\temp\EbookTestData\Romane\Awad, Mona - Bunny.epub");
        //    var memoryStream = eBook1.GetCover();

        //    System.Drawing.Bitmap irBitmap = new System.Drawing.Bitmap(memoryStream);

        //    using (MemoryStream memory = new MemoryStream())
        //    {
        //        irBitmap.Save(memory, ImageFormat.Jpeg);
        //        memory.Position = 0;

        //        //AvIrBitmap is our new Avalonia compatible image. You can pass this to your view
        //        Avalonia.Media.Imaging.Bitmap AvIrBitmap = new Avalonia.Media.Imaging.Bitmap(memory);

        //        SearchResults.Add(new AlbumViewModel(new Album("A1", "A2", @"P:\Downloads\71rJrY05MYL._SY466_.jpg")));
        //        SearchResults[0].Cover = AvIrBitmap;
        //    }




        //        //SearchResults.Add(new AlbumViewModel(new Album("A1", "A2", @"P:\Downloads\71rJrY05MYL._SY466_.jpg")));

        //    //SearchResults.Add(new AlbumViewModel(new Album("B1", "B2", @"P:\Downloads\71Z+OgqUuLL._SY466_.jpg")));
        //    //SearchResults.Add(new AlbumViewModel(new Album("C1", "C2", @"P:\Downloads\81vQyIRBYTL._SL1500_.jpg")));

        //    if (!cancellationToken.IsCancellationRequested)
        //        {
        //            LoadCovers(cancellationToken);
        //        }
        //    //}

        //    IsBusy = false;
        //}
    }
}
