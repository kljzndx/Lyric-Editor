using System;
using SimpleLyricsEditor.IDAL;

namespace SimpleLyricsEditor.DAL
{
    public class Lyric : ObservableObject
    {
        private string _content;
        private TimeSpan _time;

        public TimeSpan Time
        {
            get => _time;
            set => Set(ref _time, value);
        }

        public string Content
        {
            get => _content;
            set => Set(ref _content, value);
        }
    }
}