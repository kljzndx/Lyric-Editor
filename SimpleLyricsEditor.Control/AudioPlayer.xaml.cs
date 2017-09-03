using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using SimpleLyricsEditor.BLL;
using SimpleLyricsEditor.Core;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.Events;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricsEditor.Control
{
    public sealed partial class AudioPlayer : UserControl
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source), typeof(Music), typeof(AudioPlayer), new PropertyMetadata(Music.Empty));

        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
            nameof(Time), typeof(TimeSpan), typeof(AudioPlayer), new PropertyMetadata(TimeSpan.Zero));

        public static readonly DependencyProperty IsPlayProperty = DependencyProperty.Register(
            nameof(IsPlay), typeof(bool), typeof(AudioPlayer), new PropertyMetadata(false));

        private Music _musicTemp;
        private ThreadPoolTimer _refreshTimeTimer;
        private Settings _settings = Settings.Current;

        public AudioPlayer()
        {
            InitializeComponent();
            Rewind_Button.Opacity = 0;
            FastForward_Button.Opacity = 0;
            RewindButton_Transform.TranslateX = 44;
            FastForwardButton_Transform.TranslateX = -44;

            Position_Slider.AddHandler(PointerPressedEvent, new PointerEventHandler(Position_Slider_PointerPressed), true);
            Position_Slider.AddHandler(PointerReleasedEvent, new PointerEventHandler(Position_Slider_PointerReleased), true);

            MusicFileNotification.FileChanged += MusicFileChanged;
        }

        public Music Source
        {
            get => (Music) GetValue(SourceProperty);
            private set => SetValue(SourceProperty, value);
        }

        public bool IsPlay
        {
            get => (bool) GetValue(IsPlayProperty);
            private set => SetValue(IsPlayProperty, value);
        }

        private TimeSpan Time
        {
            get => (TimeSpan) GetValue(TimeProperty);
            set => SetValue(TimeProperty, value);
        }

        public TimeSpan Position => Player.Position;

        public event TypedEventHandler<AudioPlayer, MusicChangeEventArgs> SourceChanged;
        public event TypedEventHandler<AudioPlayer, PositionChangeEventArgs> PositionChanged;
        public event TypedEventHandler<AudioPlayer, EventArgs> Playing;
        public event TypedEventHandler<AudioPlayer, EventArgs> Paused;

        public async Task SetSource(Music source)
        {
            if (source.Equals(Music.Empty))
                return;

            _musicTemp = source;

            Player.SetSource(await _musicTemp.File.OpenAsync(FileAccessMode.Read), _musicTemp.File.ContentType);
        }

        public void SetPosition(TimeSpan newPosition)
        {
            Time = newPosition;
            Player.Position = Time;
            PositionChanged?.Invoke(this, new PositionChangeEventArgs(true, Time));
        }

        public async Task PickMusicFile()
        {
            var file = await MusicFileOpenPicker.PickFile();

            if (file != null)
                MusicFileNotification.ChangeFile(file);
        }

        private void RefreshTime()
        {
            Time = Player.Position;
            PositionChanged?.Invoke(this, new PositionChangeEventArgs(false, Time));
        }

        private void StartRefreshTimeTimer()
        {
            async void refreshTime(ThreadPoolTimer timer)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, RefreshTime);
            }

            _refreshTimeTimer = ThreadPoolTimer.CreatePeriodicTimer(refreshTime, TimeSpan.FromMilliseconds(50));
        }

        public void DisplayPositionControlButtons()
        {
            if (RewindButton_Transform.TranslateX.Equals(44))
                Expand.Begin();
        }

        public void HidePositionControlButtons()
        {
            if (RewindButton_Transform.TranslateX.Equals(0))
                Folding.Begin();
        }

        public void Play()
        {
            Player.Play();
            StartRefreshTimeTimer();
            Playing?.Invoke(this, EventArgs.Empty);
        }

        public void Pause()
        {
            Player.Pause();
            _refreshTimeTimer.Cancel();
            RefreshTime();
            Playing?.Invoke(this, EventArgs.Empty);
        }

        public void Rewind()
        {
            var ms = 500;
            var newPosition = Position - TimeSpan.FromMilliseconds(ms) >= TimeSpan.Zero ? Position - TimeSpan.FromMilliseconds(ms) : TimeSpan.Zero;

            SetPosition(newPosition);
        }

        public void FastForward()
        {
            var ms = 500;
            var newPosition = Position + TimeSpan.FromMilliseconds(ms) <= Player.NaturalDuration.TimeSpan ? Position + TimeSpan.FromMilliseconds(ms) : Player.NaturalDuration.TimeSpan;

            SetPosition(newPosition);
        }

        private async void Play_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Source.Equals(Music.Empty))
                await PickMusicFile();
            else
                Play();
        }

        private async void MusicFileChanged(object sender, FileChangeEventArgs e)
        {
            await SetSource(await Music.Parse(e.File));
        }

        private void Player_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            switch (Player.CurrentState)
            {
                case MediaElementState.Closed:
                    break;
                case MediaElementState.Opening:
                    break;
                case MediaElementState.Buffering:
                    break;
                case MediaElementState.Playing:
                    IsPlay = true;
                    break;
                case MediaElementState.Paused:
                case MediaElementState.Stopped:
                    IsPlay = false;
                    break;
            }
        }

        private void Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            Source = _musicTemp;
            SourceChanged?.Invoke(this, new MusicChangeEventArgs(Source));
            DisplayPositionControlButtons();
            Play();
        }

        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            Pause();
            SetPosition(TimeSpan.Zero);
        }

        private void Player_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            throw new Exception(e.ErrorMessage);
        }

        private void Position_Slider_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _refreshTimeTimer?.Cancel();
        }

        private void Position_Slider_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Slider slider)
                SetPosition(TimeSpan.FromMinutes(slider.Value));
            if (IsPlay)
                StartRefreshTimeTimer();
        }
    }
}