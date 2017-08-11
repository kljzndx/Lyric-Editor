using System;
using Windows.UI.Xaml.Data;

namespace SimpleLyricsEditor.ValueConvert
{
    public class TimeSpanToDouble : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((TimeSpan) value).TotalMinutes;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return TimeSpan.FromMinutes((double) value);
        }
    }
}