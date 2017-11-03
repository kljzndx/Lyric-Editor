using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleLyricsEditor.BLL
{
    public class UpdateLogFilesReader
    {
        private readonly Uri _allLogsFolderUri = new Uri("ms-appx:///Data/UpdateLogs/");

        public async Task<string> ReadDialogUI()
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(_allLogsFolderUri, "DialogUI.xml"));
            return await FileIO.ReadTextAsync(file);
        }
        
        public async Task<string> GetAllLogsJson()
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(_allLogsFolderUri, "AllLogsPath.json"));
            return await FileIO.ReadTextAsync(file);
        }

        public async Task<string> GetLogContent(string fileName)
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(_allLogsFolderUri, fileName));
            return await FileIO.ReadTextAsync(file);
        }
    }
}