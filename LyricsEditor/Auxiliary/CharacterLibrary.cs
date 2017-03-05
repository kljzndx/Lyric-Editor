using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace LyricsEditor.Auxiliary
{
    public static class CharacterLibrary
    {
        public static ResourceLoader Main { get; } = ResourceLoader.GetForCurrentView("Main");
        public static ResourceLoader Setting { get; } = ResourceLoader.GetForCurrentView("Setting");
        public static ResourceLoader MessageBox { get; } = ResourceLoader.GetForCurrentView("MessageBox");
        public static ResourceLoader ShortcutKeys { get; } = ResourceLoader.GetForCurrentView("ShortcutKeys");
    }
}
