using Windows.UI.Xaml;
using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Auxiliarys.Attributes;

namespace SimpleLyricsEditor.Core
{
    public sealed class Settings : SettingsBase
    {
        [SettingFieldByEnum(nameof(PageTheme), typeof(ElementTheme), nameof(ElementTheme.Dark))] private ElementTheme _pageTheme;
        [SettingFieldByNormal(nameof(BackgroundVisibility), true)] private bool _backgroundVisibility;
        [SettingFieldByNormal(nameof(IsFollowSongAlbumCover),  true)] private bool _isFollowSongAlbumCover;
        [SettingFieldByNormal(nameof(LocalBackgroundImagePath), "")] private string _localBackgroundImagePath;
        [SettingFieldByNormal(nameof(BackgroundBlurDegree), 5D)] private double _backgroundBlurDegree;
        [SettingFieldByNormal(nameof(BackgroundOpacity), 0.3D)] private double _backgroundOpacity;
        
        [SettingFieldByNormal(nameof(Balance), 0D)] private double _balance;
        [SettingFieldByNormal(nameof(PlaybackRate), 1D)] private double _playbackRate;

        [SettingFieldByNormal(nameof(Volume), 1D)] private double _volume;

        private Settings()
        {
            base.RenameSettingKey("IsDisplayBackground", nameof(BackgroundVisibility));
            
            InitializeSettingFields();
        }

        public static Settings Current { get; } = new Settings();

        public double PlaybackRate
        {
            get => _playbackRate;
            set => SetSetting(ref _playbackRate, value);
        }

        public double Volume
        {
            get => _volume;
            set => SetSetting(ref _volume, value);
        }

        public double Balance
        {
            get => _balance;
            set => SetSetting(ref _balance, value);
        }

        public ElementTheme PageTheme
        {
            get => _pageTheme;
            set => SetSetting(ref _pageTheme, value, settingValue: value.ToString());
        }
        
        public bool BackgroundVisibility
        {
            get => _backgroundVisibility;
            set => SetSetting(ref _backgroundVisibility, value);
        }
        public bool IsFollowSongAlbumCover
        {
            get => _isFollowSongAlbumCover;
            set => SetSetting(ref _isFollowSongAlbumCover, value);
        }
        
        public string LocalBackgroundImagePath
        {
            get => _localBackgroundImagePath;
            set => SetSetting(ref _localBackgroundImagePath, value);
        }

        public double BackgroundOpacity
        {
            get => _backgroundOpacity;
            set => SetSetting(ref _backgroundOpacity, value);
        }

        public double BackgroundBlurDegree
        {
            get => _backgroundBlurDegree;
            set => SetSetting(ref _backgroundBlurDegree, value);
        }
    }
}