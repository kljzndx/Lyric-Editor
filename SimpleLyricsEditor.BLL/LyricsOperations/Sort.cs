using System.Collections.Generic;
using SimpleLyricsEditor.DAL;

namespace SimpleLyricsEditor.BLL.LyricsOperations
{
    public class Sort : LyricsChangeOperationBase
    {
        public Sort(IEnumerable<Lyric> items, IList<Lyric> targetList) : base(items, targetList)
        {
        }

        private void ItemSort(int itemPosition)
        {
            for (int i = itemPosition; i > 0; i--)
                if (TargetList[i].CompareTo(TargetList[i - 1]) > 0)
                {
                    TargetList.Insert(i + 1, TargetList[i - 1]);
                    TargetList.RemoveAt(i - 1);
                }
        }

        public override void Do()
        {
            foreach (int position in Positions.Keys)
                ItemSort(position);
        }

        public override void Undo()
        {
            foreach (var item in Positions)
            {
                TargetList.Remove(item.Value);
                TargetList.Insert(item.Key, item.Value);
            }
        }
    }
}