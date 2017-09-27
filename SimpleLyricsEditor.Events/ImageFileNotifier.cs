using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace SimpleLyricsEditor.Events
{
    public static class ImageFileNotifier
    {
        public static event EventHandler<ImageFileChangeEventArgs> FileChanged;

        public static async Task ChangeFile(StorageFile file)
        {
            BitmapImage image = new BitmapImage();
            image.SetSource(await file.OpenAsync(FileAccessMode.Read));
            FileChanged?.Invoke(null, new ImageFileChangeEventArgs(file, image));
        }
    }
}