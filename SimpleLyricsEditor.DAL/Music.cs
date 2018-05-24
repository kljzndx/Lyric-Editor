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
        }

        private Music(string name, string artist, string album, StorageItemThumbnail albumImageData, StorageFile file)
        {
            Name = name;
            Artist = artist;
            Album = album;
            AlbumImageData = albumImageData;
            File = file;
        }

        public static Music Empty { get; } = new Music();

        public string Name { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public StorageItemThumbnail AlbumImageData { get; set; }
        public StorageFile File { get; set; }

        public static async Task<Music> Parse(StorageFile file)
        {
            var musicProperties = await file.Properties.GetMusicPropertiesAsync();
            var bitmapData = await file.GetThumbnailAsync(ThumbnailMode.MusicView);
            return new Music(musicProperties.Title, musicProperties.Artist, musicProperties.Album, bitmapData, file);
        }
    }
}