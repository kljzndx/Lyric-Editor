using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace LyricsEditor.Model
{
    public static class LyricManager
    {
        public static void LrcAnalysis(string content, IList<Lyric> lyricContent, LyricIDTag idTag)
        {
            lyricContent.Clear();

            if (String.IsNullOrEmpty(idTag.Title))
                idTag.Title = GetIDTag(content, "ti");
            if (String.IsNullOrEmpty(idTag.Artist))
                idTag.Artist = GetIDTag(content, "ar");
            if (String.IsNullOrEmpty(idTag.Album))
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
            else if (!String.IsNullOrEmpty(content))
            {
                string[] lines = content.Split('\n');
                foreach (var line in lines)
                {
                    lyricContent.Add(new Lyric { Time = new TimeSpan(), Content = line.Trim() });
                }
            }
        }

        public static async Task<bool> SaveLyric(IList<Lyric> lyricList,LyricIDTag tag)
        {
            StorageFile lrcFile = null;
            if (LyricFileManager.ThisLRCFile is null)
            {
                FileSavePicker picker = new FileSavePicker();
                picker.SuggestedFileName = "New_LRC_File";
                picker.DefaultFileExtension = ".lrc";
                picker.FileTypeChoices.Add("LRC File", new List<string> { ".lrc" });

                lrcFile = await picker.PickSaveFileAsync();
                if (lrcFile is null)
                    return false;
            }
            else
                lrcFile = LyricFileManager.ThisLRCFile as StorageFile;

            string lyric_str = String.Empty;
            lyric_str = tag.ToString() + "\r\n\r\n";
            foreach (var item in lyricList)
            {
                lyric_str += item.ToString() + "\r\n";
            }
            await FileIO.WriteTextAsync(lrcFile, lyric_str);
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
