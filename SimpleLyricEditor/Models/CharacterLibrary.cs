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
        public static ResourceLoader ErrorDialog { get => ResourceLoader.GetForCurrentView("ErrorDialog"); }
        public static ResourceLoader GetReviewsDialog { get => ResourceLoader.GetForCurrentView("GetReviewsDialog"); }
        public static ResourceLoader LyricsListClearDialog { get => ResourceLoader.GetForCurrentView("LyricsListClearDialog"); }
        public static ResourceLoader DragOrDrop { get => ResourceLoader.GetForCurrentView("DragOrDrop"); }
    }
}
