using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleLyricEditor.Extensions;

namespace SimpleLyricEditor.Models
{
    public class Lyric : ObservableObject, IComparable
    {
        public static Lyric Empty { get; } = new Lyric();

        private TimeSpan time;
        public TimeSpan Time { get => time; set => Set(ref time, value); }

        private string content;
        public string Content
        {
            get => content;
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    Set(ref content, String.Empty);
                else
                    Set(ref content, value);
            }
        }

        public Lyric()
        {
            time = TimeSpan.Zero;
            content = String.Empty;
        }

        public override string ToString()
        {
            return $"[{time.ToLyricTimeString()}]{content}";
        }

        public int CompareTo(object obj)
        {
            return ((IComparable)Time).CompareTo(((Lyric)obj).Time);
        }
    }
}
