using System;
using Windows.UI;
using Windows.UI.Xaml;
using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Auxiliarys.Attributes;
using HappyStudio.UwpToolsLibrary.Information;
using SimpleLyricsEditor.ValueConvert;

namespace SimpleLyricsEditor.Core
{
    public sealed class Settings : SettingsBase
    {
        [SettingFieldByNormal(nameof(PlaybackRate), 1D)] private double _playbackRate;
        [SettingFieldByNormal(nameof(Balance), 0D)] private double _balance;
        [SettingFieldByNormal(nameof(Volume), 1D)] private double _volume;

        [SettingFieldByEnum(nameof(PageTheme), typeof(ElementTheme), nameof(ElementTheme.Dark))] private ElementTheme _pageTheme;

        [SettingFieldByNormal(nameof(IsFrostedGlassEffectDisplay), true)] private bool _isFrostedGlassEffectDisplay;
        [SettingFieldByNormal(nameof(FrostedGlassOpacity), 0.65D)] private double _frostedGlassOpacity;

        [SettingFieldByNormal(nameof(IsDisplayBackground), true)] private bool _isDisplayBackground;
        [SettingFieldByNormal(nameof(IsFollowSongAlbumCover),  true)] private bool _isFollowSongAlbumCover;
        [SettingFieldByNormal(nameof(BackgroundImagePath), "")] private string _backgroundImagePath;
        [SettingFieldByNormal(nameof(BackgroundBlurDegree), 5D)] private double _backgroundBlurDegree;
        [SettingFieldByNormal(nameof(BackgroundOpacity), 0.3D)] private double _backgroundOpacity;

        [SettingFieldByNormal(nameof(MultilineEditModeEnabled), false)] private bool _multilineEditModeEnabled;

        [SettingFieldByNormal(nameof(PreviewBackgroundOpacity), 0.3)] private double _previewBackgroundOpacity;
        [SettingFieldByNormal(nameof(SinglePreviewFontSize), 20D)] private double _singlePreviewFontSize;
        [SettingFieldByNormal(nameof(CaraOkEffectEnabled), true)] private bool _caraOkEffectEnabled;
        private Color _caraOkEffectColor;

        private Settings()
        {
            base.RenameSettingKey("IsDisplayBackground", nameof(IsDisplayBackground));
            if (base.SettingObject.Values.ContainsKey("BackgroundSourceType"))
                base.SettingObject.Values.Remove("BackgroundSourceType");
            if (base.SettingObject.Values.ContainsKey("AdClickDate"))
                base.SettingObject.Values.Remove("AdClickDate");

            _caraOkEffectColor = base.GetSetting(nameof(CaraOkEffectColor), Colors.Red.ToHexString(),
                s => s.HexColorToColor());
            
            InitializeSettingFields();
        }

        public static Settings Current { get; } = new Settings();

        public double PlaybackRate
        {
            get => _playbackRate;
            set => SetSetting(ref _playbackRate, value);
        }

        public double Balance
        {
            get => _balance;
            set => SetSetting(ref _balance, value);
        }

        public double Volume
        {
            get => _volume;
            set => SetSetting(ref _volume, value);
        }

        public bool IsLightTheme => PageTheme == ElementTheme.Light || 
                                    PageTheme == ElementTheme.Default && Application.Current.RequestedTheme == ApplicationTheme.Light;
        
        public ElementTheme PageTheme
        {
            get => _pageTheme;
            set
            {
                SetSetting(ref _pageTheme, value, settingValue: value.ToString()); 
                OnPropertyChanged(nameof(IsLightTheme));
            }
        }

        public bool IsFrostedGlassUsability => SystemInfo.BuildVersion >= 15063;

        public bool IsFrostedGlassEffectDisplay
        {
            get
            {
                if (IsFrostedGlassUsability)
                    return _isFrostedGlassEffectDisplay;
                else
                    return false;
            }
            set => SetSetting(ref _isFrostedGlassEffectDisplay, value);
        }

        public double FrostedGlassOpacity
        {
            get => _frostedGlassOpacity;
            set => SetSetting(ref _frostedGlassOpacity, value);
        }

        public bool IsDisplayBackground
        {
            get => _isDisplayBackground;
            set => SetSetting(ref _isDisplayBackground, value);
        }

        public bool IsFollowSongAlbumCover
        {
            get => _isFollowSongAlbumCover;
            set => SetSetting(ref _isFollowSongAlbumCover, value);
        }

        public string BackgroundImagePath
        {
            get => _backgroundImagePath;
            set => SetSetting(ref _backgroundImagePath, value);
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

        public bool MultilineEditModeEnabled
        {
            get => _multilineEditModeEnabled;
            set => SetSetting(ref _multilineEditModeEnabled, value);
        }
        
        public double PreviewBackgroundOpacity
        {
            get => _previewBackgroundOpacity;
            set => SetSetting(ref _previewBackgroundOpacity, value);
        }

        public double SinglePreviewFontSize
        {
            get => _singlePreviewFontSize;
            set => SetSetting(ref _singlePreviewFontSize, value);
        }

        public bool CaraOkEffectEnabled
        {
            get => _caraOkEffectEnabled;
            set => SetSetting(ref _caraOkEffectEnabled, value);
        }

        public Color CaraOkEffectColor
        {
            get => _caraOkEffectColor;
            set => SetSetting(ref _caraOkEffectColor, value, settingValue: value.ToHexString());
        }
    }
}