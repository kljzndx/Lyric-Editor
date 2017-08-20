using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Auxiliarys.Attributes;

namespace SimpleLyricsEditor.Core
{
    public sealed class Settings : SettingsBase
    {
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
    }
}