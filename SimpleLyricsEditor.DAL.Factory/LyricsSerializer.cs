using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleLyricsEditor.DAL.Factory
{
    public static class LyricsSerializer
    {
        private static readonly Regex TagRegex = new Regex(@"\[.*:.*\]");
        private static readonly Regex LyricRegex =
            new Regex(@"\[(?<min>\d{2}):(?<sec>\d{2})\.(?<ms>\d{2,3})\]\s?(?<content>.*)");

        public static string Serialization(IEnumerable<Lyric> lyrics, IEnumerable<LyricsTag> tags)
        {
            var builder = new StringBuilder();

            foreach (var tag in tags)
                builder.AppendLine(tag.ToString());

            builder.AppendLine();

            foreach (var lyric in lyrics)
                builder.AppendLine(lyric.ToString());

            return builder.AppendLine().ToString();
        }

        public static (IEnumerable<Lyric> lyrics, IEnumerable<LyricsTag> tags) Deserialization(IEnumerable<string> lines)
        {
            var items = new List<Lyric>();
            var tags = LyricsTagFactory.CreateTags();
            var builder = new StringBuilder();
            Lyric item = null;
            var isMatchTag = true;
            var strs = lines.Where(l => !String.IsNullOrWhiteSpace(l));
            
            foreach (var line in strs)
            {
                if (isMatchTag)
                {
                    var spaceTags = tags.Where(t => String.IsNullOrEmpty(t.TagValue));
                    foreach (var tag in spaceTags)
                        tag.GeiTag(line.ToLower());
                }

                var match = LyricRegex.Match(line);

                if (match.Success)
                {
                    if (item != null)
                    {
                        item.Content = builder.ToString().Trim();
                        items.Add(item);
                    }

                    var groups = match.Groups;

                    var min = int.Parse(groups["min"].Value);
                    var sec = int.Parse(groups["sec"].Value);
                    var ms = int.Parse(groups["ms"].Value);

                    builder.Clear();
                    item = new Lyric(new TimeSpan(0, 0, min, sec, groups["ms"].Value.Length == 2 ? ms * 10 : ms));
                    builder.AppendLine(groups["content"].Value);
                    isMatchTag = false;
                }
                else if (item != null)
                {
                    builder.AppendLine(line);
                }
                else if (TagRegex.IsMatch(line))
                {
                    continue;
                }
                else
                {
                    items.Add(new Lyric(TimeSpan.Zero, line));
                    isMatchTag = false;
                }
            }

            if (item != null)
                items.Add(item);

            return (items, tags);
        }
    }
}