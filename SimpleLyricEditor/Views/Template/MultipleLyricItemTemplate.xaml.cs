using SimpleLyricEditor.Models;
using SimpleLyricEditor.ViewModels;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricEditor.Views.Template
{
    public sealed partial class MultipleLyricItemTemplate : UserControl
    {
        private LyricItem lyric { get => this.DataContext as LyricItem; }
        private Settings settings = Settings.GetSettingsObject();

        public MultipleLyricItemTemplate()
        {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) =>
            {
                this.Bindings.Update();

                if (lyric is LyricItem)
                    lyric.PropertyChanged += (se, ev) =>
                    {
                        if (ev.PropertyName == "IsSelected")
                        {
                            if (lyric is LyricItem && lyric.IsSelected)
                            {
                                Main_TextBlock.FontWeight = FontWeights.Bold;
                                Main_TextBlock.FontSize += 2;
                            }
                            else
                            {
                                Main_TextBlock.FontWeight = FontWeights.Normal;
                                Main_TextBlock.FontSize = settings.ScrollLyricsPreviewFontSize;
                            }
                        }
                    };
            };
        }
    }
}
