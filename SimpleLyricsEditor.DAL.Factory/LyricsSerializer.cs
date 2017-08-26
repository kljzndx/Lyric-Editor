using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleLyricsEditor.DAL.Factory
{
    public class LyricsSerializer
    {
        private static readonly Regex _rege =
            new Regex(@"\[(?<min>\d{2}):(?<sec>\d{2}).(?<ms>\d{2, 3})\]\s?(?<content>.*)");

        public static string Serialization(IList<Lyric> lyrics, IList<LyricsTag> tags)
        {
            var builder = new StringBuilder();

            foreach (var tag in tags)
                builder.AppendLine(tag.ToString());

            builder.AppendLine();

            foreach (var lyric in lyrics)
                builder.AppendLine(lyric.ToString());

            return builder.AppendLine().ToString();
        }

        public static (List<Lyric> lyrics, List<LyricsTag> tags) Deserialization(IList<string> inputs)
        {
            var items = new List<Lyric>();
            var tags = new List<LyricsTag>
            {
                new LyricsTag("ti"),
                new LyricsTag("tr"),
                new LyricsTag("al"),
                new LyricsTag("by"),
                new LyricsTag("re"),
                new LyricsTag("ve")
            };

            var builder = new StringBuilder();
            Lyric item = null;
            var isMatchTag = true;

            foreach (var input in inputs.SkipWhile(string.IsNullOrWhiteSpace))
            {
                if (isMatchTag)
                {
                    var tagB = false;
                    foreach (var tag in tags.TakeWhile(t => string.IsNullOrEmpty(t.TagValue)))
                        if (tag.GeiTag(input))
                            tagB = true;
                    if (tagB)
                        continue;
                }

                var match = _rege.Match(input);

                if (match.Success)
                {
                    if (item != null)
                    {
                        item.Content = builder.ToString();
                        items.Add(item);
                    }

                    var groups = match.Groups;

                    var min = int.Parse(groups["min"].Value);
                    var sec = int.Parse(groups["sec"].Value);
                    var ms = int.Parse(groups["ms"].Value);

                    builder.Clear();
                    item = new Lyric(new TimeSpan(0, 0, min, sec, groups["ms"].Value.Length == 2 ? ms * 10 : ms));
                    builder.AppendLine(groups["content"].Value.Trim());
                    isMatchTag = false;
                }
                else if (item != null)
                {
                    builder.AppendLine(input.Trim());
                }
                else
                {
                    items.Add(new Lyric(TimeSpan.Zero, input));
                    isMatchTag = false;
                }
            }

            if (item != null)
                items.Add(item);

            return (items, tags.SkipWhile(t => string.IsNullOrEmpty(t.TagValue)).ToList());
        }
    }
}