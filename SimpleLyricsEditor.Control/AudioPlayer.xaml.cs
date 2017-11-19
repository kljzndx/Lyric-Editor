using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using SimpleLyricsEditor.BLL.Pickers;
using SimpleLyricsEditor.Core;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.Events;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricsEditor.Control
{
    public sealed partial class AudioPlayer : UserControl
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source), typeof(Music), typeof(AudioPlayer), new PropertyMetadata(Music.Empty));

        public static readonly DependencyProperty IsPlayProperty = DependencyProperty.Register(
            nameof(IsPlay), typeof(bool), typeof(AudioPlayer), new PropertyMetadata(false));

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

        private readonly MediaPlayer _player;
        private readonly object _positionLocker = new object();
        private readonly SystemMediaTransportControls _smtc;

        private bool _isPressShift;
        private bool _isPressSlider;

        private Music _musicTemp;
        private Settings _settings = Settings.Current;
        private RandomAccessStreamReference _thumbnailStream;

        public AudioPlayer()
        {
            InitializeComponent();

            _player = new MediaPlayer
            {
                AudioCategory = MediaPlayerAudioCategory.Media,
                AudioBalance = _settings.Balance,
                Volume = _settings.Volume
            };
            _smtc = _player.SystemMediaTransportControls;
            _smtc.ButtonPressed += SMTC_ButtonPressed;
            _smtc.PlaybackPositionChangeRequested += SMTC_PlaybackPositionChangeRequested;
            _smtc.PlaybackRateChangeRequested += SMTC_PlaybackRateChangeRequested;
            _player.CommandManager.IsEnabled = false;
            _player.MediaOpened += Player_MediaOpened;
            _player.MediaFailed += Player_MediaFailed;
            _player.MediaEnded += Player_MediaEnded;
            _player.PlaybackSession.PlaybackStateChanged += Player_PlaybackSession_PlaybackStateChanged;
            _player.PlaybackSession.PositionChanged += Player_PlaybackSession_PositionChanged;

            Rewind_Button.Opacity = 0;
            FastForward_Button.Opacity = 0;
            RewindButton_Transform.TranslateX = 44;
            FastForwardButton_Transform.TranslateX = -44;

            Position_Slider.AddHandler(PointerPressedEvent, new PointerEventHandler(Position_Slider_PointerPressed),
                true);
            Position_Slider.AddHandler(PointerReleasedEvent, new PointerEventHandler(Position_Slider_PointerReleased),
                true);

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

        public TimeSpan Position => _player.PlaybackSession.Position;
        public TimeSpan Duration => _player.PlaybackSession.NaturalDuration;

        public event TypedEventHandler<AudioPlayer, MusicChangeEventArgs> SourceChanged;
        public event TypedEventHandler<AudioPlayer, PositionChangeEventArgs> PositionChanged;
        public event TypedEventHandler<AudioPlayer, EventArgs> Playing;
        public event TypedEventHandler<AudioPlayer, EventArgs> Paused;

        public void SetSource(Music source)
        {
            if (source.Equals(Music.Empty))
                return;

            _musicTemp = source;

            _player.Source = MediaSource.CreateFromStorageFile(source.File);
        }

        private void EnableSmtcButton()
        {
            _smtc.IsEnabled = true;
            _smtc.IsPlayEnabled = true;
            _smtc.IsPauseEnabled = true;
            _smtc.IsStopEnabled = true;
            _smtc.IsNextEnabled = true;
            _smtc.IsFastForwardEnabled = true;
            _smtc.IsPreviousEnabled = true;
            _smtc.IsRewindEnabled = true;
        }

        private void RefreshPosition()
        {
            var currentPositon = _player.PlaybackSession.Position;
            Position_Slider.Value = currentPositon.TotalMinutes;
            SetSmtcPosition(currentPositon);

            PositionChanged?.Invoke(this, new PositionChangeEventArgs(false, currentPositon));
        }

        public void SetPosition(TimeSpan newPosition)
        {
            lock (_positionLocker)
            {
                _player.PlaybackSession.Position = newPosition;
                SetSmtcPosition(newPosition);

                Position_Storyboard.Stop();
                Position_Slider.Value = newPosition.TotalMinutes;
                if (IsPlay)
                    Position_Storyboard.Begin();

                PositionChanged?.Invoke(this, new PositionChangeEventArgs(true, newPosition));
            }
        }

        private void SetSmtcPosition(TimeSpan position)
        {
            var timeLine = new SystemMediaTransportControlsTimelineProperties
            {
                StartTime = TimeSpan.Zero,
                MinSeekTime = TimeSpan.Zero,
                Position = position,
                MaxSeekTime = _player.PlaybackSession.NaturalDuration,
                EndTime = _player.PlaybackSession.NaturalDuration
            };

            _smtc.UpdateTimelineProperties(timeLine);
        }

        public async Task<bool> PickMusicFile()
        {
            var file = await MusicFileOpenPicker.PickFile();

            if (file != null)
            {
                MusicFileNotifier.ChangeFile(file);
                return true;
            }
            return false;
        }

        public void SetSmtcAudioInfo(string title = "", string artist = "")
        {
            if (Source.Equals(Music.Empty))
                return;

            var updater = _smtc.DisplayUpdater;
            updater.MusicProperties.Title = title;
            updater.MusicProperties.Artist = artist;
            updater.Thumbnail = _thumbnailStream;
            updater.Update();
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
            _player.Play();
        }

        public void Pause()
        {
            _player.Pause();
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
            var newPosition = Position + time <= _player.PlaybackSession.NaturalDuration
                ? Position + time
                : _player.PlaybackSession.NaturalDuration;

            SetPosition(newPosition);
        }

        private async void MusicFileChanged(object sender, FileChangeEventArgs e)
        {
            SetSource(await Music.Parse(e.File));

            PlayOrPause_ToggleButton.Visibility = Visibility.Visible;
            OpenMusicFile_Button.Visibility = Visibility.Collapsed;
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

        private void OnKeyUp(object sender, GlobalKeyEventArgs e)
        {
            _isPressShift = e.IsPressShift;

            if (!_isPressShift)
            {
                Rewind_Button.Content = '\uE100';
                FastForward_Button.Content = '\uE101';
            }
        }

        private async void Player_MediaOpened(MediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    Source = _musicTemp;
                    _player.PlaybackSession.PlaybackRate = _settings.PlaybackRate;
                    _thumbnailStream = RandomAccessStreamReference.CreateFromStream(
                        (await Source.File.GetScaledImageAsThumbnailAsync(ThumbnailMode.MusicView)).CloneStream());

                    EnableSmtcButton();

                    SourceChanged?.Invoke(this, new MusicChangeEventArgs(Source));
                    DisplayPositionControlButtons();
                    Position_Slider.Maximum = sender.PlaybackSession.NaturalDuration.TotalMinutes;

                    var updater = _smtc.DisplayUpdater;
                    await updater.CopyFromFileAsync(MediaPlaybackType.Music, Source.File);
                    updater.Update();

                    Play();
                }
            );
        }

        private async void Player_MediaEnded(MediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Pause();
                SetPosition(TimeSpan.Zero);
            });
        }

        private async void Player_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (Source.Equals(Music.Empty))
                {
                    OpenMusicFile_Button.Visibility = Visibility.Visible;
                    PlayOrPause_ToggleButton.Visibility = Visibility.Collapsed;
                }
                else
                    SetSource(Source);

                throw new Exception(args.ErrorMessage);
            });
        }

        private async void Player_PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (sender.PlaybackState)
                {
                    case MediaPlaybackState.None:
                        _smtc.PlaybackStatus = MediaPlaybackStatus.Closed;
                        break;
                    case MediaPlaybackState.Opening:
                    case MediaPlaybackState.Buffering:
                        _smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
                        break;
                    case MediaPlaybackState.Playing:
                        _smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
                        IsPlay = true;
                        Position_Storyboard.Begin();
                        Playing?.Invoke(this, EventArgs.Empty);
                        break;
                    case MediaPlaybackState.Paused:
                        _smtc.PlaybackStatus = MediaPlaybackStatus.Paused;
                        IsPlay = false;
                        Position_Storyboard.Stop();
                        RefreshPosition();
                        Paused?.Invoke(this, EventArgs.Empty);
                        break;
                }
            });
        }

        private async void Player_PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    if (!_isPressSlider)
                        PositionChanged?.Invoke(this, new PositionChangeEventArgs(false, sender.Position));
                });
        }

        private async void SMTC_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (args.Button)
                {
                    case SystemMediaTransportControlsButton.Play:
                        Play();
                        break;
                    case SystemMediaTransportControlsButton.Pause:
                        Pause();
                        break;
                    case SystemMediaTransportControlsButton.Stop:
                        Pause();
                        SetPosition(TimeSpan.Zero);
                        break;
                    case SystemMediaTransportControlsButton.Next:
                    case SystemMediaTransportControlsButton.FastForward:
                        FastForward(FastForwardTimeOnPressedShift);
                        break;
                    case SystemMediaTransportControlsButton.Previous:
                    case SystemMediaTransportControlsButton.Rewind:
                        Rewind(RewindTimeOnPressedShift);
                        break;
                }
            });
        }

        private async void SMTC_PlaybackPositionChangeRequested(SystemMediaTransportControls sender, PlaybackPositionChangeRequestedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SetPosition(args.RequestedPlaybackPosition));
        }

        private async void SMTC_PlaybackRateChangeRequested(SystemMediaTransportControls sender, PlaybackRateChangeRequestedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => PlaybackRate_Slider.Value = args.RequestedPlaybackRate * 100);
        }

        private void Position_Slider_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Position_Storyboard.Stop();
            _isPressSlider = true;
        }

        private void Position_Slider_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Slider slider)
                SetPosition(TimeSpan.FromMinutes(slider.Value));
            _isPressSlider = false;
        }

        private void Position_Storyboard_Completed(object sender, object e)
        {
            Position_Storyboard.Stop();

            RefreshPosition();

            if (IsPlay)
                Position_Storyboard.Begin();
        }

        private void PlaybackRate_Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!Source.Equals(Music.Empty))
                _player.PlaybackSession.PlaybackRate = e.NewValue / 100;
        }

        private void Volume_Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!Source.Equals(Music.Empty))
                _player.Volume = e.NewValue / 100;
        }

        private void Balance_Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!Source.Equals(Music.Empty))
                _player.AudioBalance = e.NewValue / 100;
        }

        #region PlayerControlButton

        private async void OpenMusicFile_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenMusicFile_SymbolIcon.Visibility = Visibility.Collapsed;
            OpenMusicFile_Button.IsEnabled = false;
            MusicFileOpening_ProgressRing.IsActive = true;

            if (await PickMusicFile())
            {
                PlayOrPause_ToggleButton.Visibility = Visibility.Visible;
                OpenMusicFile_Button.Visibility = Visibility.Collapsed;
            }

            MusicFileOpening_ProgressRing.IsActive = false;
            OpenMusicFile_Button.IsEnabled = true;
            OpenMusicFile_SymbolIcon.Visibility = Visibility.Visible;
        }

        private void PlayOrPause_ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            (sender as ToggleButton).Content = '\uE103';

            if (!IsPlay)
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
    }
}