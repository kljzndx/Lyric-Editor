using System;
using System.Collections.Generic;
using System.Linq;
using SimpleLyricsEditor.DAL;

namespace SimpleLyricsEditor.BLL.LyricsOperations
{
    public class Move : LyricsChangeOperationBase
    {
        private readonly TimeSpan _interpolation;
        private readonly bool _isBig;

        public Move(TimeSpan targetTime, IEnumerable<Lyric> items, IList<Lyric> targetList) : base(items, targetList)
        {
            TargetTime = targetTime;

            var oldTime = Items.First().Time;
            _isBig = oldTime > TargetTime;
            _interpolation = _isBig ? oldTime - TargetTime : TargetTime - oldTime;
        }
        
        public TimeSpan TargetTime { get; set; }

        public override void Do()
        {
            foreach (var item in Items)
                item.Time = _isBig ? item.Time - _interpolation : item.Time + _interpolation;
        }

        public override void Undo()
        {
            foreach (var item in Items)
                item.Time = !_isBig ? item.Time - _interpolation : item.Time + _interpolation;
        }
        
    }
}