using System;
using System.Collections.Generic;
using System.Linq;
using SimpleLyricsEditor.DAL;

namespace SimpleLyricsEditor.BLL.LyricsOperations
{
    public class Sort : LyricsOperationBase
    {
        private readonly IEnumerable<Lyric> _oldList;

        public Sort(IList<Lyric> items) : base(null)
        {
            Items = items;
            _oldList = items.ToList();
        }

        public IList<Lyric> Items { get; set; }

        public override void Do()
        {
            for (var i = Items.Count; i > 0; i--)
                for (var j = 0; j < i - 1; j++)
                    if (Items[j].CompareTo(Items[j + 1]) > 0)
                    {
                        Items.Insert(j + 2, Items[j]);
                        Items.RemoveAt(j);
                    }
        }

        public override void Undo()
        {
            Items.Clear();
            foreach (Lyric lyric in _oldList)
                Items.Add(lyric);
        }
    }
}