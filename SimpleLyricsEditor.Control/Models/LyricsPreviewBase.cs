using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.Events;

namespace SimpleLyricsEditor.Control.Models
{
    public abstract class LyricsPreviewBase : UserControl
    {
        private static readonly ObservableCollection<Lyric> EmptyLyricList = new ObservableCollection<Lyric>();

        public static readonly DependencyProperty LyricsProperty = DependencyProperty.Register(
            nameof(Lyrics), typeof(IList<Lyric>), typeof(LyricsPreviewBase), new PropertyMetadata(EmptyLyricList));

        public static readonly DependencyProperty CurrentLyricProperty = DependencyProperty.Register(
            nameof(CurrentLyric), typeof(Lyric), typeof(LyricsPreviewBase), new PropertyMetadata(Lyric.Empty));
        
        protected Lyric BackLyric;
        protected int NextIndex;

        public IList<Lyric> Lyrics
        {
            get => (IList<Lyric>) GetValue(LyricsProperty);
            set => SetValue(LyricsProperty, value);
        }

        public Lyric CurrentLyric
        {
            get => (Lyric) GetValue(CurrentLyricProperty);
            set
            {
                BackLyric = GetValue(CurrentLyricProperty) as Lyric;
                SetValue(CurrentLyricProperty, value); 
                Refreshed?.Invoke(this, new LyricsPreviewRefreshEventArgs(value));
            }
        }

        public event TypedEventHandler<LyricsPreviewBase, LyricsPreviewRefreshEventArgs> Refreshed;

        protected bool CanPreview => IsEnabled && Visibility == Visibility.Visible && Lyrics != null && Lyrics.Any();
        
        public void RefreshLyric(TimeSpan position)
        {
            if (!CanPreview)
                return;

            if (NextIndex >= Lyrics.Count)
                NextIndex = 0;

            long currentPositionTicks = position.Ticks;
            long currentLyricTimeTicks = CurrentLyric != null ? CurrentLyric.Time.Ticks : 0;
            Lyric backLyric = Lyrics[NextIndex > 1 ? NextIndex - 2 : 0];
            Lyric nextLyric = Lyrics[NextIndex];
            long nextLyricTimeTicks = nextLyric.Time.Ticks;

            if (currentPositionTicks >= nextLyricTimeTicks)
            {
                NextIndex++;
                CurrentLyric = nextLyric;
            }
            else if (currentPositionTicks < currentLyricTimeTicks)
            {
                if (NextIndex > 0)
                    NextIndex--;

                CurrentLyric = backLyric;
            }
        }

        public void Reposition(TimeSpan position)
        {
            if (!CanPreview)
                return;
            
            if (position.CompareTo(Lyrics.First().Time) <= 0)
            {
                NextIndex = 0;
                CurrentLyric = Lyric.Empty;
                return;
            }

            if (position.CompareTo(Lyrics.Last().Time) >= 0)
            {
                NextIndex = 0;
                CurrentLyric = Lyrics.Last();
                return;
            }

            for (var i = 0; i < Lyrics.Count; i++)
            {
                if (position.CompareTo(Lyrics[i].Time) < 0)
                {
                    NextIndex = i;
                    CurrentLyric = Lyrics[i - 1];
                    break;
                }
            }
        }
    }
}