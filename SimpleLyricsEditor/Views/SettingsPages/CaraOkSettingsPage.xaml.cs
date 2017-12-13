using System;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using SimpleLyricsEditor.Models;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleLyricsEditor.Views.SettingsPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CaraOkSettingsPage : SettingsPageBase
    {
        private Action _changeCaraOkEffectColor;

        public CaraOkSettingsPage() : base()
        {
            this.InitializeComponent();

            _changeCaraOkEffectColor =  () =>
                _settings.CaraOkEffectColor = HslToColor(Hue_Slider.Value, Saturation_Slider.Value / 100, Lightness_Slider.Value / 100);

            var hsl = ColorToHsl(_settings.CaraOkEffectColor);
            Hue_Slider.Value = hsl.hue;
            Saturation_Slider.Value = hsl.saturation * 100;
            Lightness_Slider.Value = hsl.lightness * 100;
        }

        private Color HslToColor(double hue, double saturation, double lightness)
        {
            double colorness = lightness * saturation;
            double hueCalibration = hue / 60;
            double x = colorness * (1 - Math.Abs(hueCalibration % 2 - 1));
            double light = lightness - colorness;
            double red = 0D, green = 0D, blue = 0D;

            if (hueCalibration < 1)
            {
                red = colorness;
                green = x;
            }
            else if (hueCalibration < 2)
            {
                red = x;
                green = colorness;
            }
            else if (hueCalibration < 3)
            {
                green = colorness;
                blue = x;
            }
            else if (hueCalibration < 4)
            {
                green = x;
                blue = colorness;
            }
            else if (hueCalibration < 5)
            {
                red = x;
                blue = colorness;
            }
            else
            {
                red = colorness;
                blue = x;
            }

            return Color.FromArgb(255, (byte)(255 * (red + light)), (byte)(255 * (green + light)), (byte)(255 * (blue + light)));
        }

        private (double hue, double saturation, double lightness) ColorToHsl(Color color)
        {
            double hue = 0D, saturation = 0D, lightness = 0D;
            double red = color.R / 255D, green = color.G / 255D, blue = color.B / 255D;
            double min = Math.Min(Math.Min(red, green), blue), max = Math.Max(Math.Max(red, green), blue);
            double difference = max - min, total = max + min;

            lightness = total / 2;

            if (difference != 0)
            {
                double redDifference = (max - red) / difference;
                double greenDifference = (max - green) / difference;
                double blueDifference = (max - blue) / difference;

                if (lightness < 0.5)
                    saturation = difference / total;
                else
                    saturation = difference / (2 - total);

                if (red.Equals(max))
                    hue = blueDifference - greenDifference;
                else if (green.Equals(max))
                    hue = 2 + redDifference - blueDifference;
                else if (blue.Equals(max))
                    hue = 4 + greenDifference - redDifference;

                hue *= 60;

                if (hue < 0)
                    hue += 360;
                else if (hue >= 360)
                    hue -= 360;
            }

            return (hue, saturation, lightness);
        }
    }
}
