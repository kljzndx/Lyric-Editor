using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace SimpleLyricEditor.Models
{
    public static class CharacterLibrary
    {
        public static ResourceLoader App => ResourceLoader.GetForCurrentView("App");
        public static ResourceLoader ErrorDialog => ResourceLoader.GetForCurrentView("ErrorDialog");
        public static ResourceLoader GetReviewsDialog => ResourceLoader.GetForCurrentView("GetReviewsDialog");
        public static ResourceLoader LyricsListClearDialog => ResourceLoader.GetForCurrentView("LyricsListClearDialog");
        public static ResourceLoader DragOrDrop => ResourceLoader.GetForCurrentView("DragOrDrop");
    }
}
