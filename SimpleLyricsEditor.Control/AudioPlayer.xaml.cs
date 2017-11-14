using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;
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

        private bool _isPressShift;

        private Music _musicTemp;
        private Settings _settings = Settings.Current;
        private MediaPlayer _player;
        private bool _isPressSlider;

        public AudioPlayer()
        {
            InitializeComponent();

            _player = new MediaPlayer
            {
                AudioCategory = MediaPlayerAudioCategory.Media,
                AudioBalance = _settings.Balance,
                Volume = _settings.Volume,
            };
            _player.PlaybackSession.PlaybackRate = _settings.PlaybackRate;
            _player.MediaOpened += Player_MediaOpened;
            _player.MediaFailed += Player_MediaFailed;
            _player.MediaEnded += Player_MediaEnded;
            _player.PlaybackSession.PlaybackStateChanged += Player_PlaybackSession_PlaybackStateChanged;
            _player.PlaybackSession.PositionChanged += Player_PlaybackSession_PositionChanged;

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

        public void SetPosition(TimeSpan newPosition)
        {
            _player.PlaybackSession.Position = newPosition;
            Position_Storyboard.Stop();
            Position_Slider.Value = newPosition.TotalMinutes;
            if (IsPlay)
                Position_Storyboard.Begin();
            PositionChanged?.Invoke(this, new PositionChangeEventArgs(true, newPosition));
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

            Playing?.Invoke(this, EventArgs.Empty);
        }

        public void Pause()
        {
            _player.Pause();

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
            var newPosition = Position + time <= _player.PlaybackSession.NaturalDuration
                ? Position + time
                : _player.PlaybackSession.NaturalDuration;

            SetPosition(newPosition);
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
        
        private async void Player_MediaOpened(MediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Source = _musicTemp;
                SourceChanged?.Invoke(this, new MusicChangeEventArgs(Source));
                DisplayPositionControlButtons();
                Position_Slider.Maximum = sender.PlaybackSession.NaturalDuration.TotalMinutes;
                Play();
            });
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
            });

            throw new Exception(args.ErrorMessage);
        }

        private async void Player_PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (sender.PlaybackState)
                {
                    case MediaPlaybackState.None:
                        break;
                    case MediaPlaybackState.Opening:
                        break;
                    case MediaPlaybackState.Buffering:
                    case MediaPlaybackState.Playing:
                        IsPlay = true;
                        Position_Storyboard.Begin();
                        break;
                    case MediaPlaybackState.Paused:
                        IsPlay = false;
                        Position_Storyboard.Stop();
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

        private void Position_Slider_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Position_Storyboard.Stop();
            _isPressSlider = true;
        }

        private void Position_Slider_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Slider slider)
                SetPosition(TimeSpan.FromMinutes(slider.Value));

            Position_Storyboard.Begin();
            _isPressSlider = false;
        }

        private void Position_Storyboard_Completed(object sender, object e)
        {
            Position_Slider.Value = _player.PlaybackSession.Position.TotalMinutes;

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
    }
}