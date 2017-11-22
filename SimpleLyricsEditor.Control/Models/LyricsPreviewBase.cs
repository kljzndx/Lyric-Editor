using System;
using System.Collections.Generic;
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
        public static readonly DependencyProperty LyricsProperty = DependencyProperty.Register(
            nameof(Lyrics), typeof(IList<Lyric>), typeof(LyricsPreviewBase), new PropertyMetadata(Lyric.Empty));

        public static readonly DependencyProperty CurrentLyricProperty = DependencyProperty.Register(
            nameof(CurrentLyric), typeof(Lyric), typeof(LyricsPreviewBase), new PropertyMetadata(null));
        
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

        protected bool CanPreview => IsEnabled && Visibility == Visibility.Visible && Lyrics.Any();

        protected abstract void RefreshLyricCore(TimeSpan position);
        protected abstract void RepositionCore(TimeSpan position);

        public void RefreshLyric(TimeSpan position)
        {
            if (NextIndex >= Lyrics.Count)
                NextIndex = 0;

            if (CanPreview)
                RefreshLyricCore(position);
        }

        public void Reposition(TimeSpan position)
        {
            if (CanPreview)
                RepositionCore(position);
        }
    }
}