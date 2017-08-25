using System.Collections.Generic;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.IBLL;

namespace SimpleLyricsEditor.BLL.LyricsOperations
{
    public class Add : ILyricsOperation
    {
        /// <param name="items">要添加的项目</param>
        /// <param name="insertIndex">为 -1 则添加到最后</param>
        /// <param name="targetList"></param>
        public Add(IList<Lyric> items, int insertIndex, IList<Lyric> targetList)
        {
            Items = items;
            InsertIndex = insertIndex;
            TargetList = targetList;
        }

        public IList<Lyric> Items { get; set; }
        public int InsertIndex { get; set; }
        public IList<Lyric> TargetList { get; set; }

        public void Do()
        {
            if (InsertIndex.Equals(-1))
                foreach (var lyric in Items)
                    TargetList.Add(lyric);

            foreach(var lyric in Items)
                TargetList.Insert(InsertIndex, lyric);
        }

        public void Undo()
        {
            foreach (var lyric in Items)
                TargetList.Remove(lyric);
        }
    }
}