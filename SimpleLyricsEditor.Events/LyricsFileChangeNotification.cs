using System;
using Windows.Storage;

namespace SimpleLyricsEditor.Events
{
    public static class LyricsFileChangeNotification
    {
        public static event EventHandler<FileChangeEventArgs> FileChanged;
        
        public static void ChangeFile(StorageFile file)
        {
            FileChanged?.Invoke(null, new FileChangeEventArgs(file));
        }
    }
}