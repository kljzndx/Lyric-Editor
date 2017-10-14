using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleLyricsEditor.BLL
{
    public static class UpdateLogFilesReader
    {
        private static readonly Uri AllLogsFolderUri = new Uri("ms-appx:///Data/UpdateLogs/");
        
        public static async Task<string> GetAllLogsJson()
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(AllLogsFolderUri, "AllLogsPath.json"));
            return await FileIO.ReadTextAsync(file);
        }

        public static async Task<string> GetLogContent(string fileName)
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(AllLogsFolderUri, fileName));
            return await FileIO.ReadTextAsync(file);
        }
    }
}