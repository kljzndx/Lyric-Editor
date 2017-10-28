using System;
using Windows.Storage;

namespace SimpleLyricsEditor.Events
{
    public static class LyricsFileSaveNotifier
    {
        public static event EventHandler<FileChangeEventArgs> SaveRequested;

        public static void SendSaveRequest(StorageFile file)
        {
            SaveRequested?.Invoke(null, new FileChangeEventArgs(file));
        }
    }
}