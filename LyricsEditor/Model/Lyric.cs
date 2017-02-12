using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LyricsEditor.Model
{
    public class Lyric : Auxiliary, IComparable
    {
        private TimeSpan time;
        private string content;
        public TimeSpan Time { get => time; set { SetProperty(ref time, value); } }
        public string Content { get => content; set { SetProperty(ref content, value); } }

        public int CompareTo(object obj)
        {
            if (this.Time.TotalMinutes == (obj as Lyric).Time.TotalMinutes)
                return 0;
            return Time.TotalMinutes > (obj as Lyric).Time.TotalMinutes ? 1 : -1;
        }
        public override string ToString()
        {
            return $"[{Time.Minutes.ToString("D2")}:{Time.Seconds.ToString("D2")}.{Time.Milliseconds.ToString("D3").Remove(2)}] {Content}";
        }
    }
}
