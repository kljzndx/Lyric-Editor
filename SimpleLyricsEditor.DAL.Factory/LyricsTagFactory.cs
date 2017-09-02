using System.Collections.Generic;
using HappyStudio.UwpToolsLibrary.Information;

namespace SimpleLyricsEditor.DAL.Factory
{
    public static class LyricsTagFactory
    {
        public static List<LyricsTag> CreateTags()
        {
            var list = new List<LyricsTag>
            {
                new LyricsTag("ti"),
                new LyricsTag("ar"),
                new LyricsTag("al"),
                new LyricsTag("by"),
                new LyricsTag("re", AppInfo.Name + " for UWP"),
                new LyricsTag("ve", AppInfo.Version)
            };
            return list;
        }
    }
}