using SimpleLyricEditor.Models;
using SimpleLyricEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleLyricEditor.Tools
{
    public static class LyricTools
    {
        public static ObservableCollection<LyricItem> LrcParse(string content, LyricTags tags)
        {
            ObservableCollection<LyricItem> lyrics = new ObservableCollection<LyricItem>();

            if (String.IsNullOrEmpty(tags.SongName))
                tags.SongName = GetTagValue(content, "ti");
            if (String.IsNullOrEmpty(tags.Artist))
                tags.Artist = GetTagValue(content, "ar");
            if (String.IsNullOrEmpty(tags.Album))
                tags.Album = GetTagValue(content, "al");
            if (String.IsNullOrEmpty(tags.LyricsAuthor))
                tags.LyricsAuthor = GetTagValue(content, "by");

            Regex rege = new Regex(@"\[(\d{1,2}):(\d{1,2}).(\d{2,3})\](.*)");
            if (rege.IsMatch(content))
            {
                foreach (Match item in rege.Matches(content))
                {
                    byte min = Byte.Parse(item.Groups[1].Value);
                    byte s = Byte.Parse(item.Groups[2].Value);
                    byte ms =  Byte.Parse(item.Groups[3].Value);
                    lyrics.Add(new LyricItem { Time = new TimeSpan(0, 0, min, s, ms < 100 ? ms * 10 : ms), Content = item.Groups[4].Value.Trim() });
                }
            }
            else
                foreach (string item in content.Split('\n'))
                    lyrics.Add(new LyricItem { Time = TimeSpan.Zero, Content = item.Trim() });

            return lyrics;
        }

        public static string PrintLyric(LyricTags tags, Collection<LyricItem> lyrics)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(tags.ToString());
            sb.AppendLine(String.Empty);

            foreach (Lyric item in lyrics)
                sb.AppendLine(item.ToString());

            return sb.ToString();
        }

        private static string GetTagValue(string input, string tagName)
        {
            Regex rege = new Regex($@"\[{tagName}:(.*)\]");
            var tagvalue = rege.Match(input);
            return tagvalue.Success ? tagvalue.Groups[1].Value : String.Empty;
        }
    }
}
