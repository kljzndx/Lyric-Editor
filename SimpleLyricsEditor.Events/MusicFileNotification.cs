using System;
using Windows.Storage;

namespace SimpleLyricsEditor.Events
{
    public class MusicFileNotification
    {
        public static event EventHandler<FileChangeEventArgs> FileChanged;

        public static void ChangeFile(StorageFile file)
        {
            FileChanged?.Invoke(null, new FileChangeEventArgs(file));
        }
    }
}