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
using LyricsEditor.Information;
using LyricsEditor.Auxiliary;
using Windows.Storage;
using LyricsEditor.Tools;
using LyricsEditor.EventArg;
using Windows.System.Threading;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace LyricsEditor
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Music music = new Music();
        private int theLyricID;
        private Lyric theLyric;
        private ObservableCollection<Lyric> lyrics = new ObservableCollection<Lyric>();
        private Setting settings = Setting.GetSettingObject();
        private bool InputBoxAvailableFocus = false;
        private ThreadPoolTimer lyricPreview_ThreadPoolTimer;

        public MainPage()
        {
            this.InitializeComponent();
            var theWindow = CoreWindow.GetForCurrentThread();

            theWindow.KeyDown += MainPage_KeyDown;
            theWindow.KeyUp += TheWindow_KeyUp;
            
            AppVersionValue_TextBlock.Text = AppInfo.AppVersion;

            LyricFileTools.LyricFileChanageEvent +=
                (s, e) =>
                {
                    LyricTools.LrcAnalysis(e.Content, lyrics, settings.IdTag);
                    RefreshTheLyric();
                };

            music.MusicChanageEvent += BackgroundImage.RefreshAlbumImage;
            music.MusicChanageEvent += AudioPlayer.SwitchMusic;

            LyricPreview.Tapped += (s, e) => Lyric_ListView.SelectedIndex = -1;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (SystemInfo.DeviceType == "Windows.Mobile")
                Grid.SetRow(LyricEditButton_StackPanel, 1);
            else
                Grid.SetColumn(LyricEditButton_StackPanel, 1);

            if (AppInfo.BootCount++ == 1)
                ShortcutKeysPanel.PopUp();
            else if (AppInfo.BootCount >= 10 && !AppInfo.IsReviewsed)
                GetReviewsPanel.StartDisplay();
        }

        private async void MainPage_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            var selectItem = Lyric_ListView.SelectedItem as Lyric;

            //添加歌词
            if (args.VirtualKey == VirtualKey.Space && !InputBoxAvailableFocus && selectItem is null)
                AddLyric();

            //修改歌词
            if (args.VirtualKey == VirtualKey.Space && !InputBoxAvailableFocus && selectItem is Lyric)
            {
                ChangeLyricTime();
                if (Lyric_ListView.SelectedItems.Count == 1)
                    if (Lyric_ListView.SelectedIndex < lyrics.Count - 1)
                        Lyric_ListView.SelectedIndex++;
                    else
                        Lyric_ListView.SelectedIndex = -1;
            }

            //删除歌词
            if (args.VirtualKey == VirtualKey.Delete && !InputBoxAvailableFocus)
                DeleteLyric();
            //歌词列表排序
            if (args.VirtualKey == VirtualKey.S && !InputBoxAvailableFocus)
                LyricSort();

            //播放控制
            if (args.VirtualKey == VirtualKey.P && !InputBoxAvailableFocus && music.File != null)
            {
                if (AudioPlayer.IsPlay)
                    AudioPlayer.Pause();
                else
                    AudioPlayer.Play();
            }

            if (args.VirtualKey == VirtualKey.Shift)
            {
                DelLyric_Button.Content = "\uE107";
            }

            if (App.isPressCtrl)
            {
                if (App.isPressShift && args.VirtualKey == VirtualKey.S)
                {
                    await LyricFileTools.SaveLyricAsync(null, lyrics, settings.IdTag);
                    return;
                }

                switch (args.VirtualKey)
                {
                    case VirtualKey.M:
                        await music.OpenFile();
                        break;
                    case VirtualKey.L:
                        await LyricFileTools.OpenFileAsync();
                        break;
                    case VirtualKey.S:
                        await LyricFileTools.SaveLyricAsync(LyricFileTools.ThisLRCFile, lyrics, settings.IdTag);
                        break;
                }
            }
        }

        private void TheWindow_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Shift)
            {
                DelLyric_Button.Content = "\uE108";
            }
        }


        #region 自己定义的方法

        private void PreviewLyric()
        {
            if (!lyrics.Any())
                return;

            if (theLyric is null)
                theLyric = lyrics[theLyricID];

            var theTime = AudioPlayer.PlayPosition;

            if (theTime.Minutes == theLyric.Time.Minutes && theTime.Seconds == theLyric.Time.Seconds && theTime.Milliseconds / 10 >= theLyric.Time.Milliseconds / 10 - 10)
            {
                LyricPreview.SwitchLyric(theLyric.Content);
                if (theLyricID < lyrics.Count - 1)
                    theLyric = lyrics[++theLyricID];
                else
                {
                    theLyricID = 0;
                    theLyric = lyrics[0];
                }
            }

        }

        private async void PreviewLyric(ThreadPoolTimer timer)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, PreviewLyric);
        }

        private void StartDisplayLyricPreview()
        {
            lyricPreview_ThreadPoolTimer = ThreadPoolTimer.CreatePeriodicTimer(PreviewLyric, TimeSpan.FromMilliseconds(50));
        }

        private void RefreshTheLyric()
        {
            if (!lyrics.Any())
                return;

            bool isOk = false;
            TimeSpan Time = AudioPlayer.PlayPosition;

            if (Time < lyrics[0].Time)
            {
                theLyricID = 0;
                LyricPreview.SwitchLyric(String.Empty);
                isOk = true;
            }

            if (Time >= lyrics[lyrics.Count - 1].Time)
            {
                theLyricID = 0;
                if (AudioPlayer.IsPlay)
                    LyricPreview.SwitchLyric(lyrics[lyrics.Count - 1].Content);
                isOk = true;
            }

            if (!isOk)
            {
                for (int i = 0; i < lyrics.Count; i++)
                {
                    if (lyrics[i].Time.CompareTo(Time) == 0)
                    {
                        theLyricID = i < lyrics.Count - 1 ? i + 1 : i;
                        if (AudioPlayer.IsPlay)
                            LyricPreview.SwitchLyric(lyrics[i].Content);
                        break;
                    }
                    if (lyrics[i].Time.CompareTo(Time) == 1)
                    {
                        theLyricID = i;
                        if (AudioPlayer.IsPlay)
                            LyricPreview.SwitchLyric(lyrics[i - 1].Content);
                        break;
                    }
                }
            }

            theLyric = lyrics[theLyricID];
        }

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

            if (Lyric_ListView.SelectedItems.Count == 1)
            {
                int line = 0;
                for (int i = 0; i < lyrics.Count; i++)
                {
                    if (lyrics[i].Equals(Lyric_ListView.SelectedItems[0]))
                    {
                        line = i + 1;
                        break;
                    }
                }
                if (line < lyrics.Count - 1)
                    lyrics.Move(lyrics.Count - 1, line);
                (Lyric_ListView.ItemsPanelRoot.Children[line - 1] as ListViewItem).IsSelected = false;
                (Lyric_ListView.ItemsPanelRoot.Children[line] as ListViewItem).IsSelected = true;
            }
            RefreshTheLyric();
        }

        private void DeleteLyric()
        {
            bool b = lyrics.Any();

            if (App.isPressShift)
                lyrics.Clear();
            else
                foreach (Lyric item in Lyric_ListView.SelectedItems)
                    lyrics.Remove(item);

            //如果之前有歌词项而现在没有的话就新建个文件
            if (b && !lyrics.Any())
                LyricFileTools.ThisLRCFile = null;
            
            RefreshTheLyric();
        }

        private async void ChangeLyricTime()
        {
            if (Lyric_ListView.SelectedItems.Count == 1)
            {
                (Lyric_ListView.SelectedItem as Lyric).Time = AudioPlayer.PlayPosition;
                RefreshTheLyric();
            }
            else
                await MessageBox.ShowMessageBoxAsync(CharacterLibrary.MessageBox.GetString("ChangeLyricTimeError"), CharacterLibrary.MessageBox.GetString("Close"));

            RefreshTheLyric();
        }

        private async void ChangeLyricContent()
        {
            if (LyricContent_TextBox.Text.Trim().Contains('\n') || LyricContent_TextBox.Text.Trim().Contains('\r'))
            {
                await MessageBox.ShowMessageBoxAsync(CharacterLibrary.MessageBox.GetString("ChangeLyricContentError"), CharacterLibrary.MessageBox.GetString("Close"));
                return;
            }
            foreach (Lyric item in Lyric_ListView.SelectedItems)
                item.Content = LyricContent_TextBox.Text.Trim();

            RefreshTheLyric();
        }

        private void LyricSort()
        {
            if (lyrics.Count > 1)
                for (int i = lyrics.Count; i > 0; i--)
                    for (int j = 0; j < i - 1; j++)
                        if (lyrics[j].CompareTo(lyrics[j + 1]) > 0)
                            lyrics.Move(j, j + 1);
            RefreshTheLyric();
        }

        #endregion
        #region 播放器

        private void AudioPlayer_PlayPositionUserChange(object sender, PlayPositionUserChangeEventArgs e)
        {
            if (music.File is null)
                return;
            if (e.IsChanging)
            {
                lyricPreview_ThreadPoolTimer.Cancel();
                return;
            }

            RefreshTheLyric();
            if (AudioPlayer.IsPlay)
                StartDisplayLyricPreview();
        }


        private void AudioPlayer_Played(object sender, EventArgs e)
        {
            RefreshTheLyric();
            StartDisplayLyricPreview();
        }

        private void AudioPlayer_Paused(object sender, EventArgs e)
        {
            lyricPreview_ThreadPoolTimer.Cancel();
            LyricPreview.SwitchLyric(String.Empty);
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

        private void LyricContent_TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                e.Handled = true;

                if (App.isPressCtrl)
                {
                    AddLyric();
                }
                else if (Lyric_ListView.SelectedItem is Lyric)
                {
                    ChangeLyricContent();
                    Lyric_ListView.SelectedIndex = Lyric_ListView.SelectedIndex < Lyric_ListView.Items.Count - 1 ? Lyric_ListView.SelectedIndex + 1 : -1;
                }
                else
                {
                    var thisBox = (sender as TextBox);
                    string selectPositionLaft = thisBox.Text.Substring(0, thisBox.SelectionStart) + "\r\n";
                    string result = selectPositionLaft + thisBox.Text.Substring(thisBox.SelectionStart);
                    thisBox.Text = result;
                    thisBox.Select(selectPositionLaft.Length - 1, 0);
                }
            }
        }

        #endregion
        #region 歌词编辑按钮
        private void Unselect_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (ListViewItem item in Lyric_ListView.ItemsPanelRoot.Children)
            {
                item.IsSelected = !item.IsSelected;
            }
        }

        private void SelectBeforeItem_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            bool select = false;
            for (int i = Lyric_ListView.ItemsPanelRoot.Children.Count - 1; i >= 0; i--)
            {
                var item = Lyric_ListView.ItemsPanelRoot.Children[i] as ListViewItem;
                if (select)
                    item.IsSelected = true;
                else if (item.IsSelected)
                    select = true;
            }
        }

        private void SelectAfterItem_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            bool select = false;
            foreach (ListViewItem item in Lyric_ListView.ItemsPanelRoot.Children)
            {
                if (select)
                    item.IsSelected = select;
                else if (item.IsSelected)
                    select = true;
            }
        }

        private void SelectParagraph_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            int selectID = 0;
            bool select = false;
            for (int i = 0; i < Lyric_ListView.ItemsPanelRoot.Children.Count; i++)
            {
                var listItem = Lyric_ListView.ItemsPanelRoot.Children[i] as ListViewItem;
                var lyricItem = Lyric_ListView.Items[i] as Lyric;
                if (select && String.IsNullOrEmpty(lyricItem.Content))
                    break;

                if (select)
                    listItem.IsSelected = true;
                else if (listItem.IsSelected)
                {
                    select = true;
                    selectID = i;
                }
            }

            for (int i = selectID; i >= 0; i--)
            {
                var listItem = Lyric_ListView.ItemsPanelRoot.Children[i] as ListViewItem;
                var lyricItem = Lyric_ListView.Items[i] as Lyric;
                if (String.IsNullOrEmpty(lyricItem.Content))
                    break;

                listItem.IsSelected = true;
            }
        }

        private void MultilineEdit_Button_Click(object sender, RoutedEventArgs e)
        {
            Lyric_ListView.SelectionMode = ListViewSelectionMode.Multiple;
            SelectToolkit_Button.Visibility = Visibility.Visible;
            MultilineEdit_Button.Visibility = Visibility.Collapsed;
            Submit_Button.Visibility = Visibility.Visible;
            ChanageTime_MenuFlyoutItem.IsEnabled = false;
        }

        private void Submit_Button_Click(object sender, RoutedEventArgs e)
        {
            Lyric_ListView.SelectionMode = ListViewSelectionMode.Single;
            SelectToolkit_Button.Visibility = Visibility.Collapsed;
            Submit_Button.Visibility = Visibility.Collapsed;
            MultilineEdit_Button.Visibility = Visibility.Visible;
            ChanageTime_MenuFlyoutItem.IsEnabled = true;
        }

        private void AddLyric_Button_Click(object sender, RoutedEventArgs e)
        {
            AddLyric();
        }

        private void DelLyric_Button_Click(object sender, RoutedEventArgs e)
        {
            DeleteLyric();
        }

        private void ChanageTime_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            ChangeLyricTime();
        }

        private void ChanageLyric_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            ChangeLyricContent();
        }
        #endregion
        #region 歌词列表

        private void Lyric_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selects = (sender as ListView).SelectedItems;
            if (selects.Any())
            {
                string text = String.Empty;
                foreach (Lyric item in selects)
                    text += item.Content + "\r\n";
                LyricContent_TextBox.Text = text.Trim();
            }
            else
                LyricContent_TextBox.Text = String.Empty;
        }

        private void Sort_Button_Click(object sender, RoutedEventArgs e)
        {
            LyricSort();
        }
        
        #endregion
        #region 低栏
        private async void OpenMusic_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            await music.OpenFile();
        }

        private async void OpenLRC_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            await LyricFileTools.OpenFileAsync();
        }

        private async void Save_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await LyricFileTools.SaveLyricAsync(LyricFileTools.ThisLRCFile, lyrics, settings.IdTag);
        }

        private async void SaveAs_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await LyricFileTools.SaveLyricAsync(null, lyrics, settings.IdTag);
        }

        private void Settings_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Sidebar_SplitView.IsPaneOpen = !Sidebar_SplitView.IsPaneOpen;
            Sidebar_Frame.Navigate(typeof(Setting_Page));
        }

        private async void Feedback_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri($"mailto:kljzndx@outlook.com?subject=Simple Lyric Editor {AppInfo.AppVersion} Feedback"));
        }

        private void ShowShortcutKeysPanel_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ShortcutKeysPanel.PopUp();
        }

        private void ShowUpdateLogPanel_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateLogPanel.StartPopup();
        }

        #endregion

        private void Sidebar_SplitView_PaneClosed(SplitView sender, object args)
        {
            Sidebar_Frame.Navigate(typeof(Page));
        }

        private void HideMenu_Button_Click(object sender, RoutedEventArgs e)
        {
            FastMenu_Grid.Visibility = Visibility.Collapsed;
        }

        private async void Main_Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                bool isGetMusic = false;
                bool isGetLyric = false;
                var items = await e.DataView.GetStorageItemsAsync();

                foreach (var item in items)
                {
                    var file = item as StorageFile;
                    var folder = item as StorageFolder;

                    if (file != null)
                    {
                        string fileType = file.FileType;
                        if (!isGetMusic)
                        {
                            if (fileType == ".mp3" || fileType == ".flac" || fileType == ".aac" || fileType == ".wav")
                            {
                                await music.OpenFile(file);
                                isGetMusic = true;
                            }
                        }
                        if (!isGetLyric)
                        {
                            if (fileType == ".lrc" || fileType == ".txt")
                            {
                                LyricTools.LrcAnalysis(await LyricFileTools.ReadLyricFile(file), lyrics, settings.IdTag);
                                isGetLyric = true;
                            }
                        }
                        if (isGetMusic && isGetLyric)
                            break;
                    }
                    else
                    {
                        await MessageBox.ShowMessageBoxAsync(CharacterLibrary.MessageBox.GetString("UnsupportedFolder"), CharacterLibrary.MessageBox.GetString("Close"));
                    }
                }
            }
        }

        private void Main_Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Link;
            e.DragUIOverride.Caption = CharacterLibrary.Main.GetString("DragAndDrop");
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsContentVisible = true;
        }

        private void MenuFlyout_Opening(object sender, object e)
        {
            ElementTheme theme = settings.Theme == ElementTheme.Default ? (App.Current.RequestedTheme == ApplicationTheme.Light ? ElementTheme.Light : ElementTheme.Dark) : (settings.Theme == ElementTheme.Light ? ElementTheme.Light : ElementTheme.Dark);
            var theFlyout = sender as MenuFlyout;
            Style style = new Style { TargetType = typeof(MenuFlyoutPresenter) };
            style.Setters.Add(new Setter { Property = MenuFlyoutPresenter.RequestedThemeProperty, Value = theme });
            theFlyout.MenuFlyoutPresenterStyle = style;
        }

    }
}
