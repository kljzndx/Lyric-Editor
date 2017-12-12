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

        public CaraOkSettingsPage()
        {
            this.InitializeComponent();
            _changeCaraOkEffectColor =  () =>
                _settings.CaraOkEffectColor = HslToColor(Math.Abs(Hue_Slider.Value), Saturation_Slider.Value / 100, Lightness_Slider.Value / 100);
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
    }
}
