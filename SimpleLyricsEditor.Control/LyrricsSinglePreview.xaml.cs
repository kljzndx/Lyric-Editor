using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using SimpleLyricsEditor.Control.Models;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.Events;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricsEditor.Control
{
    public sealed partial class LyrricsSinglePreview : LyricsPreviewBase
    {
        public static readonly DependencyProperty CaraOkEffectEnabledProperty = DependencyProperty.Register(
            nameof(CaraOkEffectEnabled), typeof(bool), typeof(LyrricsSinglePreview), new PropertyMetadata(true));

        private DoubleAnimation _scaleAnimation;
        private Storyboard _caraOk;

        public LyrricsSinglePreview()
        {
            InitializeComponent();
            base.Refreshed += LyrricsSinglePreview_Refreshed;

            _scaleAnimation = new DoubleAnimation
            {
                From = 0,
                EnableDependentAnimation = true
            };
            _caraOk = new Storyboard
            {
                Children = {_scaleAnimation}
            };

            Storyboard.SetTarget(_scaleAnimation, Top_TextBlock);
            Storyboard.SetTargetProperty(_scaleAnimation, nameof(Top_TextBlock.Width));
        }

        public bool CaraOkEffectEnabled
        {
            get => (bool) GetValue(CaraOkEffectEnabledProperty);
            set => SetValue(CaraOkEffectEnabledProperty, value);
        }

        private Duration ComputeCaraOkDuration(Lyric currentLyric)
        {
            TimeSpan nextLyricTime = Lyrics[NextIndex].Time;
            return nextLyricTime.CompareTo(currentLyric.Time) >= 0
                ? new Duration(nextLyricTime - currentLyric.Time)
                : new Duration(currentLyric.Time - nextLyricTime);
        }
        
        private void LyrricsSinglePreview_Refreshed(LyricsPreviewBase sender, LyricsPreviewRefreshEventArgs args)
        {
            bool isAny = !String.IsNullOrEmpty(BackLyric.Content);

            if (!String.IsNullOrEmpty(args.CurrentLyric.Content))
            {
                Bottom_TextBlock.Text = args.CurrentLyric.Content;
                if (CaraOkEffectEnabled && NextIndex != 0 && Lyrics.IndexOf(args.CurrentLyric) < Lyrics.Count - 1)
                {
                    Top_TextBlock.Text = args.CurrentLyric.Content;
                    _scaleAnimation.Duration = ComputeCaraOkDuration(args.CurrentLyric);
                    _caraOk.Begin();
                }
            }

            if (!isAny && !String.IsNullOrEmpty(args.CurrentLyric.Content))
                FadeIn.Begin();
            if (isAny && String.IsNullOrEmpty(args.CurrentLyric.Content))
                FadeOut.Begin();
        }

        private void Bottom_TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _scaleAnimation.To = e.NewSize.Width;
        }
    }
}