using System;
using System.Collections.Generic;
using System.Linq;
using SimpleLyricsEditor.DAL;

namespace SimpleLyricsEditor.BLL.LyricsOperations
{
    public class Copy : LyricsChangeOperationBase
    {
        private readonly TimeSpan _interpolation;
        private readonly bool _isBig;

        public Copy(IEnumerable<Lyric> items, TimeSpan targetTime, IList<Lyric> targetList) : base(items, targetList)
        {
            TargetTime = targetTime;

            var oldTime = Items.First().Time;
            _isBig = oldTime > targetTime;
            _interpolation = _isBig ? oldTime - targetTime : targetTime - oldTime;
        }
        
        public TimeSpan TargetTime { get; set; }

        public override void Do()
        {
            foreach (var lyric in Items)
            {
                var time = _isBig ? lyric.Time - _interpolation : lyric.Time + _interpolation;
                TargetList.Add(new Lyric(time, lyric.Content));
            }
        }

        public override void Undo()
        {
            foreach (var lyric in Items)
                TargetList.Remove(lyric);
        }
    }
}
