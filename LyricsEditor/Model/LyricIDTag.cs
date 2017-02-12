using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsEditor.Model
{
    public class LyricIDTag : Auxiliary
    {
        private string title, artist, album, lyricAuthor;

        public string Title { get => title; set { SetProperty(ref title, value); } }
        public string Artist { get => artist; set { SetProperty(ref artist, value); } }
        public string Album { get => album; set { SetProperty(ref album, value); } }
        public string LyricAuthor { get => lyricAuthor; set { SetProperty(ref lyricAuthor, value); } }


        public LyricIDTag()
        {
            title = String.Empty;
            artist = String.Empty;
            album = String.Empty;
            lyricAuthor = String.Empty;
        }
        public override string ToString()
        {
            string result = String.Empty;
            result += WriteIDTag("ti", title);
            result += WriteIDTag("ar", artist);
            result += WriteIDTag("al", album);
            result += WriteIDTag("by", lyricAuthor);
            return result.Trim();
        }
        public string WriteIDTag(string tagName, string Content)
        {
            string result = String.Empty;
            if (Content != String.Empty)
            {
                result = $"[{tagName}:{Content}]\r\n";
            }
            return result;
        }
    }
}
