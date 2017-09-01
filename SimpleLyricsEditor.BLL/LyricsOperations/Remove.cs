using System.Collections.Generic;
using System.Linq;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.IBLL;

namespace SimpleLyricsEditor.BLL.LyricsOperations
{
    public class Remove : LyricsOperationBase
    {
        private readonly Dictionary<int, Lyric> _positions;

        public Remove(IList<Lyric> items, IList<Lyric> targetList)
        {
            Items = items;
            TargetList = targetList;
            _positions = new Dictionary<int, Lyric>();

            for (int i = 0; i < targetList.Count; i++)
                foreach (Lyric lyric in items.SkipWhile(_positions.Values.Contains))
                    if (lyric.Equals(targetList[i]))
                        _positions.Add(i, lyric);
        }

        public IList<Lyric> Items { get; set; }
        public IList<Lyric> TargetList { get; set; }

        public override void Do()
        {
            foreach (Lyric lyric in Items)
                TargetList.Remove(lyric);
        }

        public override void Undo()
        {
            foreach (var item in _positions)
                TargetList.Insert(item.Key, item.Value);
        }
    }
}