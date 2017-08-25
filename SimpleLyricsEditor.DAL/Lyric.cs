using System;
using SimpleLyricsEditor.IDAL;

namespace SimpleLyricsEditor.DAL
{
    public class Lyric : ObservableObject, IComparable<Lyric>
    {
        private string _content;
        private TimeSpan _time;

        public Lyric(TimeSpan time, string content)
        {
            _time = time;
            _content = content;
        }

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

        public int CompareTo(Lyric other)
        {
            return this.Time.CompareTo(other.Time);
        }
    }
}