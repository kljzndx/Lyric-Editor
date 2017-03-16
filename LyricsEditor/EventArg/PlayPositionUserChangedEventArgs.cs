using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsEditor.EventArg
{
    public class PlayPositionUserChangeEventArgs : EventArgs
    {
        public bool IsChanging { get; set; }
        public TimeSpan Time { get; set; }
    }
}
