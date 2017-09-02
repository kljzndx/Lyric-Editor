using System;
using Windows.Storage;

namespace SimpleLyricsEditor.Events
{
    public static class LyricsFileSaveNotification
    {
        public static event EventHandler<FileChangeEventArgs> RunSaved;

        public static void SaveFile(StorageFile file)
        {
            RunSaved?.Invoke(null, new FileChangeEventArgs(file));
        }
    }
}