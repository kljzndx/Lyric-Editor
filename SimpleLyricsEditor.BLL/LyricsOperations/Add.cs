using System;
using System.Collections.Generic;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.IBLL;

namespace SimpleLyricsEditor.BLL.LyricsOperations
{
    public class Add : ILyricsOperation
    {
        /// <param name="contents"></param>
        /// <param name="insertIndex">为 -1 则添加到最后</param>
        /// <param name="targetList"></param>
        /// <param name="time"></param>
        public Add(TimeSpan time, string[] contents, int insertIndex, IList<Lyric> targetList)
        {
            Items = new List<Lyric>();
            InsertIndex = insertIndex;
            TargetList = targetList;

            foreach (var str in contents)
                Items.Add(new Lyric(time, str.Trim()));
        }

        public IList<Lyric> Items { get; set; }
        public int InsertIndex { get; set; }
        public IList<Lyric> TargetList { get; set; }

        public void Do()
        {
            if (InsertIndex.Equals(-1))
                foreach (var lyric in Items)
                    TargetList.Add(lyric);

            foreach (var lyric in Items)
                TargetList.Insert(InsertIndex, lyric);
        }

        public void Undo()
        {
            foreach (var lyric in Items)
                TargetList.Remove(lyric);
        }
    }
}