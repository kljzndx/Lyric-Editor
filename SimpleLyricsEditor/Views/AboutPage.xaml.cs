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
using HappyStudio.UwpToolsLibrary.Information;
using SimpleLyricsEditor.Core;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleLyricsEditor.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AboutPage : Page
    {
        private string version = AppInfo.Version;

        public AboutPage()
        {
            this.InitializeComponent();

            JoinQqGroup_HyperlinkButton.AddHandler(PointerEnteredEvent, new PointerEventHandler(JoinQqGroup_HyperlinkButton_PointerEntered), true);
            JoinQqGroup_HyperlinkButton.AddHandler(PointerExitedEvent, new PointerEventHandler(JoinQqGroup_HyperlinkButton_PointerCanceled), true);
            JoinQqGroup_HyperlinkButton.AddHandler(PointerReleasedEvent, new PointerEventHandler(JoinQqGroup_HyperlinkButton_PointerCanceled), true);
            JoinQqGroup_HyperlinkButton.AddHandler(PointerCanceledEvent, new PointerEventHandler(JoinQqGroup_HyperlinkButton_PointerCanceled), true);
            JoinQqGroup_HyperlinkButton.AddHandler(PointerCaptureLostEvent, new PointerEventHandler(JoinQqGroup_HyperlinkButton_PointerCanceled), true);
        }

        private void JoinQqGroup_HyperlinkButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            JoinQqGroup_TextBlock.Text = CharacterLibrary.About.GetString("JoinQqGroup");
        }

        private void JoinQqGroup_HyperlinkButton_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            JoinQqGroup_TextBlock.Text = "184437847";
        }
    }
}
