using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLyricEditor.Extensions
{
    public static class TimeSpan_Extension
    {
        public static string ToLyricTimeString(this TimeSpan time)
        {
            return $"{time.Minutes.ToString("D2")}:{time.Seconds.ToString("D2")}.{time.Milliseconds.ToString("D3").Remove(2)}";
        }
    }
}
