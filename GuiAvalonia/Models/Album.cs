using BE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuiAvalonia.Models
{
    public class Album
    {
        //private static iTunesSearchManager s_SearchManager = new();

        public string Artist { get; set; }
        public string Title { get; set; }
        public string CoverUrl { get; set; }

        public Album(string artist, string title, string coverUrl)
        {
            Artist = artist;
            Title = title;
            CoverUrl = coverUrl;
        }

        //public static async Task<IEnumerable<Album>> SearchAsync(string searchTerm)
        //{
        //    var query = await s_SearchManager.GetAlbumsAsync(searchTerm)
        //        .ConfigureAwait(false);

        //    return query.Albums.Select(x =>
        //        new Album(x.ArtistName, x.CollectionName,
        //            x.ArtworkUrl100.Replace("100x100bb", "600x600bb")));
        //}

        public async Task<System.IO.Stream> LoadCoverBitmapAsync()
        {
            //Task.Delay(1000);
            // Cover cover = new Cover();
                return Cover.GetMemoryStreamFromFile(CoverUrl);

        }
    }
}
