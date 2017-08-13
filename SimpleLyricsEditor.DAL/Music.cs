using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;

namespace SimpleLyricsEditor.DAL
{
    public class Music
    {
        private Music()
        {
            Name = string.Empty;
            Artist = string.Empty;
            Album = string.Empty;
            AlbumImage = new BitmapImage();
        }

        private Music(string name, string artist, string album, BitmapSource albumImage, StorageFile file)
        {
            Name = name;
            Artist = artist;
            Album = album;
            AlbumImage = albumImage;
            File = file;
        }

        public static Music Empty { get; } = new Music();

        public string Name { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public BitmapSource AlbumImage { get; set; }
        public StorageFile File { get; set; }

        public static async Task<Music> Parse(StorageFile file)
        {
            var musicProperties = await file.Properties.GetMusicPropertiesAsync();
            var image = new BitmapImage();
            image.SetSource(await file.GetThumbnailAsync(ThumbnailMode.MusicView));
            return new Music(musicProperties.Title, musicProperties.Artist, musicProperties.Album, image, file);
        }
    }
}