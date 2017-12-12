using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;

namespace SimpleLyricsEditor.Models
{
    public class PageModel
    {
        public PageModel(Type pageType)
        {
            Title = String.Empty;
            PageType = pageType;

            Regex regex = new Regex(@"(?<pageName>.*)(Page)$");
            Match match = regex.Match(PageType.Name);

            if (match.Success)
            {
                string pageName = match.Groups["pageName"].Value;

                try
                {
                    string pageTitle = ResourceLoader.GetForCurrentView(pageName).GetString("Title");

                    if (!String.IsNullOrWhiteSpace(pageTitle))
                        Title = pageTitle;
                    else
                        throw new Exception($"Title of \" {pageName} \" Page Not found");
                }
                catch (COMException)
                {
                    throw new Exception($"Resource Map of \" {pageName} \" Page Not found");
                }
            }
        }

        public string Title { get; }
        public Type PageType { get; }
        
    }
}