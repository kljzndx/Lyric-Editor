using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.UI;

namespace SimpleLyricsEditor.ValueConvert
{
    public static class StringExtension
    {
        public static Color HexColorToColor(this string hexColor)
        {
            Regex regex = new Regex(@"#(?<A>.{2})(?<R>.{2})(?<G>.{2})(?<B>.{2})");
            Match match = regex.Match(hexColor);
            List<byte> bytes = new List<byte>();

            foreach (Group item in match.Groups)
                bytes.Add(System.Convert.ToByte(item.Value, 16));

            return Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]);
        }
    }
}