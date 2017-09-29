using System;

namespace SimpleLyricsEditor.Models
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