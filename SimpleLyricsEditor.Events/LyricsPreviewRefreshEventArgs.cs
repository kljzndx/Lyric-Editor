using System;
using SimpleLyricsEditor.DAL;

namespace SimpleLyricsEditor.Events
{
    public class LyricsPreviewRefreshEventArgs : EventArgs
    {
        public LyricsPreviewRefreshEventArgs(Lyric currentLyric)
        {
            CurrentLyric = currentLyric;
        }

        public Lyric CurrentLyric { get; set; }
    }
}