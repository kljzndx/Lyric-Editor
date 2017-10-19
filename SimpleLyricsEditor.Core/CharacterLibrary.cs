using Windows.ApplicationModel.Resources;

namespace SimpleLyricsEditor.Core
{
    public static class CharacterLibrary
    {
        public static ResourceLoader MessageBox => ResourceLoader.GetForCurrentView("MessageBox");
        public static ResourceLoader ErrorTable => ResourceLoader.GetForCurrentView("ErrorTable");
        public static ResourceLoader ErrorInfo => ResourceLoader.GetForCurrentView("ErrorInfo");
        public static ResourceLoader SettingsRoot => ResourceLoader.GetForCurrentView("SettingsRoot");
        public static ResourceLoader BackgroundSettings => ResourceLoader.GetForCurrentView("BackgroundSettings");
        public static ResourceLoader LyricsPreviewSettings => ResourceLoader.GetForCurrentView("LyricsPreviewSettings");
    }
}