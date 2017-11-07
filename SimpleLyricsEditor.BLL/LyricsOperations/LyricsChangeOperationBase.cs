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
            List<Lyric> list = TargetList.OrderBy(l => l.Time).ToList();
            Items = items.ToList();
            Positions = new Dictionary<int, Lyric>();

            foreach (Lyric lyric in Items.Where(l => !Positions.ContainsValue(l)))
                Positions.Add(list.IndexOf(lyric), lyric);
        }

        public List<Lyric> Items { get; }

        public abstract override void Do();

        public abstract override void Undo();
    }
}