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
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.System;
using Windows.System.Threading;
using LyricsEditor.Model;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Windows.UI.Core;
using System.Threading.Tasks;
using LyricsEditor.Pages;
using Windows.System.Profile;

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
        private ThreadPoolTimer displayTime_ThreadPoolTimer;
        private Setting settings = Setting.GetSettingObject();

        private bool isPressSlider = false;
        private bool InputBoxAvailableFocus = false;

        public MainPage()
        {
            this.InitializeComponent();
            CoreWindow.GetForCurrentThread().KeyDown += MainPage_KeyDown;
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await Background_Image.Blur(settings.BackgroundBlurDegree, 0, 0).StartAsync();


            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                Grid.SetRow(LyricEditButton_StackPanel, 1);
            else
                Grid.SetColumn(LyricEditButton_StackPanel, 1);
        }

        private void MainPage_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            var selectItem = Lyric_ListView.SelectedItem as Lyric;
            //添加歌词
            if (args.VirtualKey == VirtualKey.Space &&
                !InputBoxAvailableFocus &&
                selectItem is null)
            {
                AudioPlayer_MediaElement.Pause();
                SwitchDisplayPlayAndPauseButton(true);
                AddLyric();
            }
            //修改歌词
            if (args.VirtualKey == VirtualKey.Space &&
                !InputBoxAvailableFocus &&
                selectItem is Lyric)
            {
                selectItem.Time = AudioPlayer_MediaElement.Position;
                selectItem.Content = LyricContent_TextBox.Text;
                if(Lyric_ListView.SelectedIndex < lyrics.Count-1)
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
        private void DisplayTime()
        {
            var playTime = AudioPlayer_MediaElement.Position;
            if (!isPressSlider)
                PlayPosithon_Slider.Value = playTime.TotalMinutes;
        }
        private async void DisplayTime(ThreadPoolTimer timer)
        {
            await base.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, DisplayTime);
        }

        private void StartDisPlayTime()
        {
            displayTime_ThreadPoolTimer = ThreadPoolTimer.CreatePeriodicTimer(DisplayTime, TimeSpan.FromMilliseconds(100), DisplayTime);
        }
        /// <summary>
        /// 切换显示播放和暂停按钮
        /// </summary>
        /// <param name="isDisplayPlayButton">是否显示播放按钮</param>
        private void SwitchDisplayPlayAndPauseButton(bool isDisplayPlayButton)
        {
            if (isDisplayPlayButton)
            {
                Play_Button.Visibility = Visibility.Visible;
                Pause_Button.Visibility = Visibility.Collapsed;
            }
            else
            {
                Play_Button.Visibility = Visibility.Collapsed;
                Pause_Button.Visibility = Visibility.Visible;
            }
        }
        
        private void AddLyric()
        {
            if (!LyricContent_TextBox.Text.Trim().Contains('\n') && !LyricContent_TextBox.Text.Trim().Contains('\r'))
                lyrics.Add(new Lyric { Time = AudioPlayer_MediaElement.Position, Content = LyricContent_TextBox.Text.Trim() });
            else
            {
                char lineBreak = LyricContent_TextBox.Text.Contains('\n') ? '\n' : '\r';
                string[] lines = LyricContent_TextBox.Text.Trim().Split(lineBreak);
                foreach (var line in lines)
                {
                    lyrics.Add(new Lyric { Time = AudioPlayer_MediaElement.Position, Content = line.Trim() });
                }
            }
        }
#endregion
#region 播放器
        private void AudioPlayer_MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            (sender as MediaElement).Play();
            PlayPosithon_Slider.Maximum = music.Alltime.TotalMinutes;
            StartDisPlayTime();
            SwitchDisplayPlayAndPauseButton(false);
            GoBack_Button.IsEnabled = true;
            GoForward_Button.IsEnabled = true;
            settings.IdTag.Title = music.Name;
            settings.IdTag.Artist = music.Artist;
            settings.IdTag.Album = music.Album;
        }

        private void AudioPlayer_MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            displayTime_ThreadPoolTimer.Cancel();
            SwitchDisplayPlayAndPauseButton(true);
        }
#endregion
#region 进度条
        private void PlayPosithon_Slider_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            isPressSlider = true;
        }

        private void PlayPosithon_Slider_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            isPressSlider = false;
        }

        private void PlayPosithon_Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            AudioPlayer_MediaElement.Position = TimeSpan.FromMinutes(e.NewValue);
        }
#endregion
#region 播放器控制按钮
        private async void Play_Button_Click(object sender, RoutedEventArgs e)
        {
            if (music.File is null && await music.OpenFile())
                AudioPlayer_MediaElement.SetSource(await music.File.OpenAsync(Windows.Storage.FileAccessMode.Read), music.File.ContentType);
            else
            {
                AudioPlayer_MediaElement.Play();
                StartDisPlayTime();
            }
            SwitchDisplayPlayAndPauseButton(false);
        }

        private void Pause_Button_Click(object sender, RoutedEventArgs e)
        {
            AudioPlayer_MediaElement.Pause();
            displayTime_ThreadPoolTimer.Cancel();
            SwitchDisplayPlayAndPauseButton(true);
        }

        private void GoBack_Button_Click(object sender, RoutedEventArgs e)
        {
            double thisPlayTime = AudioPlayer_MediaElement.Position.TotalMinutes;
            double milliseconds = TimeSpan.FromMilliseconds(100).TotalMinutes;
            double time = thisPlayTime - milliseconds;
            AudioPlayer_MediaElement.Position = TimeSpan.FromMinutes(time);
            DisplayTime();
        }

        private void GoForward_Button_Click(object sender, RoutedEventArgs e)
        {
            double thisPlayTime = AudioPlayer_MediaElement.Position.TotalMinutes;
            double milliseconds = TimeSpan.FromMilliseconds(100).TotalMinutes;
            double time = thisPlayTime + milliseconds;
            AudioPlayer_MediaElement.Position = TimeSpan.FromMinutes(time);
            DisplayTime();
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
            foreach (Lyric item in Lyric_ListView.SelectedItems)
            {
                lyrics.Remove(item);
            }
        }

        private void ChanageTime_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (Lyric_ListView.SelectedItems.Count == 1)
                (Lyric_ListView.SelectedItem as Lyric).Time = AudioPlayer_MediaElement.Position;
            else
                foreach (Lyric item in Lyric_ListView.SelectedItems)
                {
                    item.Time = AudioPlayer_MediaElement.Position;
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
            {
                for (int i = lyrics.Count - 1; i > 0; i--)
                {
                    if (lyrics[i].CompareTo(lyrics[i - 1]) < 0)
                    {
                        lyrics.Move(i, i - 1);
                    }
                }
            }
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
                AudioPlayer_MediaElement.SetSource(await music.File.OpenAsync(Windows.Storage.FileAccessMode.Read), music.File.ContentType);
        }
        private async void OpenLRC_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            await LyricManager.OpenLRCAndAnalysis(lyrics, settings.IdTag);
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
        }

        private void Submit_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Lyric_ListView.SelectionMode = ListViewSelectionMode.Single;
            //SelectToolkit_Button.Visibility = Visibility.Collapsed;
            Submit_AppBarButton.Visibility = Visibility.Collapsed;
            MultilineEdit_AppBarButton.Visibility = Visibility.Visible;
        }

        #endregion

        private void Sidebar_SplitView_PaneClosed(SplitView sender, object args)
        {
            Sidebar_Frame.Navigate(typeof(Page));
        }

    }
}
