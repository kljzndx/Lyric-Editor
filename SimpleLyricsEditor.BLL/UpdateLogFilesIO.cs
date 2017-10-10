using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleLyricsEditor.BLL
{
    public static class UpdateLogFilesIO
    {
        private static readonly Uri AllLogsFolderUri = new Uri("ms-appx:///Data/UpdateLogs/");
        private static readonly Uri AllLogsFileUri = new Uri(AllLogsFolderUri, "AllLogsPath.json");
        
        public static async Task<string> GetAllLogsJson()
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(AllLogsFileUri);
            return await FileIO.ReadTextAsync(file);
        }

        public static async Task<string> GetLogContent(string fileName)
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(AllLogsFolderUri, fileName));
            return await FileIO.ReadTextAsync(file);
        }
    }
}