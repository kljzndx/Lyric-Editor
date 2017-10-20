using System.Collections.Generic;
using System.Linq;
using SimpleLyricsEditor.DAL;

namespace SimpleLyricsEditor.BLL.LyricsOperations
{
    public class Remove : LyricsChangeOperationBase
    {
        public Remove(IEnumerable<Lyric> items, IList<Lyric> targetList) : base(items, targetList)
        {
        }
        
        public override void Do()
        {
            foreach (var item in Items)
                TargetList.Remove(item);
        }

        public override void Undo()
        {
            foreach (var item in Positions)
                TargetList.Insert(item.Key, item.Value);
        }
    }
}