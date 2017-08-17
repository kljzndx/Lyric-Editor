using System;
using Windows.UI.Xaml.Data;

namespace SimpleLyricsEditor.ValueConvert
{
    public class DoubleToLyricTimeString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return TimeSpan.FromMinutes((double) value).ToLyricTimeString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}