using Windows.ApplicationModel.Resources;

namespace SimpleLyricsEditor.Core
{
    public static class CharacterLibrary
    {
        static CharacterLibrary()
        {
            SettingsRoot = ResourceLoader.GetForCurrentView("SettingsRoot");
        }

        public static ResourceLoader SettingsRoot { get; }
    }
}