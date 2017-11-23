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
        
        private void LyrricsSinglePreview_Refreshed(LyricsPreviewBase sender, LyricsPreviewRefreshEventArgs args)
        {
            bool isAny = !String.IsNullOrEmpty(BackLyric.Content);

            if (!String.IsNullOrEmpty(args.CurrentLyric.Content))
                TextBlock.Text = args.CurrentLyric.Content;

            if (!isAny && !String.IsNullOrEmpty(args.CurrentLyric.Content))
                FadeIn.Begin();
            if (isAny && String.IsNullOrEmpty(args.CurrentLyric.Content))
                FadeOut.Begin();
        }
    }
}