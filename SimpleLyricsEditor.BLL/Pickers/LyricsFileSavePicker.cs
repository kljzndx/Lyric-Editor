using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SimpleLyricsEditor.BLL.Pickers
{
    public static class LyricsFileSavePicker
    {
        public static readonly FileSavePicker Picker = GetPicker();

        public static async Task<StorageFile> PickFile(string fileName)
        {
            Picker.SuggestedFileName = !String.IsNullOrWhiteSpace(fileName) ? fileName : "new lyrics file";
            return await Picker.PickSaveFileAsync();
        }

        private static FileSavePicker GetPicker()
        {
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };
            picker.FileTypeChoices.Add("LRC File", new List<string>{".lrc"});

            return picker;
        }
    }
}