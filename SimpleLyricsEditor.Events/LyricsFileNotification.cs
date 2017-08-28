using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleLyricsEditor.Events
{
    public static class LyricsFileNotification
    {
        public static event EventHandler<LyricsFileChangeEventArgs> FileChanged;
        
        public static void ChangeFile(StorageFile file)
        {
            FileChanged?.Invoke(null, new LyricsFileChangeEventArgs(file));
        }
    }
}