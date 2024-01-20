using Avalonia.Media.Imaging;
using BE;
using GuiAvalonia.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuiAvalonia.ViewModels
{
    public class AlbumViewModel : ViewModelBase
    {
        private readonly Album _album;

        public AlbumViewModel(Album album)
        {
            _album = album;
        }

        public string Artist => _album.Artist;

        public string Title => _album.Title;

        private Bitmap? _cover;

        public Bitmap? Cover
        {
            get => _cover;
            set => this.RaiseAndSetIfChanged(ref _cover, value);
        }

        public async Task LoadCover()
        {
            await using (var imageStream = await _album.LoadCoverBitmapAsync())
            {
                //Cover = await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 400));
                //Cover = await Task.Run(() => new Bitmap(_album.CoverUrl));

                eBook eBook1 = new eBook(@"C:\temp\EbookTestData\Romane\Awad, Mona - Bunny.epub");
                var memoryStream = eBook1.GetCover();

                System.Drawing.Bitmap irBitmap = new System.Drawing.Bitmap(memoryStream);

                using (MemoryStream memory = new MemoryStream())
                {
                    irBitmap.Save(memory, ImageFormat.Jpeg);
                    memory.Position = 0;

                    //AvIrBitmap is our new Avalonia compatible image. You can pass this to your view
                    Avalonia.Media.Imaging.Bitmap AvIrBitmap = new Avalonia.Media.Imaging.Bitmap(memory);

                    //SearchResults.Add(new AlbumViewModel(new Album("A1", "A2", @"P:\Downloads\71rJrY05MYL._SY466_.jpg")));
                    Cover = AvIrBitmap;
                }


            }
        }
    }
}
