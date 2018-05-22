using System.Linq;
using Windows.UI;

namespace SimpleLyricsEditor.ValueConvert
{
    public static class ColorExtension
    {
        public static string ToHexString(this Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public static (double hue, double saturation, double value) ToHsv(this Color color)
        {
            double hue = 0D, saturation = 0D, value = 0D;
            double r = color.R / 255D, g = color.G / 255D, b = color.B / 255D;

            double max = new[] { r, g, b }.Max();
            double min = new[] { r, g, b }.Min();
            double delta = max - min;

            saturation = max != 0 ? delta / max : 0;
            value = max;

            if (saturation == 0)
                return (0, 0, 0);

            if (r == max)
                hue = ((g - b) / delta);
            else if (g == max)
                hue = ((b - r) / delta) + 2.0;
            else if (b == max)
                hue = ((r - g) / delta) + 4.0;

            hue *= 60.0;

            if (hue < 0)
                hue += 360.0;

            return (hue, saturation, value);
        }
    }
}