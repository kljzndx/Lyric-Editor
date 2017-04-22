using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleLyricEditor.EventArgss
{
    public class LyricFileChangeEventArgs : EventArgs
    {
        public StorageFile NewFile { get; set; }

        public LyricFileChangeEventArgs(StorageFile file)
        {
            NewFile = file;
        }
    }
}
