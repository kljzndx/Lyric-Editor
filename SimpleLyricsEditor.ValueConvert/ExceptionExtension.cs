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

            builder.AppendLine($"{CharacterLibrary.ErrorTable.GetString("Code")} 0x{ex.HResult:X}");
            builder.AppendLine($"{CharacterLibrary.ErrorTable.GetString("Information")} {ex.Message}");

            return builder.ToString().Trim();
        }

        public static string ToLongString(this Exception ex)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"{CharacterLibrary.ErrorTable.GetString("Code")} 0x{ex.HResult:X}");
            builder.AppendLine($"{CharacterLibrary.ErrorTable.GetString("Information")} {ex.Message}");
            builder.AppendLine($"{CharacterLibrary.ErrorTable.GetString("Source")} {ex.Source}");
            builder.AppendLine($"{CharacterLibrary.ErrorTable.GetString("HelpLink")}: {ex.HelpLink}");
            builder.AppendLine($"{CharacterLibrary.ErrorTable.GetString("Ohter")} {ex.Data}");
            builder.AppendLine(CharacterLibrary.ErrorTable.GetString("StackTrace"));
            builder.AppendLine(ex.StackTraceEx());

            return builder.ToString().Trim();
        }
    }
}