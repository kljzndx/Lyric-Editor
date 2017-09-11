using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Auxiliarys.Attributes;

namespace SimpleLyricsEditor.Core
{
    public sealed class Settings : SettingsBase
    {
        [SettingFieldByNormal(nameof(BackgroundBlurDegree), 5D)] private double _backgroundBlurDegree;
        [SettingFieldByNormal(nameof(BackgroundOpacity), 0.3D)] private double _backgroundOpacity;
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

        [SettingFieldByEnum(nameof(BackgroundSourceType), typeof(BackgroundSourceTypeEnum), nameof(BackgroundSourceTypeEnum.AlbumImage))] private BackgroundSourceTypeEnum _backgroundSourceType;

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