using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media.Imaging;

namespace LyricsEditor.Model
{
    class Music : Auxiliary
    {
        private string name, artist, album;
        private BitmapImage albumImage;
        private TimeSpan allTime;
        private StorageFile file;

        /// <summary>
        /// 歌名
        /// </summary>
        public string Name { get => name; set => SetProperty(ref name,value); }
        /// <summary>
        /// 歌手
        /// </summary>
        public string Artist { get => artist; set => SetProperty(ref artist, value); }
        /// <summary>
        /// 专辑名
        /// </summary>
        public string Album { get => album; set => SetProperty(ref album, value); }
        /// <summary>
        /// 专辑图
        /// </summary>
        public BitmapImage AlbumImage { get => albumImage; set => SetProperty(ref albumImage, value); }
        /// <summary>
        /// 总时长
        /// </summary>
        public TimeSpan Alltime { get => allTime; set => SetProperty(ref allTime, value); }
        /// <summary>
        /// 音乐文件
        /// </summary>
        public StorageFile File { get => file; set => SetProperty(ref file, value); }
        public Music()
        {
            albumImage = new BitmapImage();
        }
        /// <summary>
        /// 打开音乐文件
        /// </summary>
        /// <returns>成功与否</returns>
        public async Task<bool> OpenFile()
        {
            FileOpenPicker picker = new FileOpenPicker { CommitButtonText = "打开此音乐" };
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".flac");
            picker.FileTypeFilter.Add(".wav");
            picker.FileTypeFilter.Add(".aac");
            picker.FileTypeFilter.Add(".m4a");
            File = await picker.PickSingleFileAsync();
            if (File == null)
                return false;
            var properties = await File.Properties.GetMusicPropertiesAsync();
            Name = properties.Title;
            Artist = properties.Artist;
            Album = properties.Album;
            Alltime = properties.Duration;
            AlbumImage.SetSource(await File.GetThumbnailAsync(ThumbnailMode.MusicView));
            return true;
        }
    }
}
