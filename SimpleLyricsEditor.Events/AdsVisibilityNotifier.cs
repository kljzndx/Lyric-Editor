using System;

namespace SimpleLyricsEditor.Events
{
    public static class AdsVisibilityNotifier
    {
        public static event EventHandler DisplayRequested;
        public static event EventHandler HideRequested;

        public static void DisplayAdsRequest()
        {
            DisplayRequested?.Invoke(null, EventArgs.Empty);
        }

        public static void HideAdsRequest()
        {
            HideRequested?.Invoke(null, EventArgs.Empty);
        }
    }
}