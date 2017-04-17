using HappyStudio.UwpToolsLibrary.Auxiliarys;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricEditor.Views.UserControls
{
    public sealed partial class AudioPlayer : UserControl
    {
        // Using a DependencyProperty as the backing store for IsPlay.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPlayProperty =
            DependencyProperty.Register(nameof(IsPlay), typeof(bool), typeof(AudioPlayer), new PropertyMetadata(false));


        // Using a DependencyProperty as the backing store for Music.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MusicProperty =
            DependencyProperty.Register(nameof(MusicSource), typeof(Music), typeof(AudioPlayer), new PropertyMetadata(Music.Empty));

        // Using a DependencyProperty as the backing store for Time.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register(nameof(Time), typeof(TimeSpan), typeof(AudioPlayer), new PropertyMetadata(new TimeSpan()));

        private ThreadPoolTimer ReloadTimeTimer;
        private Settings settings = Settings.GetSettingsObject();

        public event EventHandler Played;
        public event EventHandler Paused;
        public event EventHandler<MusicFileChangeEventArgs> MusicFileChanged;
        public event EventHandler<PositionChangeEventArgs> PositionChanged;

        public bool IsPlay
        {
            get { return (bool)GetValue(IsPlayProperty); }
            set { SetValue(IsPlayProperty, value); }
        }

        public bool IsAvailableSource { get => MusicSource != Music.Empty; }

        public Music MusicSource
        {
            get { return (Music)GetValue(MusicProperty); }
            set
            {
                SetValue(MusicProperty, value);
                this.Bindings.Update();
            }
        }

        public TimeSpan Time
        {
            get { return (TimeSpan)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }


        public AudioPlayer()
        {
            this.InitializeComponent();

            //为进度条订阅指针释放路由事件，至于为什么不在前台订阅嘛。。。自己看看最后一个参数就知道了
            Position_Slider.AddHandler(PointerReleasedEvent, new PointerEventHandler((s, e) => PositionChanged?.Invoke(this, new PositionChangeEventArgs(true, Time))), true);

            CoreWindow window = CoreWindow.GetForCurrentThread();
            window.KeyDown += Window_KeyDown;
            window.KeyUp += Window_KeyUp;
        }

        private void ReloadTime(bool isUserChange = false)
        {
            Time = AudioPlayer_MediaElement.Position;

            PositionChanged?.Invoke(this, new PositionChangeEventArgs(isUserChange, Time));
        }

        public async void ReloadTime(ThreadPoolTimer timer)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ReloadTime());
        }

        public void StartReloadTimeTimer()
        {
            ReloadTimeTimer = ThreadPoolTimer.CreatePeriodicTimer(ReloadTime, TimeSpan.FromMilliseconds(50), ReloadTime);
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

            MusicSource = await Music.Parse(musicFile);
            AudioPlayer_MediaElement.SetSource(await musicFile.OpenAsync(FileAccessMode.Read), musicFile.ContentType);
        }

        #region 播放控制

        public void Play()
        {
            if (!IsAvailableSource)
                OpenMusicFile();
            else
            {
                IsPlay = true;
                AudioPlayer_MediaElement.Play();
                StartReloadTimeTimer();
                Played?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Pause()
        {
            IsPlay = false;
            AudioPlayer_MediaElement.Pause();
            ReloadTimeTimer.Cancel();
            Paused?.Invoke(this, EventArgs.Empty);
        }

        public void FastRewind()
        {
            short ms = App.IsPressShift ? (short)5000 : (short)500;
            Time = Time >= TimeSpan.FromMilliseconds(ms) ? Time - TimeSpan.FromMilliseconds(ms) : TimeSpan.Zero;
            ReloadTime(true);
        }

        public void FastForward()
        {
            short ms = App.IsPressShift ? (short)5000 : (short)500;
            Time = Time <= MusicSource.AllTime - TimeSpan.FromMilliseconds(ms) ? Time + TimeSpan.FromMilliseconds(ms) : MusicSource.AllTime;
            ReloadTime(true);
        }
        #endregion

        private void Window_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Shift)
            {
                FastRewind_Button.Content = "\uE0A6";
                FastForward_Button.Content = "\uE0AB";
            }

            if (args.VirtualKey == VirtualKey.Left)
                FastRewind();

            if (args.VirtualKey == VirtualKey.Right)
                FastForward();

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

        private void AudioPlayer_MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            Play();
            MusicFileChanged?.Invoke(this, new MusicFileChangeEventArgs(MusicSource));
        }

        private void AudioPlayer_MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MusicSource = Music.Empty;
            Pause();
            MessageBox.ShowAsync(CharacterLibrary.ErrorDialog.GetString("OpenMusicError"), CharacterLibrary.ErrorDialog.GetString("Close"));
        }

        private void AudioPlayer_MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            Pause();
            Time = TimeSpan.Zero;
        }

    }
}
