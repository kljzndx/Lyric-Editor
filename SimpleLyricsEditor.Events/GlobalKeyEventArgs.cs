using System;
using Windows.System;

namespace SimpleLyricsEditor.Events
{
    public class GlobalKeyEventArgs : EventArgs
    {
        public GlobalKeyEventArgs(VirtualKey key, bool isPressCtrl, bool isPressShift, bool isInputing)
        {
            Key = key;
            IsPressCtrl = isPressCtrl;
            IsPressShift = isPressShift;
            IsInputing = isInputing;
        }

        public VirtualKey Key { get; }
        public bool IsPressCtrl { get; }
        public bool IsPressShift { get; }
        public bool IsInputing { get; }
    }
}