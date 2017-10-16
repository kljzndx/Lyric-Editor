using System;

namespace SimpleLyricsEditor.Events
{
    public static class AdsVisibilityNotifier
    {
        public static event EventHandler Displayed;
        public static event EventHandler Hided;

        public static void DisplayAds()
        {
            Displayed?.Invoke(null, EventArgs.Empty);
        }

        public static void HideAds()
        {
            Hided?.Invoke(null, EventArgs.Empty);
        }
    }
}