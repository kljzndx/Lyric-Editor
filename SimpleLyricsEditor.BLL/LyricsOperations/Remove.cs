using System.Collections.Generic;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.IBLL;

namespace SimpleLyricsEditor.BLL.LyricsOperations
{
    public class Remove : ILyricsOperation
    {
        public Remove(IList<Lyric> items, int seletionIndex, IList<Lyric> targetList)
        {
            Items = items;
            SeletionIndex = seletionIndex;
            TargetList = targetList;
        }

        public IList<Lyric> Items { get; set; }
        public int SeletionIndex { get; set; }
        public IList<Lyric> TargetList { get; set; }

        public void Do()
        {
            foreach (var lyric in Items)
                TargetList.Remove(lyric);
        }

        public void Undo()
        {
            foreach (var lyric in Items)
                TargetList.Insert(SeletionIndex, lyric);
        }
    }
}