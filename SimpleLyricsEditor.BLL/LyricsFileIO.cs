using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleLyricsEditor.BLL
{
    public static class LyricsFileIO
    {
        private static readonly Encoding GbkEncoding = GetGbkEncoding();

        public static async Task<string> ReadText(IStorageFile file)
        {
            string result;

            try
            {
                result = await FileIO.ReadTextAsync(file);
            }
            catch (Exception)
            {
                var buffer = await FileIO.ReadBufferAsync(file);
                var bytes = buffer.ToArray();

                result = GbkEncoding.GetString(bytes);
            }

            return result;
        }

        public static async Task WriteText(StorageFile file, string contents)
        {
            await FileIO.WriteTextAsync(file, contents);
        }

        private static Encoding GetGbkEncoding()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return Encoding.GetEncoding("GBK");
        }
    }
}