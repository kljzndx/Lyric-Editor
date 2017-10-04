using Windows.ApplicationModel.Resources;

namespace SimpleLyricsEditor.Core
{
    public static class CharacterLibrary
    {
        public static ResourceLoader Error { get; } = ResourceLoader.GetForCurrentView("Error");
        public static ResourceLoader SettingsRoot { get; } = ResourceLoader.GetForCurrentView("SettingsRoot");
        public static ResourceLoader BackgroundSettings { get; } = ResourceLoader.GetForCurrentView("BackgroundSettings");
        public static ResourceLoader LyricsPreviewSettings { get; } = ResourceLoader.GetForCurrentView("LyricsPreviewSettings");
    }
}