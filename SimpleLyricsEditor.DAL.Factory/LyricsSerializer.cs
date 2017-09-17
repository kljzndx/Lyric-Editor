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

        public static (IEnumerable<Lyric> lyrics, IEnumerable<LyricsTag> tags) Deserialization(
            IEnumerable<string> lines)
        {
            List<Lyric> items = new List<Lyric>();
            List<LyricsTag> tags = LyricsTagFactory.CreateTags();
            StringBuilder builder = new StringBuilder();
            Lyric item = null;
            bool isMatchTag = true;
            var strs = lines.Where(l => !string.IsNullOrWhiteSpace(l));

            /* 基本思路如下
             * 
             * 1，
             */

            foreach (var line in strs)
            {
                // 检查是否需要匹配Tag
                if (isMatchTag)
                {
                    // 取出空白Tag
                    var spaceTags = tags.Where(t => string.IsNullOrEmpty(t.TagValue));
                    bool isSuccess = false;

                    foreach (var tag in spaceTags)
                        if (!isSuccess)
                           isSuccess = tag.GeiTag(line.ToLower());

                    if (isSuccess)
                        continue;
                }

                // 进行匹配当前行
                var match = LyricRegex.Match(line);

                // 检查是否匹配成功
                if (match.Success)
                {
                    /*
                     * 判断上一个歌词项是否存在
                     * 如果存在则添加进歌词列表
                     */
                    if (item != null)
                    {
                        item.Content = builder.ToString().Trim();
                        items.Add(item);
                    }

                    // 获取匹配到的字符串
                    var groups = match.Groups;

                    // 提取分，秒和毫秒
                    int min = int.Parse(groups["min"].Value);
                    int sec = int.Parse(groups["sec"].Value);
                    int ms = int.Parse(groups["ms"].Value);

                    item = new Lyric(new TimeSpan(0, 0, min, sec, groups["ms"].Value.Length == 2 ? ms * 10 : ms));
                    
                    builder.Clear();
                    builder.AppendLine(groups["content"].Value);

                    // 停止匹配Tag
                    isMatchTag = false;
                }

                /* 
                 * 如果已经匹配到一个歌词项
                 * 则添加一行内容
                 */
                else if (item != null)
                {
                    builder.AppendLine(line);
                }

                /*
                 * 如果没有匹配到歌词且当前行也不是Tag
                 * 则直接添加进列表
                 */
                else if (!TagRegex.IsMatch(line))
                {
                    items.Add(new Lyric(TimeSpan.Zero, line));
                    // 停止匹配Tag
                    isMatchTag = false;
                }
            }

            // 添加最后一个歌词项
            if (item != null)
            {
                item.Content = builder.ToString().Trim();
                items.Add(item);
            }

            return (items, tags);
        }
    }
}