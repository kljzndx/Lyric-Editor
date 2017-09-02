using System.Collections.Generic;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.IBLL;

namespace SimpleLyricsEditor.BLL.LyricsOperations
{
    public class Modify : LyricsOperationBase
    {
        private readonly List<string> _oldContents;

        public Modify(IEnumerable<Lyric> items, string newContent)
        {
            Items = items;
            _oldContents = new List<string>();
            NewContent = newContent;
        }

        public IEnumerable<Lyric> Items { get; set; }
        public string NewContent { get; }

        public override void Do()
        {
            _oldContents.Clear();
            foreach (var lyric in Items)
            {
                _oldContents.Add(lyric.Content);
                lyric.Content = NewContent;
            }
        }

        public override void Undo()
        {
            var crt = Items.GetEnumerator();
            var old = _oldContents.GetEnumerator();

            if (crt is IEnumerator<Lyric> && old is IEnumerator<string>)
                while (crt.MoveNext() && old.MoveNext())
                    crt.Current.Content = old.Current;
        }
    }
}