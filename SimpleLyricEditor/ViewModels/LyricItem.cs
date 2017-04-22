using GalaSoft.MvvmLight;
using SimpleLyricEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLyricEditor.ViewModels
{
    public class LyricItem : Lyric
    {
        public static LyricItem Empty { get; } = new LyricItem();

        private bool isSelected;

        public bool IsSelected { get => isSelected; set => Set(ref isSelected, value); }

        public LyricItem() : base()
        {

        }
    }
}
