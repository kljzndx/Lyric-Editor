using System;
using Windows.Storage;

namespace SimpleLyricsEditor.Events
{
    public class FileChangeEventArgs : EventArgs
    {
        public FileChangeEventArgs(StorageFile file)
        {
            File = file;
        }

        public StorageFile File { get; set; }
    }
}