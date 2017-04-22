using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleLyricEditor.EventArgss;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Runtime.InteropServices.WindowsRuntime;
using SimpleLyricEditor.Models;
using System.Collections.ObjectModel;
using SimpleLyricEditor.ViewModels;

namespace SimpleLyricEditor.Tools
{
    public static class LyricFileTools
    {
        public static event EventHandler<LyricFileChangeEventArgs> LyricFileChanged;


        public static void ChangeFile(StorageFile file)
        {
            LyricFileChanged?.Invoke(null, new LyricFileChangeEventArgs(file));
        }

        public static async Task<StorageFile> OpenFileAsync()
        {
            StorageFile file = null;
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".lrc");
            picker.FileTypeFilter.Add(".txt");
            file = await picker.PickSingleFileAsync();
            if (file is StorageFile)
                ChangeFile(file);

            return file;
        }

        public static async Task<string> ReadFileAsync(StorageFile file)
        {
            string content = String.Empty;

            try
            {
                content = await FileIO.ReadTextAsync(file);
            }
            catch (Exception)
            {
                var buffer = await FileIO.ReadBufferAsync(file);
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                try
                {
                    var gbkEncoding = Encoding.GetEncoding("GBK");
                    content = gbkEncoding.GetString(buffer.ToArray());
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return content;
        }

        public static async Task<StorageFile> SaveFileAsync(LyricTags tags, Collection<LyricItem> lyrics, StorageFile file = null)
        {
            StorageFile result = null;
            if (file is null)
            {
                FileSavePicker picker = new FileSavePicker()
                {
                    DefaultFileExtension = ".lrc"
                };
                var types = new List<string>();
                types.Add(".lrc");
                picker.FileTypeChoices.Add("LRC File", types);

                result = await picker.PickSaveFileAsync();
            }
            else
                result = file;

            if (result is null)
                return null;

            await FileIO.WriteTextAsync(result, LyricTools.PrintLyric(tags, lyrics));

            if (file is null)
                ChangeFile(result);

            return result;
        }
    }
}
