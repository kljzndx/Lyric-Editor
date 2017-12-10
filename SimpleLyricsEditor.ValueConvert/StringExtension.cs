using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.UI;

namespace SimpleLyricsEditor.ValueConvert
{
    public static class StringExtension
    {
        public static Color HexColorToColor(this string hexColor)
        {
            Regex regex = new Regex(@"#(?<A>[0-9a-fA-F]{2})(?<R>[0-9a-fA-F]{2})(?<G>[0-9a-fA-F]{2})(?<B>[0-9a-fA-F]{2})");
            Match match = regex.Match(hexColor);
            if (!match.Success)
                throw new Exception("此字符串不是有效的16进制颜色值" +
                                    Environment.NewLine +
                                    "有效的颜色值示例如下" +
                                    Environment.NewLine +
                                    "#FF000000");

            GroupCollection groups = match.Groups;
            List<byte> bytes = new List<byte>();

            for (int i = 1; i < groups.Count; i++)
                bytes.Add(System.Convert.ToByte(groups[i].Value, 16));

            return Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]);
        }
    }
}