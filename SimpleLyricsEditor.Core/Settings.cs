using Windows.UI.Xaml;
using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Auxiliarys.Attributes;

namespace SimpleLyricsEditor.Core
{
    public sealed class Settings : SettingsBase
    {
        [SettingFieldByNormal(nameof(BackgroundBlurDegree), 5D)] private double _backgroundBlurDegree;
        [SettingFieldByNormal(nameof(BackgroundOpacity), 0.3D)] private double _backgroundOpacity;

        [SettingFieldByEnum(nameof(BackgroundSourceType), typeof(BackgroundSourceTypeEnum),
            nameof(BackgroundSourceTypeEnum.AlbumImage))] private BackgroundSourceTypeEnum _backgroundSourceType;

        [SettingFieldByNormal(nameof(Balance), 0D)] private double _balance;

        [SettingFieldByNormal(nameof(PlaybackRate), 1D)] private double _playbackRate;

        [SettingFieldByEnum(nameof(Theme), typeof(ElementTheme), "Dark")] private ElementTheme _theme;
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

        public BackgroundSourceTypeEnum BackgroundSourceType
        {
            get => _backgroundSourceType;
            set => SetSetting(ref _backgroundSourceType, value, settingValue: value.ToString());
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