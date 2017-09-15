using System;
using Windows.System;

namespace SimpleLyricsEditor.Events
{
    public class GlobalKeyEventArgs : EventArgs
    {
        public GlobalKeyEventArgs(VirtualKey key, bool isPressCtrl, bool isPressShift)
        {
            Key = key;
            IsPressCtrl = isPressCtrl;
            IsPressShift = isPressShift;
        }

        public VirtualKey Key { get; set; }
        public bool IsPressCtrl { get; set; }
        public bool IsPressShift { get; set; }
    }
}