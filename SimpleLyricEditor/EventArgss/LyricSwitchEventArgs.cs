using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleLyricEditor.ViewModels;

namespace SimpleLyricEditor.EventArgss
{
    public class LyricSwitchEventArgs : EventArgs
    {
        public LyricItem CurrentLyric { get; set; }

        public LyricSwitchEventArgs(LyricItem lyric)
        {
            CurrentLyric = lyric;
        }
    }
}
