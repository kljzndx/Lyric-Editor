using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace LyricsEditor.Converter
{
    class Percentage_Double:IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            return (double)value * 100;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (double)value / 100;
        }
    }
}
