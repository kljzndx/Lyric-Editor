using System;

namespace SimpleLyricsEditor.ViewModels
{
    public class PageModel
    {
        public PageModel(string title, Type pageType)
        {
            Title = title;
            PageType = pageType;
        }

        public string Title { get; set; }
        public Type PageType { get; set; }
    }
}