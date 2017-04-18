using SimpleLyricEditor.Models;
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
        public static ObservableCollection<Lyric> LrcParse(string content, LyricTags tags)
        {
            ObservableCollection<Lyric> lyrics = new ObservableCollection<Lyric>();

            tags.SongName = String.IsNullOrEmpty(tags.SongName) ? GetTagValue(content, "ti") : String.Empty;
            tags.Artist = String.IsNullOrEmpty(tags.Artist) ? GetTagValue(content, "ar") : String.Empty;
            tags.Album = String.IsNullOrEmpty(tags.Album) ? GetTagValue(content, "al") : String.Empty;
            tags.LyricsAuthor = String.IsNullOrEmpty(tags.LyricsAuthor) ? GetTagValue(content, "by") : String.Empty;

            Regex rege = new Regex(@"\[(\d{2}):(\d{2}).(\d{2})\](.*)");
            if (rege.IsMatch(content))
            {
                foreach (Match item in rege.Matches(content))
                {
                    byte min = Byte.Parse(item.Groups[1].Value);
                    byte s = Byte.Parse(item.Groups[2].Value);
                    byte ms = Byte.Parse(item.Groups[3].Value);
                    lyrics.Add(new Lyric { Time = new TimeSpan(0, 0, min, s, ms * 10), Content = item.Groups[4].Value.Trim() });
                }
            }
            else
                foreach (string item in content.Split('\n'))
                    lyrics.Add(new Lyric { Time = TimeSpan.Zero, Content = item.Trim() });

            return lyrics;
        }

        public static string PrintLyric(LyricTags tags, Collection<Lyric> lyrics)
        {
            string result = String.Empty;
            result += tags.ToString() + "\r\n\r\n";

            foreach (Lyric item in lyrics)
                result += item.ToString() + "\r\n";

            return result.Trim();
        }

        private static string GetTagValue(string input,string tagName)
        {
            Regex rege = new Regex($@"\[{tagName}:(.*)\]");
            var tagvalue = rege.Match(input);
            return tagvalue.Success ? tagvalue.Groups[1].Value : String.Empty;
        }
    }
}
