using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using HappyStudio.UwpToolsLibrary.Auxiliarys;
using SimpleLyricsEditor.BLL.Pickers;
using SimpleLyricsEditor.Core;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.Events;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricsEditor.Control
{
    public sealed partial class AudioPlayer : UserControl
    {
        #region Locker

        private static readonly object PlayerPositionLocker = new object();

        #endregion

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
        private readonly Settings _settings = Settings.Current;

        public AudioPlayer()
        {
            InitializeComponent();
            
            Rewind_Button.Opacity = 0;
            FastForward_Button.Opacity = 0;
            RewindButton_Transform.TranslateX = 44;
            FastForwardButton_Transform.TranslateX = -44;
            
            Position_Slider.AddHandler(PointerReleasedEvent,
                                       new PointerEventHandler((s, e) => PositionChanged?.Invoke(this, new PositionChangeEventArgs(true, Player.Position))),
                                       true);

            GlobalKeyNotifier.KeyDown += OnKeyDown;
            GlobalKeyNotifier.KeyUp += OnKeyUp;
        }

        public Music Source
        {
            get => (Music)GetValue(SourceProperty);
            private set => SetValue(SourceProperty, value);
        }

        public bool IsPlay
        {
            get => (bool)GetValue(IsPlayProperty);
            private set
            {
                SetValue(IsPlayProperty, value);
                PlayOrPause_ToggleButton.IsChecked = value;
            }
        }

        public TimeSpan RewindTime
        {
            get => (TimeSpan)GetValue(RewindTimeProperty);
            set => SetValue(RewindTimeProperty, value);
        }

        public TimeSpan FastForwardTime
        {
            get => (TimeSpan)GetValue(FastForwardTimeProperty);
            set => SetValue(FastForwardTimeProperty, value);
        }

        public TimeSpan RewindTimeOnPressedShift
        {
            get => (TimeSpan)GetValue(RewindTimeOnPressedShiftProperty);
            set => SetValue(RewindTimeOnPressedShiftProperty, value);
        }

        public TimeSpan FastForwardTimeOnPressedShift
        {
            get => (TimeSpan)GetValue(FastForwardTimeOnPressedShiftProperty);
            set => SetValue(FastForwardTimeOnPressedShiftProperty, value);
        }

        public TimeSpan Position
        {
            get => Player.Position;
            private set => Player.Position = value;
        }

        public TimeSpan Duration => Player.NaturalDuration.TimeSpan;

        public event TypedEventHandler<AudioPlayer, MusicChangeEventArgs> SourceChanged;
        public event TypedEventHandler<AudioPlayer, PositionChangeEventArgs> PositionChanged;
        public event TypedEventHandler<AudioPlayer, EventArgs> Playing;
        public event TypedEventHandler<AudioPlayer, EventArgs> Paused;

        public async void SetSource(Music source)
        {
            if (source.Equals(Music.Empty))
                return;

            _musicTemp = source;

            DisplayOpenFileButton_Wait();
            Player.SetSource(await source.File.OpenAsync(FileAccessMode.Read), source.File.ContentType);
        }

        private void RefreshPosition()
        {
            lock (PlayerPositionLocker)
            {
                if (Source.Equals(Music.Empty))
                    return;
                
                Position_Slider.Value = Position.TotalMinutes;

                PositionChanged?.Invoke(this, new PositionChangeEventArgs(false, Position));
            }
        }

        public void SetPosition(TimeSpan newPosition)
        {
            lock (PlayerPositionLocker)
            {
                TimeSpan currentPosition = Position;
                Position_Slider.Value = newPosition.TotalMinutes;

                PositionChanged?.Invoke(this, new PositionChangeEventArgs(true, newPosition));
            }
        }
        
        public async Task<bool> PickMusicFile()
        {
            DisplayOpenFileButton_Wait();
            var file = await MusicFileOpenPicker.PickFile();

            if (file != null)
            {
                SetSource(await Music.Parse(file));
                return true;
            }

            DisplayOpenFileButton_Normal();
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

        private void DisplayOpenFileButton_Normal()
        {
            MusicFileOpening_ProgressRing.IsActive = false;
            OpenMusicFile_Button.IsEnabled = true;
            OpenMusicFile_SymbolIcon.Visibility = Visibility.Visible;
            OpenMusicFile_Button.Visibility = Visibility.Visible;
            PlayOrPause_ToggleButton.Visibility = Visibility.Collapsed;
        }

        private void DisplayOpenFileButton_Wait()
        {
            MusicFileOpening_ProgressRing.IsActive = true;
            OpenMusicFile_Button.IsEnabled = false;
            OpenMusicFile_SymbolIcon.Visibility = Visibility.Collapsed;
            OpenMusicFile_Button.Visibility = Visibility.Visible;
            PlayOrPause_ToggleButton.Visibility = Visibility.Collapsed;
        }

        private void HideOpenFileButton()
        {
            OpenMusicFile_Button.Visibility = Visibility.Collapsed;
            PlayOrPause_ToggleButton.Visibility = Visibility.Visible;
        }

        public void Play()
        {
            Player.Play();
        }

        public void Pause()
        {
            Player.Pause();
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
            var newPosition = Position + time <= Duration
                ? Position + time
                : Duration;

            SetPosition(newPosition);
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
        
        private void Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            Source = _musicTemp;
            HideOpenFileButton();

            SourceChanged?.Invoke(this, new MusicChangeEventArgs(Source));
            DisplayPositionControlButtons();
            SetPosition(TimeSpan.Zero);

            Play();
        }

        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            SetPosition(TimeSpan.Zero);
        }

        private void Player_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (Source.Equals(Music.Empty))
            {
                DisplayOpenFileButton_Normal();

                HidePositionControlButtons();
            }
            else
                SetSource(Source);

            throw new Exception(e.ErrorMessage);
        }

        private void Player_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            switch (Player.CurrentState)
            {
                case MediaElementState.Buffering:
                case MediaElementState.Playing:
                    IsPlay = true;
                    RefreshPosition();
                    Playing?.Invoke(this, EventArgs.Empty);
                    break;
                case MediaElementState.Paused:
                    IsPlay = false;
                    RefreshPosition();
                    Paused?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }

        private void Position_Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            PositionChanged?.Invoke(this, new PositionChangeEventArgs(false, Position));
        }
        
        #region PlayerControlButton

        private async void OpenMusicFile_Button_Click(object sender, RoutedEventArgs e)
        {
            await PickMusicFile();
        }

        private void PlayOrPause_ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ((ToggleButton)sender).Content = '\uE103';

            if (!IsPlay)
                Play();
        }

        private void PlayOrPause_ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ((ToggleButton)sender).Content = '\uE102';

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

        private void Position_HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton theButton)
            {
                theButton.Visibility = Visibility.Collapsed;
                Position_TextBox.Visibility = Visibility.Visible;
                Position_TextBox.Text = $"{Position.Minutes:D2}:{Position.Seconds:D2}.{Position.Milliseconds:D3}";
                Position_TextBox.Focus(FocusState.Pointer);
            }
        }

        private void Position_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var theBox = sender as TextBox;
            if (theBox is null)
                return;

            Regex timeRegex = new Regex(@"^(?<minutes>\d{1,2})\:(?<seconds>\d{1,2})(\.(?<milliseconds>\d{1,3}))?$");
            var match = timeRegex.Match(theBox.Text);
            if (match.Success)
            {
                byte.TryParse(match.Groups["minutes"].Value, out byte min);
                byte.TryParse(match.Groups["seconds"].Value, out byte ss);

                StringBuilder msStringBuilder = new StringBuilder(match.Groups["milliseconds"].Value);
                int needNum = 3 - msStringBuilder.Length;
                while (needNum > 0 && needNum < 3)
                {
                    msStringBuilder.Append("0");

                    needNum--;
                }

                int.TryParse(msStringBuilder.ToString(), out int ms);

                TimeSpan newPosition = new TimeSpan(0, 0, min, ss, ms);
                SetPosition(newPosition);
            }
            else
                MessageBox.ShowAsync(CharacterLibrary.ErrorInfo.GetString("TimeFormatError"), CharacterLibrary.MessageBox.GetString("Close"));

            theBox.Visibility = Visibility.Collapsed;
            Position_HyperlinkButton.Visibility = Visibility.Visible;
            Position_HyperlinkButton.Focus(FocusState.Pointer);
        }
    }
}