using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace SimpleLyricEditor.Models
{
    public class LyricTags : ObservableObject
    {
        private string songName;
        public string SongName { get => songName; set => Set(ref songName, value); }

        private string artist;
        public string Artist { get => artist; set => Set(ref artist, value); }

        private string album;
        public string Album { get => album; set => Set(ref album, value); }

        private string lyricsAuthor;
        public string LyricsAuthor { get => lyricsAuthor; set => Set(ref lyricsAuthor, value); }


        public LyricTags()
        {
            songName = String.Empty;
            artist = String.Empty;
            album = String.Empty;
            lyricsAuthor = String.Empty;
        }

        public string WriteTag(string tagName, string value)
        {
            return !String.IsNullOrEmpty(value) ? $"[{tagName}:{value}]\r\n" : String.Empty;
        }
        public override string ToString()
        {
            string tags = String.Empty;
            tags += WriteTag("ti", songName);
            tags += WriteTag("ar", artist);
            tags += WriteTag("al", album);
            tags += WriteTag("by", lyricsAuthor);
            return tags.Trim();
        }
    }
}
