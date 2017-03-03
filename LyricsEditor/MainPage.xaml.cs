using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.System;
using LyricsEditor.Model;
using Windows.UI.Core;
using LyricsEditor.Pages;
using LyricsEditor.UserControls;
using System.Reflection;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.DataTransfer;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace LyricsEditor
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Music music = new Music();
        private ObservableCollection<Lyric> lyrics = new ObservableCollection<Lyric>();
        private Setting settings = Setting.GetSettingObject();
        private bool InputBoxAvailableFocus = false;
        

        public MainPage()
        {
            this.InitializeComponent();
            CoreWindow.GetForCurrentThread().KeyDown += MainPage_KeyDown;
            AppVersionValue_TextBlock.Text = AppInfo.AppVersion;
            LyricFileManager.LyricFileChanageEvent += async (e) => { await LyricManager.LrcAnalysis(e.File, lyrics, settings.IdTag); };
            
            music.MusicChanageEvent += BackgroundImage.RefreshImage;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (SystemInfo.DeviceType == "Windows.Mobile")
                Grid.SetRow(LyricEditButton_StackPanel, 1);
            else
                Grid.SetColumn(LyricEditButton_StackPanel, 1);
            
            if (AppInfo.BootCount++ == 1)
                ShortcutKeysPanel.PopUp();
        }

        private void MainPage_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            var selectItem = Lyric_ListView.SelectedItem as Lyric;
            //添加歌词

            if (App.isPressCtrl && args.VirtualKey == VirtualKey.Enter)
                AddLyric();

            if (args.VirtualKey == VirtualKey.Space &&
                !InputBoxAvailableFocus &&
                selectItem is null)
            {
                AddLyric();
            }
            //修改歌词
            if (args.VirtualKey == VirtualKey.Space &&
                !InputBoxAvailableFocus &&
                selectItem is Lyric)
            {
                selectItem.Time = AudioPlayer.PlayPosition;
                if (Lyric_ListView.SelectedIndex < lyrics.Count - 1)
                    Lyric_ListView.SelectedIndex++;
            }
            //删除歌词
            if (args.VirtualKey == VirtualKey.Delete &&
                !InputBoxAvailableFocus &&
                selectItem is Lyric)
            {
                foreach (Lyric item in Lyric_ListView.SelectedItems)
                {
                    lyrics.Remove(item);
                }
            }

            
        }
        

        #region 自己定义的方法

        private void AddLyric()
        {
            if (!LyricContent_TextBox.Text.Trim().Contains('\n') && !LyricContent_TextBox.Text.Trim().Contains('\r'))
                lyrics.Add(new Lyric { Time = AudioPlayer.PlayPosition, Content = LyricContent_TextBox.Text.Trim() });
            else
            {
                char lineBreak = LyricContent_TextBox.Text.Contains('\n') ? '\n' : '\r';
                string[] lines = LyricContent_TextBox.Text.Trim().Split(lineBreak);
                foreach (var line in lines)
                {
                    lyrics.Add(new Lyric { Time = AudioPlayer.PlayPosition, Content = line.Trim() });
                }
            }
        }
        #endregion

        #region 歌词输入框
        private void LyricContent_TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            InputBoxAvailableFocus = true;
        }

        private void LyricContent_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            InputBoxAvailableFocus = false;
        }
        #endregion
        #region 歌词编辑按钮
        private void Unselect_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SelectBeforeItem_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SelectAfterItem_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SelectParagraph_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {

        }
        private void AddLyric_Button_Click(object sender, RoutedEventArgs e)
        {
            AddLyric();
        }

        private void DelLyric_Button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = Lyric_ListView.SelectedItems.Count - 1; i >= 0; i--)
            {
                lyrics.Remove(Lyric_ListView.SelectedItems[i] as Lyric);
            }
        }

        private void ChanageTime_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (Lyric_ListView.SelectedItems.Count == 1)
                (Lyric_ListView.SelectedItem as Lyric).Time = AudioPlayer.PlayPosition;
            else
                foreach (Lyric item in Lyric_ListView.SelectedItems)
                {
                    item.Time = AudioPlayer.PlayPosition;
                }
        }

        private void ChanageLyric_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (Lyric_ListView.SelectedItems.Count == 1)
                (Lyric_ListView.SelectedItem as Lyric).Content = LyricContent_TextBox.Text;
            else
                foreach (Lyric item in Lyric_ListView.SelectedItems)
                {
                    item.Content = LyricContent_TextBox.Text;
                }
        }
        #endregion
        #region 歌词列表

        private void Lyric_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selects = (sender as ListView).SelectedItems;
            if (selects.Count >= 1)
            {
                string lyric = String.Empty;
                foreach (Lyric item in selects)
                {
                    lyric += item.Content + "\r\n";
                }
                LyricContent_TextBox.Text = lyric.Trim();
            }
            else
                LyricContent_TextBox.Text = String.Empty;
        }

        private void Reload_Button_Click(object sender, RoutedEventArgs e)
        {
            if (lyrics.Count > 1)
                for (int i = lyrics.Count; i > 0; i--)
                    for (int j = 0; j < i - 1; j++)
                        if (lyrics[j].CompareTo(lyrics[j + 1]) > 0)
                            lyrics.Move(j, j + 1);
        }

        private void ListEnd_Rectangle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Lyric_ListView.SelectedIndex = -1;
        }

        private void ListEnd_Rectangle_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height == 50)
            {
                LyricList_ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        }
        #endregion
        #region 低栏
        private async void OpenMusic_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (await music.OpenFile())
            {
                await AudioPlayer.PlayMusic();
            }
        }
        private async void OpenLRC_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            await LyricFileManager.OpenFileAsync();
        }
        private async void Save_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await LyricManager.SaveLyric(lyrics, settings.IdTag);
        }
        private void Settings_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Sidebar_SplitView.IsPaneOpen = !Sidebar_SplitView.IsPaneOpen;
            Sidebar_Frame.Navigate(typeof(Setting_Page));
        }
        private void MultilineEdit_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Lyric_ListView.SelectionMode = ListViewSelectionMode.Multiple;
            //SelectToolkit_Button.Visibility = Visibility.Visible;
            MultilineEdit_AppBarButton.Visibility = Visibility.Collapsed;
            Submit_AppBarButton.Visibility = Visibility.Visible;
            ChanageTime_MenuFlyoutItem.IsEnabled = false;
        }

        private void Submit_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Lyric_ListView.SelectionMode = ListViewSelectionMode.Single;
            //SelectToolkit_Button.Visibility = Visibility.Collapsed;
            Submit_AppBarButton.Visibility = Visibility.Collapsed;
            MultilineEdit_AppBarButton.Visibility = Visibility.Visible;
            ChanageTime_MenuFlyoutItem.IsEnabled = true;
        }

        private async void Feedback_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri($"mailto:kljzndx@outlook.com?subject=Simple Lyric Editor {AppInfo.AppVersion} Feedback"));
        }

        private void ShowShortcutKeysPanel_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ShortcutKeysPanel.PopUp();
        }

        #endregion

        private void Sidebar_SplitView_PaneClosed(SplitView sender, object args)
        {
            Sidebar_Frame.Navigate(typeof(Page));
        }

        private void HideMenu_Button_Click(object sender, RoutedEventArgs e)
        {
            FastMenu_StackPanel.Visibility = Visibility.Collapsed;
        }

        private void Main_Grid_Drop(object sender, DragEventArgs e)
        {
            
        }
    }
}
