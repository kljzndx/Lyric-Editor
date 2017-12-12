using Windows.ApplicationModel.Resources;

namespace SimpleLyricsEditor.Core
{
    public static class CharacterLibrary
    {
        public static ResourceLoader About => ResourceLoader.GetForCurrentView("About");
        public static ResourceLoader MessageBox => ResourceLoader.GetForCurrentView("MessageBox");
        public static ResourceLoader ErrorTable => ResourceLoader.GetForCurrentView("ErrorTable");
        public static ResourceLoader ErrorInfo => ResourceLoader.GetForCurrentView("ErrorInfo");
        public static ResourceLoader Email => ResourceLoader.GetForCurrentView("Email");
    }
}