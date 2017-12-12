using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SimpleLyricsEditor.BLL.Pickers
{
    public static class LyricsFileOpenPicker
    {
        private static readonly FileOpenPicker Picker = GetPicker();

        public static async Task<StorageFile> PickFile()
        {
            return await Picker.PickSingleFileAsync();
        }
        
        private static FileOpenPicker GetPicker()
        {
            FileOpenPicker picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };
            picker.FileTypeFilter.Add(".lrc");
            picker.FileTypeFilter.Add(".txt");

            return picker;
        }
    }
}