using LyricsEditor.EventArg;
using LyricsEditor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace LyricsEditor.Tools
{
    
    public static class LyricFileTools
    {
        public static IStorageFile ThisLRCFile { get; private set; }
        public static event EventHandler<LyricFileChanageEventArgs> LyricFileChanageEvent;
        
        public static void ChanageFile(IStorageFile file, string content)
        {
            LyricFileChanageEvent(null, new LyricFileChanageEventArgs { File = file, Content = content });
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
        public static async Task<bool> SaveLyricAsync(IList<Lyric> lyricList, LyricIDTag tag)
        {
            StorageFile lrcFile = null;
            if (LyricFileTools.ThisLRCFile is null)
            {
                FileSavePicker picker = new FileSavePicker();
                picker.SuggestedFileName = "New_LRC_File";
                picker.DefaultFileExtension = ".lrc";
                picker.FileTypeChoices.Add("LRC File", new List<string> { ".lrc" });

                lrcFile = await picker.PickSaveFileAsync();
                if (lrcFile is null)
                    return false;
            }
            else
                lrcFile = LyricFileTools.ThisLRCFile as StorageFile;

            string fileContent = LyricTools.PrintLyric(lyricList, tag);

            await FileIO.WriteTextAsync(lrcFile, fileContent);
            ThisLRCFile = lrcFile;

            return true;
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
