using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SimpleLyricsEditor.BLL.Pickers
{
    public static class ImageFileOpenPicker
    {
        private static readonly FileOpenPicker Picker = GetPicker();

        private static FileOpenPicker GetPicker()
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".dng");
            picker.FileTypeFilter.Add(".dds");
            picker.FileTypeFilter.Add(".gif");
            picker.FileTypeFilter.Add(".bmp");

            return picker;
        }

        public static async Task<StorageFile> PickFile()
        {
            return await Picker.PickSingleFileAsync();
        }
    }
}