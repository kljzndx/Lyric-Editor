using System.Collections.Generic;
using SimpleLyricsEditor.DAL;

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

            foreach (var lyric in items)
                _oldContents.Add(lyric.Content);
        }

        public IEnumerable<Lyric> Items { get; set; }
        public string NewContent { get; }

        public override void Do()
        {
            foreach (var lyric in Items)
                lyric.Content = NewContent;
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