using System.Collections.Generic;
using System.Linq;
using SimpleLyricsEditor.DAL;

namespace SimpleLyricsEditor.BLL.LyricsOperations
{
    public abstract class LyricsOperationBase
    {
        protected LyricsOperationBase(IList<Lyric> targetList)
        {
            TargetList = targetList;
        }

        public IList<Lyric> TargetList { get; }
        public string Message { get; set; }

        public abstract void Do();
        public abstract void Undo();
    }
}