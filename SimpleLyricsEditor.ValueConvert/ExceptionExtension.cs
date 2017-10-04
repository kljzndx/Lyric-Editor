using System;
using System.Text;
using SimpleLyricsEditor.Core;

namespace SimpleLyricsEditor.ValueConvert
{
    public static class ExceptionExtension
    {
        public static string ToShortString(this Exception ex)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"{CharacterLibrary.Error.GetString("Code")} 0x{ex.HResult:X}");
            builder.AppendLine($"{CharacterLibrary.Error.GetString("Information")} {ex.Message}");

            return builder.ToString().Trim();
        }

        public static string ToLongString(this Exception ex)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"{CharacterLibrary.Error.GetString("Code")} 0x{ex.HResult:X}");
            builder.AppendLine($"{CharacterLibrary.Error.GetString("Information")} {ex.Message}");
            builder.AppendLine($"{CharacterLibrary.Error.GetString("Source")} {ex.Source}");
            builder.AppendLine($"{CharacterLibrary.Error.GetString("HelpLink")}: {ex.HelpLink}");
            builder.AppendLine($"{CharacterLibrary.Error.GetString("Ohter")} {ex.Data}");
            builder.AppendLine(CharacterLibrary.Error.GetString("StackTrace"));
            builder.AppendLine(ex.StackTraceEx());

            return builder.ToString().Trim();
        }
    }
}