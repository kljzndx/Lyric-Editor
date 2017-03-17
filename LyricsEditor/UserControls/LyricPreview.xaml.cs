using LyricsEditor.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LyricsEditor.UserControls
{
    public sealed partial class LyricPreview : UserControl
    {
        private Setting settings = Setting.GetSettingObject();
        private string lyricContent;


        public string LyricContent
        {
            get { return (string)GetValue(LyricContentProperty); }
            set { SetValue(LyricContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LyricContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LyricContentProperty =
            DependencyProperty.Register(nameof(LyricContent), typeof(string), typeof(LyricPreview), new PropertyMetadata(String.Empty));


        public LyricPreview()
        {
            this.InitializeComponent();
        }

        public void SwitchLyric(string content)
        {
            lyricContent = content;
            FadeOut_Storyboard.Begin();
        }

        private void FadeOut_Storyboard_Completed(object sender, object e)
        {
            LyricContent = lyricContent;
            FadeIn_Storyboard.Begin();
        }
    }
}
