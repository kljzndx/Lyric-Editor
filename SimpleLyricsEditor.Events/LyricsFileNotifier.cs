using System;
using System.Collections.Generic;
using Windows.Storage;

namespace SimpleLyricsEditor.Events
{
    public static class LyricsFileNotifier
    {
        public static event EventHandler<FileChangeEventArgs> FileChanged;
        public static event EventHandler<FileChangeEventArgs> SaveRequested;
        public static event EventHandler<KeyValuePair<StorageFile, string>> FileSaved;
        
        public static void ChangeFile(StorageFile file)
        {
            FileChanged?.Invoke(null, new FileChangeEventArgs(file));
        }
        
        public static void SendSaveRequest(StorageFile file)
        {
            SaveRequested?.Invoke(null, new FileChangeEventArgs(file));
        }

        public static void ReturnSaveResult(StorageFile file, string content)
        {
            FileSaved?.Invoke(null, new KeyValuePair<StorageFile, string>(file, content));
        }
    }
}