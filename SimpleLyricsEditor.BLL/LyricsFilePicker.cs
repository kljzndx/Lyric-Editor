using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SimpleLyricsEditor.BLL
{
    public static class LyricsFilePicker
    {
        private static readonly FileOpenPicker OpenPicker = GetOpenPicker();
        private static readonly FileSavePicker SavePicker = GetSavePicker();
        
        public static async Task<StorageFile> ShowOpenPicker()
        {
            return await OpenPicker.PickSingleFileAsync();
        }

        public static async Task<StorageFile> ShowSavePicker()
        {
            return await SavePicker.PickSaveFileAsync();
        }

        private static FileOpenPicker GetOpenPicker()
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".lrc");
            picker.FileTypeFilter.Add(".txt");
            return picker;
        }

        private static FileSavePicker GetSavePicker()
        {
            FileSavePicker picker = new FileSavePicker();
            picker.FileTypeChoices.Add("LRC File", new List<string> {".lrc"});
            return picker;
        }
    }
}