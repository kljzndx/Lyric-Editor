using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SimpleLyricsEditor.Control.Models;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.Events;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricsEditor.Control
{
    public sealed partial class LyrricsSinglePreview : LyricsPreviewBase
    {
        public LyrricsSinglePreview()
        {
            InitializeComponent();
            base.Refreshed += LyrricsSinglePreview_Refreshed;
        }

        protected override void RefreshLyricCore(TimeSpan position)
        {
            var currentTime = position.Ticks;
            var nextLyric = Lyrics[NextIndex];
            var nextTime = nextLyric.Time.Ticks;

            if (currentTime >= nextTime && currentTime <= nextTime + TimeSpan.TicksPerSecond)
            {
                CurrentLyric = nextLyric;
                NextIndex++;
            }
        }

        protected override void RepositionCore(TimeSpan position)
        {
            var currentTime = position;

            if (currentTime.CompareTo(Lyrics.First().Time) <= 0)
            {
                CurrentLyric = Lyric.Empty;
                NextIndex = 0;
                return;
            }

            if (currentTime.CompareTo(Lyrics.Last().Time) >= 0)
            {
                CurrentLyric = Lyrics.Last();
                NextIndex = 0;
                return;
            }

            for (var i = 0; i < Lyrics.Count; i++)
                if (currentTime.CompareTo(Lyrics[i].Time) < 0)
                {
                    CurrentLyric = Lyrics[i - 1];
                    NextIndex = i;
                    break;
                }
        }

        private void LyrricsSinglePreview_Refreshed(LyricsPreviewBase sender, LyricsPreviewRefreshEventArgs args)
        {
            bool isAny = BackLyric is Lyric && !String.IsNullOrEmpty(BackLyric.Content);

            if (!String.IsNullOrEmpty(args.CurrentLyric.Content))
                TextBlock.Text = args.CurrentLyric.Content;

            if (!isAny && !String.IsNullOrEmpty(args.CurrentLyric.Content))
                FadeIn.Begin();
            if (isAny && String.IsNullOrEmpty(args.CurrentLyric.Content))
                FadeOut.Begin();
        }
    }
}