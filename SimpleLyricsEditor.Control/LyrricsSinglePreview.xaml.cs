using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.Events;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricsEditor.Control
{
    public sealed partial class LyrricsSinglePreview : UserControl
    {
        private const long TicksPerThreeSecond = TimeSpan.TicksPerSecond * 3;
        private static readonly Lyric Space = new Lyric(TimeSpan.Zero, String.Empty);

        public static readonly DependencyProperty LyricsProperty = DependencyProperty.Register(
            nameof(Lyrics), typeof(IList<Lyric>), typeof(LyrricsSinglePreview), new PropertyMetadata(null));

        public static readonly DependencyProperty CurrentLyricProperty = DependencyProperty.Register(
            nameof(CurrentLyric), typeof(Lyric), typeof(LyrricsSinglePreview), new PropertyMetadata(Space));

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
                bool isAny = GetValue(CurrentLyricProperty) is Lyric backLyric &&
                             !String.IsNullOrEmpty(backLyric.Content);

                SetValue(CurrentLyricProperty, value);

                if (!String.IsNullOrEmpty(value.Content))
                    TextBlock.Text = value.Content;

                if (!isAny && !String.IsNullOrEmpty(value.Content))
                    FadeIn.Begin();
                if (isAny && String.IsNullOrEmpty(value.Content))
                    FadeOut.Begin();

                Refreshed?.Invoke(this, new LyricsPreviewRefreshEventArgs(value.Content));
            }
        }

        public event TypedEventHandler<LyrricsSinglePreview, LyricsPreviewRefreshEventArgs> Refreshed;

        public void RefreshLyric(TimeSpan position)
        {
            if (!Lyrics.Any())
                return;
            if (_nextIndex >= Lyrics.Count)
                _nextIndex = 0;

            var currentTime = position.Ticks;
            var nextLyric = Lyrics[_nextIndex];
            var nextTime = nextLyric.Time.Ticks;

            if (currentTime >= nextTime && currentTime <= nextTime + TicksPerThreeSecond)
            {
                CurrentLyric = nextLyric;
                _nextIndex++;
            }
        }

        public void Reposition(TimeSpan position)
        {
            if (!Lyrics.Any())
                return;

            var currentTime = position;

            if (currentTime.CompareTo(Lyrics.First().Time) <= 0)
            {
                CurrentLyric = Space;
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
    }
}