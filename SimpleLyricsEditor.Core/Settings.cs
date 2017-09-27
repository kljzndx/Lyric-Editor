using Windows.UI.Xaml;
using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Auxiliarys.Attributes;

namespace SimpleLyricsEditor.Core
{
    public sealed class Settings : SettingsBase
    {
        [SettingFieldByEnum(nameof(Theme), typeof(ElementTheme), nameof(ElementTheme.Dark))] private ElementTheme _theme;
        [SettingFieldByNormal(nameof(BackgroundVisibility), true)] private bool _backgroundVisibility;
        [SettingFieldByEnum(nameof(BackgroundSourceType), typeof(BackgroundSourceTypeEnum), nameof(BackgroundSourceTypeEnum.AlbumImage))] private BackgroundSourceTypeEnum _backgroundSourceType;
        [SettingFieldByNormal(nameof(LocalBackgroundImagePath), "")] private string _localBackgroundImagePath;
        [SettingFieldByNormal(nameof(BackgroundBlurDegree), 5D)] private double _backgroundBlurDegree;
        [SettingFieldByNormal(nameof(BackgroundOpacity), 0.3D)] private double _backgroundOpacity;
        
        [SettingFieldByNormal(nameof(Balance), 0D)] private double _balance;
        [SettingFieldByNormal(nameof(PlaybackRate), 1D)] private double _playbackRate;

        [SettingFieldByNormal(nameof(Volume), 1D)] private double _volume;

        private Settings()
        {
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

        public ElementTheme Theme
        {
            get => _theme;
            set => SetSetting(ref _theme, value, settingValue: value.ToString());
        }
        
        public bool BackgroundVisibility
        {
            get => _backgroundVisibility;
            set => SetSetting(ref _backgroundVisibility, value);
        }
        public BackgroundSourceTypeEnum BackgroundSourceType
        {
            get => _backgroundSourceType;
            set => SetSetting(ref _backgroundSourceType, value, settingValue: value.ToString());
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