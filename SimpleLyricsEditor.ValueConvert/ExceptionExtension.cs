using System;
using System.Text;

namespace SimpleLyricsEditor.ValueConvert
{
    public static class ExceptionExtension
    {
        public static string ToLongString(this Exception ex)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"Error code: {ex.HResult:X}");
            builder.AppendLine($"Error Info: {ex.Message}");
            builder.AppendLine($"Error source: {ex.Source}");
            builder.AppendLine($"Help link: {ex.HelpLink}");
            builder.AppendLine($"Ohter message: {ex.Data}");
            builder.AppendLine("Stack trare:");
            builder.AppendLine(ex.StackTraceEx());

            return builder.ToString();
        }
    }
}