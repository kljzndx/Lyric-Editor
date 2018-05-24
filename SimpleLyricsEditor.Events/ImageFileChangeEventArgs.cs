using System;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace SimpleLyricsEditor.Events
{
    public class ImageFileChangeEventArgs : EventArgs
    {
        public ImageFileChangeEventArgs(IRandomAccessStream data, BitmapSource source)
        {
            Data = data;
            Source = source;
        }

        public IRandomAccessStream Data { get; set; }
        public BitmapSource Source { get; set; }
    }
}