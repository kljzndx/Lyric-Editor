using System.Collections.Generic;
using System.Linq;
using SimpleLyricsEditor.DAL;

namespace SimpleLyricsEditor.BLL.LyricsOperations
{
    public class Modify : LyricsChangeOperationBase
    {
        private readonly List<string> _oldContents;

        public Modify(IEnumerable<Lyric> items, string newContent, IList<Lyric> targetList) : base(items, targetList)
        {
            _oldContents = Items.Select(l => l.Content).ToList();
            NewContent = newContent;
        }
        
        public string NewContent { get; }

        public override void Do()
        {
            foreach (var item in Positions)
                TargetList[item.Key].Content = NewContent;
        }

        public override void Undo()
        {
            int i = 0;
            foreach (var id in Positions.Keys)
                TargetList[id].Content = _oldContents[i];
        }
    }
}