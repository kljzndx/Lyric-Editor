using LyricsEditor.Auxiliary;
using LyricsEditor.EventArg;
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
        private Music musicSource;
        
        public event EventHandler<PlayPositionUserChangeEventArgs> PlayPositionUserChange;
        public event EventHandler Played;
        public event EventHandler Paused;
        private bool isPressSlider = false;
        public bool IsPressSlider { get => isPressSlider; private set => UserChangePosition(ref isPressSlider, value); }
        public bool IsPlay { get => Pause_Button.Visibility == Visibility.Visible; }

        public TimeSpan PlayPosition
        {
            get { return (TimeSpan)GetValue(PlayPositionProperty); }
            set { SetValue(PlayPositionProperty, value); }
        }
        
        // Using a DependencyProperty as the backing store for PlayPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlayPositionProperty =
            DependencyProperty.Register(nameof(PlayPosition), typeof(TimeSpan), typeof(AudioPlayer), new PropertyMetadata(new TimeSpan()));
        

        public AudioPlayer()
        {
            this.InitializeComponent();
            musicSource = new Music();
            CoreWindow.GetForCurrentThread().KeyDown += AudioPlayer_KeyDown;
            CoreWindow.GetForCurrentThread().KeyUp += AudioPlayer_KeyUp;
            PlayPosithon_Slider.AddHandler(PointerPressedEvent, new PointerEventHandler(PlayPosithon_Slider_PointerPressed), true);
            PlayPosithon_Slider.AddHandler(PointerReleasedEvent, new PointerEventHandler(PlayPosithon_Slider_PointerReleased), true);
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
                AudioPlayer_MediaElement.Position < (musicSource.Alltime -= TimeSpan.FromMilliseconds(100)))
            {
                AudioPlayer_MediaElement.Position += TimeSpan.FromMilliseconds(100);
            }

            if (App.isPressShift && args.VirtualKey == VirtualKey.Left &&
                AudioPlayer_MediaElement.Position > TimeSpan.FromSeconds(5))
            {
                AudioPlayer_MediaElement.Position -= TimeSpan.FromSeconds(5);
            }

            if (App.isPressShift && args.VirtualKey == VirtualKey.Right &&
                AudioPlayer_MediaElement.Position < (musicSource.Alltime -= TimeSpan.FromSeconds(5)))
            {
                AudioPlayer_MediaElement.Position += TimeSpan.FromSeconds(5);
            }
            if (args.VirtualKey == VirtualKey.Shift)
            {
                GoBack_Button.Content = "\uE0A6";
                GoForward_Button.Content = "\uE0AB";
            }
        }

        private void AudioPlayer_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Shift)
            {
                GoBack_Button.Content = "\uE892";
                GoForward_Button.Content = "\uE893";
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
            displayTime_ThreadPoolTimer = ThreadPoolTimer.CreatePeriodicTimer(DisplayTime, TimeSpan.FromMilliseconds(50), DisplayTime);
        }

        public async void SwitchMusic(object sender, MusicChanageEventArgs e)
        {
            musicSource = e.NewMusic;
            this.Bindings.Update();
            AudioPlayer_MediaElement.SetSource(await musicSource.File.OpenAsync(Windows.Storage.FileAccessMode.Read), musicSource.File.ContentType);
        }
        /// <summary>
        /// 切换显示播放和暂停按钮
        /// </summary>
        /// <param name="isDisplayPlayButton">是否显示播放按钮</param>
        public void SwitchDisplayPlayAndPauseButton(bool isDisplayPlayButton)
        {
            if (isDisplayPlayButton)
            {
                Play_Button.Visibility = Visibility.Visible;
                Pause_Button.Visibility = Visibility.Collapsed;
                Paused?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Play_Button.Visibility = Visibility.Collapsed;
                Pause_Button.Visibility = Visibility.Visible;
                Played?.Invoke(this, EventArgs.Empty);
            }
        }

        private void UserChangePosition(ref bool pressSlider,bool value)
        {
            pressSlider = value;

            PlayPositionUserChange?.Invoke(this, new PlayPositionUserChangeEventArgs { IsChanging = value, Time = PlayPosition });
        }

        public void Play()
        {
            AudioPlayer_MediaElement.Play();
            StartDisPlayTime();
            SwitchDisplayPlayAndPauseButton(false);
        }

        public void Pause()
        {
            AudioPlayer_MediaElement.Pause();
            displayTime_ThreadPoolTimer.Cancel();
            SwitchDisplayPlayAndPauseButton(true);
        }

        #region 播放器
        private void AudioPlayer_MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            (sender as MediaElement).Play();
            PlayPosithon_Slider.Maximum = musicSource.Alltime.TotalMinutes;
            StartDisPlayTime();
            Play_Button.IsEnabled = true;
            GoBack_Button.IsEnabled = true;
            GoForward_Button.IsEnabled = true;
            settings.IdTag.Title = musicSource.Name;
            settings.IdTag.Artist = musicSource.Artist;
            settings.IdTag.Album = musicSource.Album;
            SwitchDisplayPlayAndPauseButton(false);
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
            IsPressSlider = true;
        }

        private void PlayPosithon_Slider_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IsPressSlider = false;
        }

        private void PlayPosithon_Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            PlayPosition = TimeSpan.FromMinutes(e.NewValue);
        }

        #endregion
        #region 播放器控制按钮
        private  void Play_Button_Click(object sender, RoutedEventArgs e)
        {
            Play();
        }

        private void Pause_Button_Click(object sender, RoutedEventArgs e)
        {
            Pause();
        }

        private void GoBack_Button_Click(object sender, RoutedEventArgs e)
        {
            AudioPlayer_MediaElement.Position -= App.isPressShift ? TimeSpan.FromSeconds(5) : TimeSpan.FromMilliseconds(500);
            DisplayTime();
            PlayPositionUserChange?.Invoke(this, new PlayPositionUserChangeEventArgs { IsChanging = false, Time = PlayPosition });
        }

        private void GoForward_Button_Click(object sender, RoutedEventArgs e)
        {
            AudioPlayer_MediaElement.Position += App.isPressShift ? TimeSpan.FromSeconds(5) : TimeSpan.FromMilliseconds(500);
            DisplayTime();
            PlayPositionUserChange?.Invoke(this, new PlayPositionUserChangeEventArgs { IsChanging = false, Time = PlayPosition });
        }

        #endregion

    }
}
