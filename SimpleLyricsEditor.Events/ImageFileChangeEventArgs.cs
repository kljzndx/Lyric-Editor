using System;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace SimpleLyricsEditor.Events
{
    public class ImageFileChangeEventArgs : EventArgs
    {
        public ImageFileChangeEventArgs(StorageFile file, BitmapSource source)
        {
            File = file;
            Source = source;
        }

        public StorageFile File { get; set; }
        public BitmapSource Source { get; set; }
    }
}