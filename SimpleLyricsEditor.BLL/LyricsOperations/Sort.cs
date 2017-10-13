using System.Collections.Generic;
using System.Linq;
using SimpleLyricsEditor.DAL;

namespace SimpleLyricsEditor.BLL.LyricsOperations
{
    public class Sort : LyricsChangeOperationBase
    {
        private readonly List<Lyric> _changedList = new List<Lyric>();
        private bool _isInvoked;

        public Sort(IEnumerable<Lyric> items, IList<Lyric> targetList) : base(items, targetList)
        {
        }

        private void ItemSort(Lyric sourceLyric)
        {
            int id = TargetList.IndexOf(sourceLyric);
            bool isMove = false;

            for (int i = id; i > 0; i--)
            {
                Lyric beforeLyric = TargetList[i - 1];
                Lyric currentLyric = TargetList[i];

                if (currentLyric != sourceLyric)
                    break;

                if (currentLyric.CompareTo(beforeLyric) < 0)
                {
                    TargetList.Insert(i + 1, beforeLyric);
                    TargetList.Remove(beforeLyric);

                    isMove = true;
                }
            }

            if (!_isInvoked && isMove && _changedList.Contains(sourceLyric))
                _changedList.Add(sourceLyric);
        }

        public override void Do()
        {
            Items.ForEach(ItemSort);
            _isInvoked = true;
        }

        public override void Undo()
        {
            foreach (var item in Positions.Where(d => _changedList.Contains(d.Value)))
            {
                TargetList.Remove(item.Value);
                TargetList.Insert(item.Key, item.Value);
            }
        }
    }
}