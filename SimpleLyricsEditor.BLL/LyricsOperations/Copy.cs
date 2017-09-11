using System;
using System.Collections.Generic;
using System.Linq;
using SimpleLyricsEditor.DAL;

namespace SimpleLyricsEditor.BLL.LyricsOperations
{
    public class Copy : LyricsChangeOperationBase
    {
        private readonly List<Lyric> _addItems;
        private readonly TimeSpan _interpolation;
        private readonly bool _isBig;
        private bool _isDoed;

        public Copy(IEnumerable<Lyric> items, TimeSpan targetTime, IList<Lyric> targetList) : base(items, targetList)
        {
            _addItems = new List<Lyric>();
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
                var addItem = new Lyric(time, lyric.Content);
                if (!_isDoed)
                    _addItems.Add(addItem);
                TargetList.Add(addItem);
            }
            _isDoed = true;
        }

        public override void Undo()
        {
            foreach (Lyric lyric in _addItems)
                TargetList.Remove(lyric);
        }
    }
}
