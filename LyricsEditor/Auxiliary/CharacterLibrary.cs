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
        public static ResourceLoader MessageBox { get; } = ResourceLoader.GetForCurrentView("MessageBox");
    }
}
