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
    public sealed partial class ShortcutKeysDescribedPanel : UserControl
    {
        public ShortcutKeysDescribedPanel()
        {
            this.InitializeComponent();
            if (AppInfo.LanguageCode == "en-US")
                ShortcutKeysTable_ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

        public void PopUp()
        {
            Root_Grid.Visibility = Visibility.Visible;
            PopUp_Animation.Begin();
        }
        
        private void Disappear_Animation_Completed(object sender, object e)
        {
            Root_Grid.Visibility = Visibility.Collapsed;
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Disappear_Animation.Begin();
        }
        
    }
}
