using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleLyricsEditor.BLL
{
    public class ShortcutKeysDocumentFileReader
    {
        public async Task<string> GetFileContent()
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Data/ShortcutKeysDocument/Shortcut Keys Document.xml"));
            return await FileIO.ReadTextAsync(file);
        }
    }
}