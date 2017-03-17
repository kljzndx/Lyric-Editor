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
            CoreWindow.GetForCurrentThread().KeyDown += MainPage_KeyDown;
            AppVersionValue_TextBlock.Text = AppInfo.AppVersion;

            LyricFileTools.LyricFileChanageEvent += 
                (s,e) => 
                {
                    LyricTools.LrcAnalysis(e.Content, lyrics, settings.IdTag);
                    RefreshTheLyric();
                };
            
            music.MusicChanageEvent += BackgroundImage.RefreshAlbumImage;
            music.MusicChanageEvent += AudioPlayer.SwitchMusic;
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
                else
                {
                    Lyric_ListView.SelectedIndex = -1;
                    AddLyric();
                }
                RefreshTheLyric();
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
                RefreshTheLyric();
            }

            
        }
        

        #region 自己定义的方法

        private void PreviewLyric()
        {
            if (lyrics.Count == 0)
                return;
            var theTime = AudioPlayer.PlayPosition;
            if (theLyric is null)
            {
                theLyric = lyrics[theLyricID];
            }

            if (theTime.Minutes == theLyric.Time.Minutes && theTime.Seconds == theLyric.Time.Seconds && theTime.Milliseconds / 100 >= theLyric.Time.Milliseconds - 100 / 100)
            {
                LyricPreview.SwitchLyric(theLyric.Content);
                if (theLyricID < lyrics.Count-1)
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
            lyricPreview_ThreadPoolTimer = ThreadPoolTimer.CreatePeriodicTimer(PreviewLyric, TimeSpan.FromMilliseconds(100));
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
                isOk = true;
            }

            if (Time > lyrics[lyrics.Count - 1].Time)
            {
                theLyricID = lyrics.Count - 1;
                isOk = true;
            }

            if (!isOk)
            {
                for (int i = 0; i < lyrics.Count; i++)
                {
                    if (lyrics[i].Time.CompareTo(Time) == 0)
                    {
                        theLyricID = i < lyrics.Count - 1 ? i + 1 : i;
                        LyricPreview.SwitchLyric(lyrics[i].Content);
                        break;
                    }
                    if (lyrics[i].Time.CompareTo(Time) == 1)
                    {
                        theLyricID = i;
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
            StartDisplayLyricPreview();
        }


        private void AudioPlayer_Played(object sender, EventArgs e)
        {
            StartDisplayLyricPreview();
        }

        private void AudioPlayer_Paused(object sender, EventArgs e)
        {
            lyricPreview_ThreadPoolTimer.Cancel();
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
                    RefreshTheLyric();
                }
                else if (Lyric_ListView.SelectedItem is Lyric)
                {
                    foreach (Lyric item in Lyric_ListView.SelectedItems)
                        item.Content = LyricContent_TextBox.Text;
                    Lyric_ListView.SelectedIndex = Lyric_ListView.SelectedIndex == Lyric_ListView.Items.Count - 1 ? Lyric_ListView.Items.Count - 1 : Lyric_ListView.SelectedIndex + 1;
                    RefreshTheLyric();
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

        private void AddLyric_Button_Click(object sender, RoutedEventArgs e)
        {
            AddLyric();
        }

        private void DelLyric_Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (Lyric item in Lyric_ListView.SelectedItems)
            {
                lyrics.Remove(item);
            }
            RefreshTheLyric();
        }

        private void ChanageTime_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (Lyric_ListView.SelectedItems.Count == 1)
                (Lyric_ListView.SelectedItem as Lyric).Time = AudioPlayer.PlayPosition;
            else
                foreach (Lyric item in Lyric_ListView.SelectedItems)
                    item.Time = AudioPlayer.PlayPosition;
                
            RefreshTheLyric();
        }

        private void ChanageLyric_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (Lyric_ListView.SelectedItems.Count == 1)
                (Lyric_ListView.SelectedItem as Lyric).Content = LyricContent_TextBox.Text;
            else
                foreach (Lyric item in Lyric_ListView.SelectedItems)
                    item.Content = LyricContent_TextBox.Text;

            RefreshTheLyric();
        }
        #endregion
        #region 歌词列表

        private void Lyric_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var select = (sender as ListView).SelectedItem as Lyric;
            if (select != null)
                LyricContent_TextBox.Text = select.Content;
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
            RefreshTheLyric();
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
            await music.OpenFile();
        }

        private async void OpenLRC_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            await LyricFileTools.OpenFileAsync();
        }

        private async void Save_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await LyricFileTools.SaveLyricAsync(lyrics, settings.IdTag);
        }

        private void Settings_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Sidebar_SplitView.IsPaneOpen = !Sidebar_SplitView.IsPaneOpen;
            Sidebar_Frame.Navigate(typeof(Setting_Page));
        }

        private void MultilineEdit_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Lyric_ListView.SelectionMode = ListViewSelectionMode.Multiple;
            SelectToolkit_Button.Visibility = Visibility.Visible;
            MultilineEdit_AppBarButton.Visibility = Visibility.Collapsed;
            Submit_AppBarButton.Visibility = Visibility.Visible;
            ChanageTime_MenuFlyoutItem.IsEnabled = false;
        }

        private void Submit_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Lyric_ListView.SelectionMode = ListViewSelectionMode.Single;
            SelectToolkit_Button.Visibility = Visibility.Collapsed;
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

    }
}
