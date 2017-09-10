using System.Collections.Generic;
using System.Linq;
using SimpleLyricsEditor.DAL;

namespace SimpleLyricsEditor.BLL.LyricsOperations
{
    public abstract class LyricsChangeOperationBase : LyricsOperationBase
    {
        protected readonly Dictionary<int, Lyric> Positions;

        protected LyricsChangeOperationBase(IEnumerable<Lyric> items, IList<Lyric> targetList) : base(targetList)
        {
            Items = items.ToList();
            Positions = new Dictionary<int, Lyric>();

            for (var i = 0; i < TargetList.Count; i++)
                foreach (var lyric in Items.Where(l => !Positions.Values.Contains(l)))
                    if (TargetList[i].Equals(lyric))
                    {
                        Positions.Add(i, lyric);
                        break;
                    }
        }

        public List<Lyric> Items { get; }

        public abstract override void Do();

        public abstract override void Undo();
    }
}