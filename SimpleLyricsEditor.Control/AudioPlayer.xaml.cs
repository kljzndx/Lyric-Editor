using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using SimpleLyricsEditor.BLL.Pickers;
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

        public static readonly DependencyProperty IsPlayProperty = DependencyProperty.Register(
            nameof(IsPlay), typeof(bool), typeof(AudioPlayer), new PropertyMetadata(false));

        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
            nameof(Time), typeof(TimeSpan), typeof(AudioPlayer), new PropertyMetadata(TimeSpan.Zero));

        public static readonly DependencyProperty RewindTimeProperty = DependencyProperty.Register(
            nameof(RewindTime), typeof(TimeSpan), typeof(AudioPlayer),
            new PropertyMetadata(TimeSpan.FromMilliseconds(500)));

        public static readonly DependencyProperty FastForwardTimeProperty = DependencyProperty.Register(
            nameof(FastForwardTime), typeof(TimeSpan), typeof(AudioPlayer),
            new PropertyMetadata(TimeSpan.FromMilliseconds(500)));

        public static readonly DependencyProperty RewindTimeOnPressedShiftProperty = DependencyProperty.Register(
            nameof(RewindTimeOnPressedShift), typeof(TimeSpan), typeof(AudioPlayer),
            new PropertyMetadata(TimeSpan.FromSeconds(5)));

        public static readonly DependencyProperty FastForwardTimeOnPressedShiftProperty = DependencyProperty.Register(
            nameof(FastForwardTimeOnPressedShift), typeof(TimeSpan), typeof(AudioPlayer),
            new PropertyMetadata(TimeSpan.FromSeconds(5)));

        private bool _isPressShift;

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

            MusicFileNotifier.FileChanged += MusicFileChanged;
            GlobalKeyNotifier.KeyDown += OnKeyDown;
            GlobalKeyNotifier.KeyUp += OnKeyUp;
        }

        public Music Source
        {
            get => (Music) GetValue(SourceProperty);
            private set => SetValue(SourceProperty, value);
        }

        public bool IsPlay
        {
            get => (bool) GetValue(IsPlayProperty);
            private set
            {
                SetValue(IsPlayProperty, value);
                PlayOrPause_ToggleButton.IsChecked = value;
            }
        }

        private TimeSpan Time
        {
            get => (TimeSpan) GetValue(TimeProperty);
            set => SetValue(TimeProperty, value);
        }

        public TimeSpan RewindTime
        {
            get => (TimeSpan) GetValue(RewindTimeProperty);
            set => SetValue(RewindTimeProperty, value);
        }

        public TimeSpan FastForwardTime
        {
            get => (TimeSpan) GetValue(FastForwardTimeProperty);
            set => SetValue(FastForwardTimeProperty, value);
        }

        public TimeSpan RewindTimeOnPressedShift
        {
            get => (TimeSpan) GetValue(RewindTimeOnPressedShiftProperty);
            set => SetValue(RewindTimeOnPressedShiftProperty, value);
        }

        public TimeSpan FastForwardTimeOnPressedShift
        {
            get => (TimeSpan) GetValue(FastForwardTimeOnPressedShiftProperty);
            set => SetValue(FastForwardTimeOnPressedShiftProperty, value);
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

        public async Task<bool> PickMusicFile()
        {
            var file = await MusicFileOpenPicker.PickFile();

            if (file != null)
            {
                MusicFileNotifier.ChangeFile(file);
                return true;
            }
            else
                return false;
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

            TimeSpan frequency = Debugger.IsAttached ? TimeSpan.FromSeconds(1) : TimeSpan.FromMilliseconds(100);
            
            _refreshTimeTimer = ThreadPoolTimer.CreatePeriodicTimer(refreshTime, frequency);
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
            Paused?.Invoke(this, EventArgs.Empty);
        }

        public void Rewind(TimeSpan time)
        {
            var newPosition = Position - time >= TimeSpan.Zero
                ? Position - time
                : TimeSpan.Zero;

            SetPosition(newPosition);
        }

        public void FastForward(TimeSpan time)
        {
            var newPosition = Position + time <= Player.NaturalDuration.TimeSpan
                ? Position + time
                : Player.NaturalDuration.TimeSpan;

            SetPosition(newPosition);
        }

        #region PlayerControlButton

        private async void PlayOrPause_ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton tb = sender as ToggleButton;
            tb.Content = '\uE103';

            if (Source.Equals(Music.Empty))
            {
                if (!await PickMusicFile())
                    tb.IsChecked = false;
            }
            else if (!IsPlay)
                Play();
        }

        private void PlayOrPause_ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            (sender as ToggleButton).Content = '\uE102';

            if (IsPlay)
                Pause();
        }

        private void Rewind_Button_Click(object sender, RoutedEventArgs e)
        {
            Rewind(_isPressShift ? RewindTimeOnPressedShift : RewindTime);
        }

        private void FastForward_Button_Click(object sender, RoutedEventArgs e)
        {
            FastForward(_isPressShift ? FastForwardTimeOnPressedShift : FastForwardTime);
        }

        #endregion
        
        private async void MusicFileChanged(object sender, FileChangeEventArgs e)
        {
            await SetSource(await Music.Parse(e.File));
        }

        private async void OnKeyDown(object sender, GlobalKeyEventArgs e)
        {
            _isPressShift = e.IsPressShift;

            if (_isPressShift)
            {
                Rewind_Button.Content = '\uE0A6';
                FastForward_Button.Content = '\uE0AB';
            }

            if (!e.IsInputing)
            {
                switch (e.Key)
                {
                    case VirtualKey.P:
                        if (IsPlay)
                            Pause();
                        else if (!Source.Equals(Music.Empty))
                            Play();
                        else
                            await PickMusicFile();
                        break;
                    case VirtualKey.Left:
                        Rewind(_isPressShift ? RewindTimeOnPressedShift : RewindTime);
                        break;
                    case VirtualKey.Right:
                        FastForward(_isPressShift ? FastForwardTimeOnPressedShift : FastForwardTime);
                        break;
                }
            }
        }

        private void OnKeyUp(object sender, GlobalKeyEventArgs e)
        {
            _isPressShift = e.IsPressShift;

            if (!_isPressShift)
            {
                Rewind_Button.Content = '\uE100';
                FastForward_Button.Content = '\uE101';
            }
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