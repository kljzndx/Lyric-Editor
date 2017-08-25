using System.Collections.Generic;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.IBLL;

namespace SimpleLyricsEditor.BLL.LyricsOperations
{
    public class Modify : ILyricsOperation
    {
        private readonly List<string> _oldContents;

        public Modify(IList<Lyric> items, string newContent)
        {
            Items = items;
            _oldContents = new List<string>();
            NewContent = newContent;
        }

        public IList<Lyric> Items { get; set; }
        public string NewContent { get; }

        public void Do()
        {
            _oldContents.Clear();
            foreach (var lyric in Items)
            {
                _oldContents.Add(lyric.Content);
                lyric.Content = NewContent;
            }
        }

        public void Undo()
        {
            for (var i = 0; i < Items.Count; i++)
                Items[i].Content = _oldContents[i];
        }
    }
}