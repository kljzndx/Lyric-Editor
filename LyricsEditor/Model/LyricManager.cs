using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace LyricsEditor.Model
{
    public static class LyricManager
    {
        public static IStorageFile ThisLRCFile { get; set; }

        public static async Task<bool> OpenLRCAndAnalysis(ObservableCollection<Lyric> lyricContent, LyricIDTag idTag)
        {
            lyricContent.Clear();
            ThisLRCFile = null;
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".lrc");
            picker.FileTypeFilter.Add(".txt");

            ThisLRCFile = await picker.PickSingleFileAsync();

            if (ThisLRCFile is null)
                return false;
            string content = String.Empty;
            try
            {
                content = await FileIO.ReadTextAsync(ThisLRCFile);
            }
            catch (Exception ex)
            {
                ThisLRCFile = null;
                if (ex.HResult.ToString("X") == "80070459")
                    await MessageBox.ShowMessageBoxAsync(CharacterLibrary.MessageBox.GetString("EncodingError"), CharacterLibrary.MessageBox.GetString("Close"));
                else
                    throw;
            }
            
            if (idTag.Title == String.Empty)
                idTag.Title = GetIDTag(content, "ti");
            if (idTag.Artist == String.Empty)
                idTag.Artist = GetIDTag(content, "ar");
            if (idTag.Album == String.Empty)
                idTag.Album = GetIDTag(content, "al");

            idTag.LyricAuthor = GetIDTag(content, "by");

            var lyrics = Regex.Matches(content, @"\[(\d{2}):(\d{2})\..*(\d{2})\](.*)");
            if (lyrics.Count != 0)
            {
                foreach (Match item in lyrics)
                {
                    int i = 0;
                    int min = Int32.Parse(item.Groups[1].Value);
                    int souconds = Int32.Parse(item.Groups[2].Value);
                    int ms = Int32.Parse(item.Groups[3].Value) * 10;
                    var time = new TimeSpan(0, 0, min, souconds, ms);
                    lyricContent.Add(new Lyric { ID = i++, Time = time, Content = item.Groups[4].Value.Trim() });
                }
            }
            else if (content != String.Empty && content != null)
            {
                string[] lines = content.Split('\n');
                foreach (var line in lines)
                {
                    lyricContent.Add(new Lyric { Time = new TimeSpan(), Content = line.Trim() });
                }
            }
            
            return true;
        }
        public static async Task<bool> SaveLyric(ObservableCollection<Lyric> lyricList,LyricIDTag tag)
        {
            if (ThisLRCFile is null)
            {
                FileSavePicker picker = new FileSavePicker();
                picker.SuggestedFileName = "New_LRC_File";
                picker.DefaultFileExtension = ".lrc";
                picker.FileTypeChoices.Add("LRC File", new List<string> { ".lrc" });

                ThisLRCFile = await picker.PickSaveFileAsync();
                if (ThisLRCFile is null)
                    return false;
            }
            string lyric_str = String.Empty;
            lyric_str = tag.ToString() + "\r\n\r\n";
            foreach (var item in lyricList)
            {
                lyric_str += item.ToString() + "\r\n";
            }
            await FileIO.WriteTextAsync(ThisLRCFile, lyric_str);
            return true;
        }
        private static string GetIDTag(string content,string tagName)
        {
            string result = String.Empty;
            Match match = Regex.Match(content, $@"\[{tagName}:(.*)\]");
            if (match.Success)
                result = match.Groups[1].Value;
            return result;
        }
    }
}
