using SimpleLyricEditor.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace SimpleLyricEditor.ValueConverters
{
    class TimeSpanToString:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            TimeSpan time = value is double d ? TimeSpan.FromMinutes(d) : (TimeSpan)value;
            return time.ToLyricTimeString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
