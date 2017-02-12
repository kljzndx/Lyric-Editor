using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace LyricsEditor.Converter
{
    class TimeSpenToString:IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan time)
            {
                return $"{time.Minutes.ToString("D2")}:{time.Seconds.ToString("D2")}.{time.Milliseconds.ToString("D3").Remove(2)}";
            }
            else return "数据非法";
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
