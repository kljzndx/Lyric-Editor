using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SimpleLyricsEditor.BLL
{
    public static class LyricsFileSavePicker
    {
        private static readonly FileSavePicker Picker = GetPicker();

        public static async Task<StorageFile> PickFile()
        {
            return await Picker.PickSaveFileAsync();
        }

        private static FileSavePicker GetPicker()
        {
            var picker = new FileSavePicker();
            picker.FileTypeChoices.Add("LRC File", new List<string>{".lrc"});
            return picker;
        }
    }
}