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
using SimpleLyricsEditor.Core;
using SimpleLyricsEditor.Models;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleLyricsEditor.Views.SettingsPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LyricsPreviewSettingsPage : SettingsPageBase
    {
        private static PageModel CaraOkEffectSettings_PageModel = new PageModel(typeof(CaraOkSettingsPage));

        public LyricsPreviewSettingsPage() : base()
        {
            this.InitializeComponent();
        }

        private void CaraOkEffectSettings_Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(CaraOkEffectSettings_PageModel.PageType, CaraOkEffectSettings_PageModel.Title);
        }
    }
}
