using System.Text.RegularExpressions;
using GalaSoft.MvvmLight;

namespace SimpleLyricsEditor.DAL
{
    public class LyricsTag : ObservableObject
    {
        private readonly Regex _regex;
        private string _tagValue;

        public LyricsTag(string tagName)
        {
            TagName = tagName;
            _regex = new Regex($@"\[{tagName}:(.*)\]");
        }
        
        public string TagName { get; }

        public string TagValue
        {
            get => _tagValue;
            set => Set(ref _tagValue, value);
        }

        public bool GeiTag(string input)
        {
            var match = _regex.Match(input);

            if (match.Success)
            {
                TagValue = match.Groups[1].Value.Trim();
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return $"[{TagName}:{TagValue.Trim()}]";
        }
    }
}