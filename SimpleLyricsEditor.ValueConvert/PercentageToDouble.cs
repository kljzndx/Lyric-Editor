using System;
using Windows.UI.Xaml.Data;

namespace SimpleLyricsEditor.ValueConvert
{
    public class PercentageToDouble : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double)
                return (double) value * 100;
            return 0D;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double)
                return (double) value / 100;
            return 0D;
        }
    }
}