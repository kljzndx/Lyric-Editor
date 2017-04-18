using SimpleLyricEditor.Models;
using SimpleLyricEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace SimpleLyricEditor
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Main_ViewModel model = App.Locator.Main;
        public MainPage()
        {
            this.InitializeComponent();

            model.SelectedItems = Lyrics_ListView.SelectedItems;

            audioPlayer.PositionChanged +=
                (s, e) =>
                {
                    if (e.IsUserChange)
                        lyricsPreview.RepositionLyric();
                    else
                        lyricsPreview.PreviewLyric();
                };

            model.LyricsListChanged += (s, e) => lyricsPreview.RepositionLyric();
            
            CoreWindow window = CoreWindow.GetForCurrentThread();
            window.KeyDown += Window_KeyDown;
            window.KeyUp += Window_KeyUp;
        }

        private void HideFastMenu()
        {
            FastMenu_Grid.Visibility = Visibility.Collapsed;
        }

        private void Window_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Shift)
                DelLyric_Button.Content = "\uE107";
            if (!App.IsInputBoxGotFocus)
            {
                switch (args.VirtualKey)
                {
                    case VirtualKey.Space:
                        this.Focus(FocusState.Pointer);
                        break;
                }
            }
        }

        private void Window_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Shift)
                DelLyric_Button.Content = "\uE10A";
        }

        private void Lyrics_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string content = String.Empty;

            foreach (Lyric item in (sender as ListView).SelectedItems)
                content += item.Content + "\r\n";

            model.LyricContent = content.Trim();
        }

        private void LyricsContent_TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && sender is TextBox t)
            {
                e.Handled = true;

                model.LyricContent = t.Text;

                if (App.IsPressCtrl)
                    model.AddLyric();
                else if (Lyrics_ListView.SelectedItems.Any())
                {
                    model.ChangeContent();
                    Lyrics_ListView.SelectedIndex = Lyrics_ListView.SelectedIndex < Lyrics_ListView.Items.Count - 1 ? Lyrics_ListView.SelectedIndex + 1 : -1;
                }
                else
                {
                    t.Text += "\n";
                    t.Select(t.Text.Length, 0);
                }
            }
        }

        private void Settings_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            HidePanel_SplitView.IsPaneOpen = !HidePanel_SplitView.IsPaneOpen;
        }
        
        private void MenuFlyout_Opened(object sender, object e)
        {
            var menu = sender as MenuFlyout;
            Style s = new Style();
            s.TargetType = typeof(MenuFlyoutPresenter);
            s.Setters.Add(new Setter { Property = MenuFlyoutPresenter.RequestedThemeProperty, Value = model.Settings.PageTheme });
            menu.MenuFlyoutPresenterStyle = s;
        }

        private void MultilineEditMode_Button_Click(object sender, RoutedEventArgs e)
        {
            Lyrics_ListView.SelectionMode = ListViewSelectionMode.Multiple;
            model.IsMultilineEditMode = true;
        }

        private void ExitMultilineEditMode_Button_Click(object sender, RoutedEventArgs e)
        {
            Lyrics_ListView.SelectionMode = ListViewSelectionMode.Single;
            model.IsMultilineEditMode = false;
        }

        private void Select_Reverse_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (ListViewItem item in Lyrics_ListView.ItemsPanelRoot.Children)
                item.IsSelected = !item.IsSelected;
        }

        private void Select_BeforeItem_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (Lyrics_ListView.SelectedItem is Lyric)
                foreach (ListViewItem item in Lyrics_ListView.ItemsPanelRoot.Children)
                {
                    if (item.IsSelected)
                        break;
                    item.IsSelected = true;
                }
        }

        private void Select_AfterItem_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (Lyrics_ListView.SelectedItem is Lyric)
                for (int i = Lyrics_ListView.Items.Count - 1; i > 0; i--)
                {
                    ListViewItem l = (ListViewItem)Lyrics_ListView.ItemsPanelRoot.Children[i];
                    if (l.IsSelected)
                        break;
                    l.IsSelected = true;
                }
        }

        private void Select_Paragraph_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            //首先确认当前选定项是否有内容
            if (Lyrics_ListView.SelectedItem is Lyric thisSelectedItem && !String.IsNullOrWhiteSpace(thisSelectedItem.Content))
            {
                //当前选定项ID
                int sID = Lyrics_ListView.SelectedIndex;

                //遍历并选定接下来的项目 直到碰到没有内容的项目
                for (int i = sID; i < Lyrics_ListView.Items.Count; i++)
                {
                    if (String.IsNullOrWhiteSpace((Lyrics_ListView.Items[i] as Lyric).Content))
                        break;
                    (Lyrics_ListView.ItemsPanelRoot.Children[i] as ListViewItem).IsSelected = true;
                }

                //跟上面的一样，不过这次是反向遍历
                for (int i = sID; i > 0; i--)
                {
                    if (String.IsNullOrWhiteSpace((Lyrics_ListView.Items[i] as Lyric).Content))
                        break;
                    (Lyrics_ListView.ItemsPanelRoot.Children[i] as ListViewItem).IsSelected = true;
                }
            }
        }

        private void LyricItemTemplate_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            model.GoToCurrentLyricTime();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            model.GoToCurrentLyricTime();
        }
    }
}
