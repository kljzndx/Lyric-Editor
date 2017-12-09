using Windows.UI;

namespace SimpleLyricsEditor.ValueConvert
{
    public static class ColorExtension
    {
        public static string ToHexString(this Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}