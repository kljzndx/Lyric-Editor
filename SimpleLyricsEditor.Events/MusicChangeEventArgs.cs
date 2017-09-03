using System;
using SimpleLyricsEditor.DAL;

namespace SimpleLyricsEditor.Events
{
    public class MusicChangeEventArgs : EventArgs
    {
        public MusicChangeEventArgs(Music source)
        {
            Source = source;
        }

        public Music Source { get; set; }
    }
}