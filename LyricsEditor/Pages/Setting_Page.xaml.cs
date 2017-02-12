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
using LyricsEditor.Model;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace LyricsEditor.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Setting_Page : Page
    {
        private Setting settings = Setting.GetSettingObject();
        public Setting_Page()
        {
            this.InitializeComponent();
            AppTheme_ComboBox.SelectedIndex = settings.Theme == ApplicationTheme.Light ? 0 : 1;
        }

        private void AppTheme_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var theme = (sender as ComboBox).SelectedIndex == 0 ? ApplicationTheme.Light : ApplicationTheme.Dark;
            settings.Theme = theme;
            AppTheme_Massage_TextBlock.Visibility = Application.Current.RequestedTheme != settings.Theme ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
