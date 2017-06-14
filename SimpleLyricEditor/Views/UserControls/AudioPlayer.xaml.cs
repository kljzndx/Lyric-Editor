using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Information;
using SimpleLyricEditor.EventArgss;
using SimpleLyricEditor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Pickers;
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
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Composition;
using SimpleLyricEditor.Extensions;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricEditor.Views.UserControls
{
    public sealed partial class AudioPlayer : UserControl
    {
        // Using a DependencyProperty as the backing store for MusicSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MusicSourceProperty =
            DependencyProperty.Register(nameof(MusicSource), typeof(Music), typeof(AudioPlayer), new PropertyMetadata(Music.Empty));

        // Using a DependencyProperty as the backing store for Time.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register(nameof(Time), typeof(TimeSpan), typeof(AudioPlayer), new PropertyMetadata(new TimeSpan()));

        private ThreadPoolTimer RefreshTime_Timer;
        private ThreadPoolTimer RefreshSMTCTime_Timer;
        private Settings settings = Settings.GetSettingsObject();


        public event TypedEventHandler<AudioPlayer, EventArgs> Played;
        public event TypedEventHandler<AudioPlayer, EventArgs> Paused;
        public event TypedEventHandler<AudioPlayer, MusicFileChangeEventArgs> MusicFileChanged;
        public event TypedEventHandler<AudioPlayer, PositionChangeEventArgs> PositionChanged;

        private bool isPlay;
        private SystemMediaTransportControls systemMediaTransportControls;

        public bool IsPlay
        {
            get => isPlay;
            private set
            {
                isPlay = value;
                this.Bindings.Update();
            }
        }

        public bool IsAvailableSource { get => MusicSource != Music.Empty; }

        /// <summary>
        /// 音乐源
        /// </summary>
        public Music MusicSource
        {
            get { return (Music)GetValue(MusicSourceProperty); }
            private set
            {
                SetValue(MusicSourceProperty, value);
                this.Bindings.Update();
            }
        }
        /// <summary>
        /// 当前进度
        /// </summary>
        public TimeSpan Time
        {
            get { return (TimeSpan)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }


        public AudioPlayer()
        {
            this.InitializeComponent();
            SetupSystemMediaTransportControls();
            Application.Current.Suspending += Application_Suspending;
            Application.Current.Resuming += Application_Resuming;

            //为进度条订阅指针释放路由事件，至于为什么不在前台订阅嘛。。。自己看看最后一个参数就知道了
            Position_Slider.AddHandler(PointerReleasedEvent, new PointerEventHandler((s, e) => SetTime(Time)), true);
            
            //获取窗口对象以订阅全局的键 按下和弹起 事件
            CoreWindow window = CoreWindow.GetForCurrentThread();
            window.KeyDown += Window_KeyDown;
            window.KeyUp += Window_KeyUp;
            Unloaded += AudioPlayer_Unloaded;
        }
        
        private void RefreshTime()
        {
            Time = AudioPlayer_MediaElement.Position;

            PositionChanged?.Invoke(this, new PositionChangeEventArgs(false, Time));
        }

        private void StartRefreshTimeTimer()
        {
            async void reloadTime(ThreadPoolTimer timer) => await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, RefreshTime);

            RefreshTime_Timer = ThreadPoolTimer.CreatePeriodicTimer(reloadTime, TimeSpan.FromMilliseconds(50));
        }

        private void RefreshSMTCTime()
        {
            var smtcTimeLineProperties = new SystemMediaTransportControlsTimelineProperties()
            {
                StartTime = TimeSpan.Zero,
                MinSeekTime = TimeSpan.Zero,
                Position = AudioPlayer_MediaElement.Position,
                MaxSeekTime = MusicSource.AllTime,
                EndTime = MusicSource.AllTime
            };
            systemMediaTransportControls.UpdateTimelineProperties(smtcTimeLineProperties);
        }

        private void StartRefreshSMTCTimeTimer()
        {
            async void refreshSMTCTimer(ThreadPoolTimer timer) => await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, RefreshSMTCTime);

            RefreshSMTCTime_Timer = ThreadPoolTimer.CreatePeriodicTimer(refreshSMTCTimer, TimeSpan.FromSeconds(5));
        }

        public async void OpenMusicFile()
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".flac");
            picker.FileTypeFilter.Add(".wav");
            picker.FileTypeFilter.Add(".aac");
            picker.FileTypeFilter.Add(".m4a");

            StorageFile musicFile = await picker.PickSingleFileAsync();
            if (musicFile is null)
                return;

            SetSource(await Music.ParseAsync(musicFile));
        }

        public void SetTime(TimeSpan time)
        {
            Time = time;
            RefreshSMTCTime();

            PositionChanged?.Invoke(this, new PositionChangeEventArgs(true, time));
        }

        public async void SetSource(Music newSource)
        {
            MusicSource = newSource;
            AudioPlayer_MediaElement.SetSource(await newSource.File.OpenAsync(FileAccessMode.Read), newSource.File.ContentType);
        }

        public void SetupSystemMediaTransportControls()
        {
            systemMediaTransportControls = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
            systemMediaTransportControls.IsPlayEnabled = true;
            systemMediaTransportControls.IsPauseEnabled = true;
            systemMediaTransportControls.IsRewindEnabled = true;
            systemMediaTransportControls.IsFastForwardEnabled = true;
            systemMediaTransportControls.IsPreviousEnabled = true;
            systemMediaTransportControls.IsNextEnabled = true;
            systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Closed;
            systemMediaTransportControls.ButtonPressed += SystemMediaTransportControls_ButtonPressed;
            systemMediaTransportControls.PlaybackPositionChangeRequested += async (s, e) => await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SetTime(e.RequestedPlaybackPosition));
            systemMediaTransportControls.PropertyChanged += SystemMediaTransportControls_PropertyChanged;
        }
        
        #region 播放控制

        public void Play()
        {
            if (IsAvailableSource)
                AudioPlayer_MediaElement.Play();
            else
                OpenMusicFile();
        }

        public void Pause()
        {
            AudioPlayer_MediaElement.Pause();
        }

        public void FastRewind()
        {
            short ms = App.IsPressShift ? (short)5000 : (short)500;
            SetTime(Time >= TimeSpan.FromMilliseconds(ms) ? Time - TimeSpan.FromMilliseconds(ms) : TimeSpan.Zero);
            RefreshSMTCTime();
        }

        public void FastForward()
        {
            short ms = App.IsPressShift ? (short)5000 : (short)500;
            SetTime(Time <= MusicSource.AllTime - TimeSpan.FromMilliseconds(ms) ? Time + TimeSpan.FromMilliseconds(ms) : MusicSource.AllTime);
            RefreshSMTCTime();
        }
        #endregion

        private void Application_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            RefreshTime_Timer?.Cancel();
        }

        private void Application_Resuming(object sender, object e)
        {
            SetTime(AudioPlayer_MediaElement.Position);
            if (isPlay)
                StartRefreshTimeTimer();
        }

        private async void SystemMediaTransportControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (args.Button)
                {
                    case SystemMediaTransportControlsButton.Play:
                        AudioPlayer_MediaElement.Play();
                        break;
                    case SystemMediaTransportControlsButton.Pause:
                        AudioPlayer_MediaElement.Pause();
                        break;
                    case SystemMediaTransportControlsButton.Rewind:
                    case SystemMediaTransportControlsButton.Previous:
                        FastRewind();
                        break;
                    case SystemMediaTransportControlsButton.FastForward:
                    case SystemMediaTransportControlsButton.Next:
                        FastForward();
                        break;
                }
            });
        }

        private async void SystemMediaTransportControls_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            if (args.Property == SystemMediaTransportControlsProperty.SoundLevel)
            {
                switch (sender.SoundLevel)
                {
                    case SoundLevel.Muted:
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, AudioPlayer_MediaElement.Pause);
                        break;
                    case SoundLevel.Low:
                    case SoundLevel.Full:
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, AudioPlayer_MediaElement.Play);
                        break;
                }
            }
        }

        private void Window_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Shift)
            {
                FastRewind_Button.Content = "\uE0A6";
                FastForward_Button.Content = "\uE0AB";
            }
            if (!App.IsInputBoxGotFocus)
            {
                switch (args.VirtualKey)
                {
                    case VirtualKey.P:
                        if (IsPlay)
                            Pause();
                        else
                            Play();
                        break;
                    case VirtualKey.Left:
                        FastRewind();
                        break;
                    case VirtualKey.Right:
                        FastForward();
                        break;
                }
            }
            if (args.VirtualKey == VirtualKey.M && App.IsPressCtrl)
                OpenMusicFile();
        }

        private void Window_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Shift)
            {
                FastRewind_Button.Content = "\uE100";
                FastForward_Button.Content = "\uE101";
            }
        }

        private void AudioPlayer_Unloaded(object sender, RoutedEventArgs e)
        {
            //用于解决内存泄露
            CoreWindow window = CoreWindow.GetForCurrentThread();
            window.KeyDown -= Window_KeyDown;
            window.KeyUp -= Window_KeyUp;
        }

        private void AudioPlayer_MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            Play();
            MusicFileChanged?.Invoke(this, new MusicFileChangeEventArgs(MusicSource));
        }

        private void AudioPlayer_MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MusicSource = Music.Empty;
            Pause();
            MessageBox.ShowAsync(
                CharacterLibrary.ErrorDialog.GetString("OpenMusicError") +
                Environment.NewLine +
                AppInfo.Name == "简易歌词编辑器" ? "以下为错误详细信息" : "The following is error detail" +
                Environment.NewLine +
                e.ErrorMessage,
                CharacterLibrary.ErrorDialog.GetString("Close")
            );
        }

        private void AudioPlayer_MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            Pause();
            SetTime(TimeSpan.Zero);
        }
        
        private void AudioPlayer_MediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            switch ((sender as MediaElement).CurrentState)
            {
                case MediaElementState.Closed:
                    systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Closed;
                    break;
                case MediaElementState.Buffering:
                    systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Changing;
                    break;
                case MediaElementState.Playing:
                    systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;

                    IsPlay = true;
                    Pause_Button.Focus(FocusState.Pointer);

                    if (!App.IsSuspend)
                        StartRefreshTimeTimer();
                    StartRefreshSMTCTimeTimer();
                    Played?.Invoke(this, EventArgs.Empty);
                    break;
                case MediaElementState.Paused:
                    systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;

                    IsPlay = false;
                    Play_Button.Focus(FocusState.Pointer);

                    RefreshTime_Timer?.Cancel();
                    RefreshSMTCTime_Timer?.Cancel();
                    Paused?.Invoke(this, EventArgs.Empty);
                    break;
                case MediaElementState.Stopped:
                    systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Stopped;

                    break;
            }
        }
    }
}
