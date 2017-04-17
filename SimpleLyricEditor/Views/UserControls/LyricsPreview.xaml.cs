using SimpleLyricEditor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricEditor.Views.UserControls
{
    public sealed partial class LyricsPreview : UserControl
    {
        private Settings settings = Settings.GetSettingsObject();
        
        private Lyric thisLyric = Lyric.Empty;

        public Lyric ThisLyric
        {
            get => thisLyric;
            set
            {
                if (thisLyric.Equals(value))
                    return;

                bool isNull = thisLyric.Equals(Lyric.Empty);

                thisLyric = value;

                if (isNull)
                {
                    FadeIn_Storybeard.Begin();
                    this.Bindings.Update();
                }
                else
                    FadeOut_Storybeard.Begin();
            }
        }

        private int nextLyricID;
        
        public IList<Lyric> Lyrics
        {
            get { return (IList<Lyric>)GetValue(LyricsProperty); }
            set { SetValue(LyricsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Lyrics.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LyricsProperty =
            DependencyProperty.Register("Lyrics", typeof(IList<Lyric>), typeof(LyricsPreview), new PropertyMetadata(new List<Lyric>()));



        public TimeSpan Position
        {
            get { return (TimeSpan)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value + TimeSpan.FromMilliseconds(100)); }
        }

        // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(TimeSpan), typeof(LyricsPreview), new PropertyMetadata(TimeSpan.Zero));



        public LyricsPreview()
        {
            this.InitializeComponent();
        }

        public void PreviewLyric()
        {
            if (!Lyrics.Any())
                return;
            
            var nextTime = Lyrics[nextLyricID].Time;

            if (Position.Minutes == nextTime.Minutes && Position.Seconds == nextTime.Seconds && Position.Milliseconds / 100 >= nextTime.Milliseconds / 100)
            {
                ThisLyric = Lyrics[nextLyricID];
                nextLyricID = nextLyricID < Lyrics.Count - 1 ? nextLyricID + 1 : 0;
            }
        }
        
        public void RepositionLyric()
        {
            if (!Lyrics.Any())
            {
                ThisLyric = Lyric.Empty;
                return;
            }
            
            if (Position <= Lyrics.First().Time)
            {
                ThisLyric = Lyric.Empty;
                try
                {
                    nextLyricID = Lyrics.Single() is Lyric ? 0 : 1;
                }
                catch (InvalidOperationException)
                {
                    nextLyricID = 1;
                }
                return;
            }

            if (Position >= Lyrics.Last().Time)
            {
                ThisLyric = Lyrics.Last();
                nextLyricID = 0;
                return;
            }

            for (int i = 0; i < Lyrics.Count; i++)
            {
                if (Lyrics[i].Time.CompareTo(Position) == 1)
                {
                    ThisLyric = Lyrics[i - 1];
                    nextLyricID = i;
                    break;
                }
            }
            
        }

        private void FadeOut_Storybeard_Completed(object sender, object e)
        {
            FadeIn_Storybeard.Begin();
            this.Bindings.Update();
        }
    }
}
