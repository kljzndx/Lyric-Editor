using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace LyricsEditor.Model
{
    public delegate void LyricFileChanageEventHandler(LyricFileChanageEventArgs e);
    public static class LyricFileManager
    {
        public static IStorageFile ThisLRCFile { get; set; }
        
        public static event LyricFileChanageEventHandler LyricFileChanageEvent;
        public static void ChanageFile(IStorageFile file)
        {
            LyricFileChanageEvent(new LyricFileChanageEventArgs { File = file });
            ThisLRCFile = file;
        }
        public static async Task<StorageFile> OpenFileAsync()
        {
            StorageFile thisLRCFile = null;
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".lrc");
            picker.FileTypeFilter.Add(".txt");
            thisLRCFile = await picker.PickSingleFileAsync();
            if (thisLRCFile != null)
                ChanageFile(thisLRCFile);
            return thisLRCFile;
        } 

    }
}
