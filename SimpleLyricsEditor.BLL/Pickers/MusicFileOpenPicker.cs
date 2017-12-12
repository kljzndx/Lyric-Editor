using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SimpleLyricsEditor.BLL.Pickers
{
    public class MusicFileOpenPicker
    {
        private static readonly FileOpenPicker Picker = GetPicker();

        public static async Task<StorageFile> PickFile()
        {
            return await Picker.PickSingleFileAsync();
        }

        private static FileOpenPicker GetPicker()
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };
            picker.FileTypeFilter.Add(".wav");
            picker.FileTypeFilter.Add(".flac");
            picker.FileTypeFilter.Add(".alac");
            picker.FileTypeFilter.Add(".aac");
            picker.FileTypeFilter.Add(".m4a");
            picker.FileTypeFilter.Add(".mp3");
            return picker;
        }
    }
}