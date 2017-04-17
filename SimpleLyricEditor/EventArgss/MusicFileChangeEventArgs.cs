using SimpleLyricEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLyricEditor.EventArgss
{
    public class MusicFileChangeEventArgs : EventArgs
    {
        public Music NewMusic { get; set; }

        public MusicFileChangeEventArgs(Music music)
        {
            NewMusic = music;
        }
    }
}
