using LyricsEditor.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.System.Threading;
using Windows.UI.Core;
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
    public sealed partial class AudioPlayer : UserControl
    {
        private Setting settings = Setting.GetSettingObject();
        private ThreadPoolTimer displayTime_ThreadPoolTimer;
        private bool isPressSlider = false;

        public TimeSpan PlayPosition
        {
            get { return (TimeSpan)GetValue(PlayPositionProperty); }
            set { SetValue(PlayPositionProperty, value); }
        }
        
        public Music MusicSource
        {
            get { return (Music)GetValue(MusicSourceProperty); }
            set { SetValue(MusicSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for music.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MusicSourceProperty = 
            DependencyProperty.Register(nameof(MusicSource), typeof(Music), typeof(AudioPlayer), new PropertyMetadata(new Music()));
        
        // Using a DependencyProperty as the backing store for PlayPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlayPositionProperty =
            DependencyProperty.Register(nameof(PlayPosition), typeof(TimeSpan), typeof(AudioPlayer), new PropertyMetadata(new TimeSpan()));
        

        public AudioPlayer()
        {
            this.InitializeComponent();
            CoreWindow.GetForCurrentThread().KeyDown += AudioPlayer_KeyDown;
        }

        private void AudioPlayer_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Up)
            {

                settings.Volume += 0.1;
            }

            if (args.VirtualKey == VirtualKey.Down)
            {
                settings.Volume -= 0.1;
            }

            if (args.VirtualKey == VirtualKey.Left &&
                AudioPlayer_MediaElement.Position > TimeSpan.FromMilliseconds(100))
            {
                AudioPlayer_MediaElement.Position -= TimeSpan.FromMilliseconds(100);
            }

            if (args.VirtualKey == VirtualKey.Right &&
                AudioPlayer_MediaElement.Position < (MusicSource.Alltime -= TimeSpan.FromMilliseconds(100)))
            {
                AudioPlayer_MediaElement.Position += TimeSpan.FromMilliseconds(100);
            }

            if (App.isPressShift && args.VirtualKey == VirtualKey.Left &&
                AudioPlayer_MediaElement.Position > TimeSpan.FromSeconds(5))
            {
                AudioPlayer_MediaElement.Position -= TimeSpan.FromSeconds(5);
            }

            if (App.isPressShift && args.VirtualKey == VirtualKey.Right &&
                AudioPlayer_MediaElement.Position < (MusicSource.Alltime -= TimeSpan.FromSeconds(5)))
            {
                AudioPlayer_MediaElement.Position += TimeSpan.FromSeconds(5);
            }
        }
        
        private void DisplayTime()
        {
            var playTime = AudioPlayer_MediaElement.Position;
            PlayPosition = playTime;
            if (!isPressSlider)
                PlayPosithon_Slider.Value = playTime.TotalMinutes;
        }
        private async void DisplayTime(ThreadPoolTimer timer)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, DisplayTime);
        }

        private void StartDisPlayTime()
        {
            displayTime_ThreadPoolTimer = ThreadPoolTimer.CreatePeriodicTimer(DisplayTime, TimeSpan.FromMilliseconds(100), DisplayTime);
        }
        public async Task PlayMusic()
        {
            AudioPlayer_MediaElement.SetSource(await MusicSource.File.OpenAsync(Windows.Storage.FileAccessMode.Read), MusicSource.File.ContentType);
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
        #region 播放器
        private void AudioPlayer_MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            (sender as MediaElement).Play();
            PlayPosithon_Slider.Maximum = MusicSource.Alltime.TotalMinutes;
            StartDisPlayTime();
            SwitchDisplayPlayAndPauseButton(false);
            Play_Button.IsEnabled = true;
            GoBack_Button.IsEnabled = true;
            GoForward_Button.IsEnabled = true;
            settings.IdTag.Title = MusicSource.Name;
            settings.IdTag.Artist = MusicSource.Artist;
            settings.IdTag.Album = MusicSource.Album;
        }

        private async void AudioPlayer_MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            SwitchDisplayPlayAndPauseButton(true);
            Play_Button.IsEnabled = false;
            GoBack_Button.IsEnabled = false;
            GoForward_Button.IsEnabled = false;
            await MessageBox.ShowMessageBoxAsync(CharacterLibrary.MessageBox.GetString("MusicPlayError"), CharacterLibrary.MessageBox.GetString("Close"));
        }

        private void AudioPlayer_MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            displayTime_ThreadPoolTimer.Cancel();
            (sender as MediaElement).Position = new TimeSpan();
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
        private  void Play_Button_Click(object sender, RoutedEventArgs e)
        {
            AudioPlayer_MediaElement.Play();
            StartDisPlayTime();
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
            AudioPlayer_MediaElement.Position = TimeSpan.FromMinutes(AudioPlayer_MediaElement.Position.TotalMinutes - TimeSpan.FromMilliseconds(100).TotalMinutes);
            DisplayTime();
        }

        private void GoForward_Button_Click(object sender, RoutedEventArgs e)
        {
            AudioPlayer_MediaElement.Position = TimeSpan.FromMinutes(AudioPlayer_MediaElement.Position.TotalMinutes + TimeSpan.FromMilliseconds(100).TotalMinutes);
            DisplayTime();
        }

        private void GoBack_Button_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            AudioPlayer_MediaElement.Position = TimeSpan.FromMinutes(AudioPlayer_MediaElement.Position.TotalMinutes - TimeSpan.FromSeconds(5).TotalMinutes);
            DisplayTime();
        }

        private void GoForward_Button_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            AudioPlayer_MediaElement.Position = TimeSpan.FromMinutes(AudioPlayer_MediaElement.Position.TotalMinutes + TimeSpan.FromSeconds(5).TotalMinutes);
            DisplayTime();
        }

        #endregion

    }
}
