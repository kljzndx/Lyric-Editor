using System;
using Windows.System;

namespace SimpleLyricsEditor.Events
{
    public static class GlobalKeyNotifier
    {
        public static event EventHandler<GlobalKeyEventArgs> KeyDown;
        public static event EventHandler<GlobalKeyEventArgs> KeyUp;

        public static void PressKey(VirtualKey key, bool isPressCtrl, bool isPressShift, bool isInputing)
        {
            KeyDown?.Invoke(null, new GlobalKeyEventArgs(key, isPressCtrl, isPressShift, isInputing));
        }

        public static void ReleaseKey(VirtualKey key, bool isPressCtrl, bool isPressShift, bool isInputing)
        {
            KeyUp?.Invoke(null, new GlobalKeyEventArgs(key, isPressCtrl, isPressShift, isInputing));
        }
    }
}