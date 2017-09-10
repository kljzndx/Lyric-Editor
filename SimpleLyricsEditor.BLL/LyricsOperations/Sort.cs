using System;
using System.Collections.Generic;
using System.Linq;
using SimpleLyricsEditor.DAL;

namespace SimpleLyricsEditor.BLL.LyricsOperations
{
    public class Sort : LyricsOperationBase
    {
        private readonly IEnumerable<Lyric> _oldList;

        public Sort(IList<Lyric> targetList) : base(targetList)
        {
            _oldList = targetList.ToList();
        }

        public override void Do()
        {
            for (var i = TargetList.Count; i > 0; i--)
                for (var j = 0; j < i - 1; j++)
                    if (TargetList[j].CompareTo(TargetList[j + 1]) > 0)
                    {
                        TargetList.Insert(j + 2, TargetList[j]);
                        TargetList.RemoveAt(j);
                    }
        }

        public override void Undo()
        {
            TargetList.Clear();
            foreach (Lyric lyric in _oldList)
                TargetList.Add(lyric);
        }
    }
}