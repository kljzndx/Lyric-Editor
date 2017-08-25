using System;
using System.Text.RegularExpressions;
using SimpleLyricsEditor.IDAL;

namespace SimpleLyricsEditor.DAL
{
    public class LyricsTag : ObservableObject
    {
        private readonly Regex _regex;
        private readonly string _tagNome;
        private string _tagValue;

        public LyricsTag(string tagNome)
        {
            _tagNome = tagNome;
            _regex = new Regex($@"\[{tagNome}:(.*)\]");
        }

        public string TagValue
        {
            get => _tagValue;
            set => Set(ref _tagValue, value);
        }

        public void GeiTag(string input)
        {
            Match match = _regex.Match(input);

            if (match.Success)
                TagValue = match.Groups[1].Value.Trim();
        }

        public override string ToString()
        {
            return $"[{_tagNome}:{TagValue.Trim()}]";
        }
    }
}