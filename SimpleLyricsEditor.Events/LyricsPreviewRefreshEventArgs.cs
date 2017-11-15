using System;

namespace SimpleLyricsEditor.Events
{
    public class LyricsPreviewRefreshEventArgs : EventArgs
    {
        public LyricsPreviewRefreshEventArgs(string currentLyric)
        {
            CurrentLyric = currentLyric;
        }

        public string CurrentLyric { get; set; }
    }
}