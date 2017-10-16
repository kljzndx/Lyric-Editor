using System;
using Windows.UI.Xaml;
using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Auxiliarys.Attributes;

namespace SimpleLyricsEditor.Core
{
    public sealed class Settings : SettingsBase
    {
        [SettingFieldByNormal(nameof(PlaybackRate), 1D)] private double _playbackRate;
        [SettingFieldByNormal(nameof(Balance), 0D)] private double _balance;
        [SettingFieldByNormal(nameof(Volume), 1D)] private double _volume;

        [SettingFieldByNormal(nameof(FrostedGlassVisibility), true)] private bool _frostedGlassVisibility;
        [SettingFieldByNormal(nameof(FrostedGlassOpacity), 0.4D)] private double _frostedGlassOpacity;

        [SettingFieldByEnum(nameof(PageTheme), typeof(ElementTheme), nameof(ElementTheme.Dark))] private ElementTheme _pageTheme;
        [SettingFieldByNormal(nameof(BackgroundVisibility), true)] private bool _backgroundVisibility;
        [SettingFieldByNormal(nameof(IsFollowSongAlbumCover),  true)] private bool _isFollowSongAlbumCover;
        [SettingFieldByNormal(nameof(BackgroundImagePath), "")] private string _backgroundImagePath;
        [SettingFieldByNormal(nameof(BackgroundBlurDegree), 5D)] private double _backgroundBlurDegree;
        [SettingFieldByNormal(nameof(BackgroundOpacity), 0.3D)] private double _backgroundOpacity;

        [SettingFieldByNormal(nameof(PreviewBackgroundOpacity), 0.3)] private double _previewBackgroundOpacity;
        [SettingFieldByNormal(nameof(SinglePreviewFontSize), 20D)] private double _singlePreviewFontSize;

        private Settings()
        {
            base.RenameSettingKey("IsDisplayBackground", nameof(BackgroundVisibility));
            if (base.SettingObject.Values.ContainsKey("BackgroundSourceType"))
                base.SettingObject.Values.Remove("BackgroundSourceType");
            
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

        public ElementTheme PageTheme
        {
            get => _pageTheme;
            set
            {
                SetSetting(ref _pageTheme, value, settingValue: value.ToString()); 
                OnPropertyChanged(nameof(IsLightTheme));
            }
        }

        public bool IsLightTheme => PageTheme == ElementTheme.Light || 
                                    PageTheme == ElementTheme.Default && Application.Current.RequestedTheme == ApplicationTheme.Light;
        
        public bool FrostedGlassVisibility
        {
            get => _frostedGlassVisibility;
            set => SetSetting(ref _frostedGlassVisibility, value);
        }

        public double FrostedGlassOpacity
        {
            get => _frostedGlassOpacity;
            set => SetSetting(ref _frostedGlassOpacity, value);
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
    }
}