using LyricsEditor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsEditor.EventArg
{
    public class MusicChanageEventArgs : EventArgs
    {
        public Music NewMusic { get; set; }
    }
}
