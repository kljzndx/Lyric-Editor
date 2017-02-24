using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LyricsEditor.Model
{
    public class LyricFileChanageEventArgs : EventArgs
    {
        public IStorageFile File { get; set; }
    }
}
