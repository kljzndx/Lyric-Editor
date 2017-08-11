using System;
using Windows.UI.Xaml.Data;

namespace SimpleLyricsEditor.ValueConvert
{
    public class TimeSpanToLyricTimeString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((TimeSpan) value).ToLyricTimeString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}