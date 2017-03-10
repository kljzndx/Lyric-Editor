using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace LyricsEditor.Model
{
    public class LyricFileChanageEventArgs : EventArgs
    {
        public IStorageFile File { get; set; }
        public string Content { get; set; }
    }

    public delegate void LyricFileChanageEventHandler(LyricFileChanageEventArgs e);

    public static class LyricFileManager
    {
        public static IStorageFile ThisLRCFile { get; set; }
        public static event LyricFileChanageEventHandler LyricFileChanageEvent;



        public static void ChanageFile(IStorageFile file, string content)
        {
            LyricFileChanageEvent(new LyricFileChanageEventArgs { File = file, Content = content });
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
            {
                string lrcContent = await ReadLyricFile(thisLRCFile);
                ChanageFile(thisLRCFile, lrcContent);
            }
            return thisLRCFile;
        }

        public static async Task<String> ReadLyricFile(IStorageFile file)
        {
            string content = String.Empty;
            try
            {
                content = await FileIO.ReadTextAsync(file);
            }
            catch (Exception)
            {
                var filebuffer = await FileIO.ReadBufferAsync(file);
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                try
                {
                    var gbkEncoding = Encoding.GetEncoding("GBK");
                    content = gbkEncoding.GetString(filebuffer.ToArray());
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return content;
        }
    }
}
