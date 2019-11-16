using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SimpleLyricsEditor.Core;
using SimpleLyricsEditor.DAL;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricsEditor.Control.Templates
{
    public sealed partial class LyricsMultilinePreviewItemTemplate : UserControl
    {
        public static readonly DependencyProperty LyricContentProperty = DependencyProperty.Register(
            nameof(LyricContent), typeof(string), typeof(LyricsMultilinePreviewItemTemplate), new PropertyMetadata(String.Empty));

        public static readonly DependencyProperty IsBoldProperty = DependencyProperty.Register(
            nameof(IsBold), typeof(bool), typeof(LyricsMultilinePreviewItemTemplate), new PropertyMetadata(false, IsBold_OnPropertyChangedCallback));

        private readonly Settings _settings = Settings.Current;

        public LyricsMultilinePreviewItemTemplate()
        {
            this.InitializeComponent();
            Main_TextBlock.FontSize = _settings.MultilinePreviewFontSize;
            Amplifier_Animation.To = _settings.MultilinePreviewFontSize + 3;
            Reset_Animation.To = _settings.MultilinePreviewFontSize;
            _settings.PropertyChanged += Settings_PropertyChanged;
        }

        public string LyricContent
        {
            get => (string) GetValue(LyricContentProperty);
            set => SetValue(LyricContentProperty, value);
        }

        public bool IsBold
        {
            get => (bool) GetValue(IsBoldProperty);
            set => SetValue(IsBoldProperty, value);
        }

        private static void IsBold_OnPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var theObj = d as LyricsMultilinePreviewItemTemplate;
            if (theObj is null)
                return;

            if ((bool) e.NewValue)
            {
                theObj.Main_TextBlock.FontWeight = FontWeights.Bold;
                theObj.Reset_Storyboard.Stop();
                theObj.Amplifier_Storyboard.Begin();
            }
            else
            {
                theObj.Main_TextBlock.FontWeight = FontWeights.Normal;
                theObj.Amplifier_Storyboard.Stop();
                theObj.Reset_Storyboard.Begin();
            }
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_settings.MultilinePreviewFontSize))
            {
                Amplifier_Animation.To = _settings.MultilinePreviewFontSize + 3;
                Reset_Animation.To = _settings.MultilinePreviewFontSize;

                Main_TextBlock.FontSize =
                    IsBold ? (double) Amplifier_Animation.To : (double) Reset_Animation.To;
            }
        }
    }
}
