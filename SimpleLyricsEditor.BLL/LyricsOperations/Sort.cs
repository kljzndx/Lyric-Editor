using System;
using System.Collections.Generic;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.IBLL;

namespace SimpleLyricsEditor.BLL.LyricsOperations
{
    public class Sort
    {
        public Sort(IList<Lyric> items)
        {
            Items = items;
        }

        public IList<Lyric> Items { get; set; }

        public void Invoke()
        {
            for (var i = Items.Count; i > 0; i--)
                for (var j = 0; j < i - 1; j++)
                    if (Items[j].CompareTo(Items[j + 1]) > 0)
                    {
                        Items.Insert(j + 1, Items[j]);
                        Items.RemoveAt(j);
                    }
        }
    }
}