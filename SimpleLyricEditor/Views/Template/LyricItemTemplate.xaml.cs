using SimpleLyricEditor.ViewModels;
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

namespace SimpleLyricEditor.Views.Template
{
    public sealed partial class LyricItemTemplate : UserControl
    {
        public event EventHandler GotoThisTime_Click;

        private LyricItem lyricItem { get => DataContext as LyricItem; }

        
        public LyricItemTemplate()
        {
            this.InitializeComponent();
            DataContextChanged += (s, e) => this.Bindings.Update();
        }

        private void GotoThisTime()
        {
            GotoThisTime_Click?.Invoke(lyricItem, EventArgs.Empty);
        }
    }
}
