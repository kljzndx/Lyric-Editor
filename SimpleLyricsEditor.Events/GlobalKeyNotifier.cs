using System;
using Windows.System;

namespace SimpleLyricsEditor.Events
{
    public static class GlobalKeyNotifier
    {
        public static event EventHandler<GlobalKeyEventArgs> KeyDown;
        public static event EventHandler<GlobalKeyEventArgs> KeyUp;

        public static void PressKey(VirtualKey key, bool isPressCtrl, bool isPressShift)
        {
            KeyDown?.Invoke(null, new GlobalKeyEventArgs(key, isPressCtrl, isPressShift));
        }

        public static void ReleaseKey(VirtualKey key, bool isPressCtrl, bool isPressShift)
        {
            KeyUp?.Invoke(null, new GlobalKeyEventArgs(key, isPressCtrl, isPressShift));
        }
    }
}