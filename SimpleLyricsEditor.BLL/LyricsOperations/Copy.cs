using System;
using System.Collections.Generic;
using System.Linq;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.IBLL;

namespace SimpleLyricsEditor.BLL.LyricsOperations
{
    public class Copy : LyricsOperationBase
    {
        private readonly TimeSpan _interpolation;
        private readonly bool _isBig;

        public Copy(IList<Lyric> items, TimeSpan targetTime, IList<Lyric> targetList)
        {
            Items = items;
            TargetTime = targetTime;
            TargetList = targetList;

            var oldTime = items.First().Time;
            _isBig = oldTime > targetTime;
            _interpolation = _isBig ? oldTime - targetTime : targetTime - oldTime;
        }

        public IList<Lyric> Items { get; set; }
        public IList<Lyric> TargetList { get; set; }
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
