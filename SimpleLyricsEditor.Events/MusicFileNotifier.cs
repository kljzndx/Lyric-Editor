using System;
using Windows.Storage;

namespace SimpleLyricsEditor.Events
{
    public class MusicFileNotifier
    {
        public static event EventHandler<FileChangeEventArgs> FileChangeRequested;

        public static void ChangeFileRequest(StorageFile file)
        {
            FileChangeRequested?.Invoke(null, new FileChangeEventArgs(file));
        }
    }
}