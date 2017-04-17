using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace SimpleLyricEditor.Models
{
    public class Music
    {
        public static Music Empty { get; } = new Music();

        public string Name { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public BitmapImage AlbumImage { get; set; }
        public TimeSpan AllTime { get; set; }
        public StorageFile File { get; set; }

        private Music()
        {
            Name = String.Empty;
            Artist = String.Empty;
            Album = String.Empty;
            AlbumImage = new BitmapImage();
            AllTime = TimeSpan.Zero;
        }

        public static async Task<Music> Parse(IStorageFile iFile)
        {
            var file = iFile as StorageFile;
            var propertys = await file.Properties.GetMusicPropertiesAsync();
            BitmapImage albumImage = new BitmapImage();
            albumImage.SetSource(await file.GetScaledImageAsThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.MusicView));
            return new Music
            {
                Name = propertys.Title,
                Artist = propertys.Artist,
                Album = propertys.Album,
                AlbumImage = albumImage,
                AllTime = propertys.Duration,
                File = file
            };
        }
    }
}
