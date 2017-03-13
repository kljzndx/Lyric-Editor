using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LyricsEditor.Auxiliary;

namespace LyricsEditor.Model
{
    public class Lyric : BindableBase, IComparable
    {
        private static int sID;
        private int id;
        private TimeSpan time;
        private string content;

        public int ID { get => id; set => SetProperty(ref id, value); }
        public TimeSpan Time { get => time; set { SetProperty(ref time, value); } }
        public string Content { get => content; set { SetProperty(ref content, value); } }

        public Lyric()
        {
            id = sID++;
        }
        public int CompareTo(object obj)
        {
            if (this.Time == (obj as Lyric).Time)
                return 0;
            return Time > (obj as Lyric).Time ? 1 : -1;
        }
        public override string ToString()
        {
            return $"[{Time.Minutes.ToString("D2")}:{Time.Seconds.ToString("D2")}.{Time.Milliseconds.ToString("D3").Remove(2)}] {Content}";
        }
    }
}
