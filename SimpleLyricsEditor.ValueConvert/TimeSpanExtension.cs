using System;

namespace SimpleLyricsEditor.ValueConvert
{
    public static class TimeSpanExtension
    {
        public static string ToLyricTimeString(this TimeSpan time)
        {
            return $"{time.Minutes:D2}:{time.Seconds:D2}.{time.Milliseconds.ToString("D3").Remove(2)}";
        }
    }
}