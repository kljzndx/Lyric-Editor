using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SimpleLyricsEditor.BLL.Pickers
{
    public static class LyricsFileSavePicker
    {
        private static readonly string DefaultFileName;

        public static readonly FileSavePicker Picker = GetPicker();

        static LyricsFileSavePicker()
        {
            DefaultFileName = ResourceLoader.GetForCurrentView("MessageBox").GetString("DefaultFileName");
        }

        public static async Task<StorageFile> PickFile(string fileName)
        {
            Picker.SuggestedFileName = !String.IsNullOrWhiteSpace(fileName) ? fileName : DefaultFileName;
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