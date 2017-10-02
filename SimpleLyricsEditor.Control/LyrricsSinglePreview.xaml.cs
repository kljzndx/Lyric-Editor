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
        private const long TicksPerThreeSecond = TimeSpan.TicksPerSecond * 3;

        private static readonly TimeSpan AnimationDuration = TimeSpan.FromMilliseconds(200);
        
        public static readonly DependencyProperty LyricsProperty = DependencyProperty.Register(
            nameof(Lyrics), typeof(IList<Lyric>), typeof(LyrricsSinglePreview), new PropertyMetadata(null));

        public static readonly DependencyProperty CurrentLyricProperty = DependencyProperty.Register(
            nameof(CurrentLyric), typeof(Lyric), typeof(LyrricsSinglePreview), new PropertyMetadata(null));
        
        private readonly Lyric _space = new Lyric(TimeSpan.Zero, string.Empty);

        private int _nextIndex;

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
        
        public void RefreshLyric(TimeSpan position)
        {
            if (!Lyrics.Any())
                return;

            var currentTime = (position + AnimationDuration).Ticks;
            var nextLyric = Lyrics[_nextIndex];
            var nextTime = nextLyric.Time.Ticks;

            if (currentTime >= nextTime && currentTime <= nextTime + TicksPerThreeSecond)
            {
                CurrentLyric = nextLyric;
                _nextIndex = _nextIndex < Lyrics.Count - 1 ? _nextIndex + 1 : 0;
            }
        }

        public void Reposition(TimeSpan position)
        {
            if (!Lyrics.Any())
                return;

            var currentTime = position + AnimationDuration;

            if (currentTime.CompareTo(Lyrics.First().Time) <= 0)
            {
                CurrentLyric = _space;
                _nextIndex = 0;
                return;
            }

            if (currentTime.CompareTo(Lyrics.Last().Time) >= 0)
            {
                CurrentLyric = Lyrics.Last();
                _nextIndex = 0;
                return;
            }

            for (var i = 0; i < Lyrics.Count; i++)
                if (currentTime.CompareTo(Lyrics[i].Time) < 0)
                {
                    CurrentLyric = Lyrics[i - 1];
                    _nextIndex = i;
                    break;
                }
        }

        private void FadeOut_Completed(object sender, object e)
        {
            TextBlock.Text = CurrentLyric.Content;
            FadeIn.Begin();
        }
    }
}