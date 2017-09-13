using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SimpleLyricsEditor.DAL;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricsEditor.Control
{
    public sealed partial class LyrricsSinglePreview : UserControl
    {
        public static readonly DependencyProperty LyricsProperty = DependencyProperty.Register(
            nameof(Lyrics), typeof(IList<Lyric>), typeof(LyrricsSinglePreview), new PropertyMetadata(null));

        public static readonly DependencyProperty CurrentLyricProperty = DependencyProperty.Register(
            nameof(CurrentLyric), typeof(Lyric), typeof(LyrricsSinglePreview), new PropertyMetadata(null));

        public static readonly DependencyProperty BackgroundOpacityProperty = DependencyProperty.Register(
            nameof(BackgroundOpacity), typeof(double), typeof(LyrricsSinglePreview), new PropertyMetadata(1));

        private int _nextIndex;
        private readonly Lyric _space = new Lyric(TimeSpan.Zero, string.Empty);

        public LyrricsSinglePreview()
        {
            InitializeComponent();
        }

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
                SetValue(CurrentLyricProperty, value);
                FadeOut.Begin();
            }
        }

        public double BackgroundOpacity
        {
            get => (double) GetValue(BackgroundOpacityProperty);
            set => SetValue(BackgroundOpacityProperty, value);
        }

        public void RefreshLyric(TimeSpan position)
        {
            if (!Lyrics.Any())
                return;

            var nextLyric = Lyrics[_nextIndex];
            var nextTime = nextLyric.Time;
            
            if (position >= nextTime)
            {
                CurrentLyric = nextLyric;
                _nextIndex = _nextIndex < Lyrics.Count - 1 ? _nextIndex + 1 : 0;
            }
        }

        public void Reposition(TimeSpan position)
        {
            if (!Lyrics.Any())
                return;

            if (position.CompareTo(Lyrics.First().Time) <= 0)
            {
                CurrentLyric = _space;
                _nextIndex = 0;
                return;
            }

            if (position.CompareTo(Lyrics.Last().Time) >= 0)
            {
                CurrentLyric = Lyrics.Last();
                _nextIndex = 0;
                return;
            }

            for (var i = 0; i < Lyrics.Count; i++)
            {
                if (position.CompareTo(Lyrics[i].Time) < 0)
                {
                    CurrentLyric = Lyrics[i - 1];
                    _nextIndex = i;
                    break;
                }
            }
        }

        private void FadeOut_Completed(object sender, object e)
        {
            TextBlock.Text = CurrentLyric.Content;
            FadeIn.Begin();
        }
    }
}