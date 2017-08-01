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

            var rege = new Regex(@"\[(\d{2}):(\d{2}).(\d{2,3})\]\s*(.*)");
            var lines = content.Split('\n');
            var builder = new StringBuilder();

            if (rege.IsMatch(content))
            {
                LyricItem item = LyricItem.Empty;
                foreach (string line in lines)
                {
                    Match match = rege.Match(line);
                    if (match.Success)
                    {
                        if (!item.Equals(LyricItem.Empty))
                        {
                            item.Content = builder.ToString().Trim();
                            lyrics.Add(item);
                            builder.Clear();
                        }

                        byte min = Byte.Parse(match.Groups[1].Value);
                        byte s = Byte.Parse(match.Groups[2].Value);
                        byte ms = Byte.Parse(match.Groups[3].Value);
                        item = new LyricItem()
                        {
                            Time = new TimeSpan(0, 0, min, s, ms < 100 ? ms * 10 : ms)
                        };

                        builder.AppendLine(match.Groups[4].Value.Trim());
                    }
                    else if(!item.Equals(LyricItem.Empty))
                        builder.AppendLine(line);
                }

                //添加最后一个歌词
                item.Content = builder.ToString().Trim();
                lyrics.Add(item);
            }
            else
                foreach (string item in lines)
                    lyrics.Add(new LyricItem { Time = TimeSpan.Zero, Content = item.Trim() });

            return lyrics;
        }

        public static string PrintLyric(LyricTags tags, Collection<LyricItem> lyrics)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(tags.ToString());
            sb.AppendLine(String.Empty);

            foreach (Lyric item in lyrics)
                sb.AppendLine(item.ToString().Trim());

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
