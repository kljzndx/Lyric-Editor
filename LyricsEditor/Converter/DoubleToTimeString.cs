using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace LyricsEditor.Converter
{
    class DoubleToTimeString:IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            string result = String.Empty;
            if (value is double dob)
            {
                if (dob > 0.0001)
                {
                    var time = TimeSpan.FromMinutes(dob);
                    result = $"{time.Minutes.ToString("D2")}:{time.Seconds.ToString("D2")}.{time.Milliseconds.ToString("D3").Remove(2)}";
                }
                else result = "00:00.00";
                
            }
            return result;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
