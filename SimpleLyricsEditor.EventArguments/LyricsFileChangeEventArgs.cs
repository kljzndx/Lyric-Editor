using System;
using Windows.Storage;

namespace SimpleLyricsEditor.EventArguments
{
    public class LyricsFileChangeEventArgs : EventArgs
    {
        public LyricsFileChangeEventArgs(StorageFile file)
        {
            File = file;
        }

        public StorageFile File { get; set; }
    }
}