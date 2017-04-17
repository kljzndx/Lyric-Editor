using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.Storage;
using HappyStudio.UwpToolsLibrary.Information;

namespace SimpleLyricEditor.Models
{
    public class Settings : ObservableObject
    {
        private static readonly object looker = new Object();
        private static Settings obj;
        public readonly ApplicationDataContainer SettingObject = ApplicationData.Current.LocalSettings;

        private double playbackRate;
        public double PlaybackRate { get => playbackRate; set => SetSetting(ref playbackRate, value); }

        private double volume;
        public double Volume { get => volume; set => SetSetting(ref volume, value); }

        private ElementTheme pageTheme;
        public ElementTheme PageTheme { get => pageTheme; set => SetSetting(ref pageTheme, value, value.ToString()); }

        private bool isDisplayBackGround;
        public bool IsDisplayBackground { get => isDisplayBackGround; set => SetSetting(ref isDisplayBackGround, value); }

        private BackgroundSourceTypeEnum backgroundSourceType;
        public BackgroundSourceTypeEnum BackgroundSourceType { get => backgroundSourceType; set => SetSetting(ref backgroundSourceType, value, value.ToString()); }

        private string localBackgroundImagePath;
        public string LocalBackgroundImagePath { get => localBackgroundImagePath; set => SetSetting(ref localBackgroundImagePath, value); }

        public bool IsBlurUsability { get; } = SystemInfo.BuildVersion >= 14393;

        private double backgroundBlurDegree;
        public double BackgroundBlurDegree { get => backgroundBlurDegree; set => SetSetting(ref backgroundBlurDegree, value); }

        private double backgroundOpacity;
        public double BackgroundOpacity { get => backgroundOpacity; set => SetSetting(ref backgroundOpacity, value); }

        private bool isDisplayLyricsPreview;
        public bool IsDisplayLyricsPreview { get => isDisplayLyricsPreview; set => SetSetting(ref isDisplayLyricsPreview, value); }

        private double previewFontSize;
        public double PreviewFontSize { get => previewFontSize; set => SetSetting(ref previewFontSize, value); }

        private double previewBackgroundOpacity;
        public double PreviewBackgroundOpacity { get => previewBackgroundOpacity; set => SetSetting(ref previewBackgroundOpacity, value); }


        private Settings()
        {
            //旧设置迁移
            RenameSetting("PlaySpeed", nameof(PlaybackRate));
            RenameSetting("BackgroundImageType", nameof(BackgroundSourceType));
            ChangeSetting(nameof(BackgroundSourceType), "UserDefined", "LocalImage");
            RenameSetting("UserDefinedBackgroundImagePath", nameof(LocalBackgroundImagePath));
            
            playbackRate = GetSetting(nameof(PlaybackRate), 1D);
            volume = GetSetting(nameof(Volume), 1D);

            pageTheme = GetSetting(nameof(PageTheme), ElementTheme.Dark.ToString(),
                s => /* if */ s == ElementTheme.Default.ToString() ? ElementTheme.Default : /* else if */ s == ElementTheme.Light.ToString() ? ElementTheme.Light : ElementTheme.Dark);

            isDisplayBackGround = GetSetting(nameof(IsDisplayBackground), true);

            backgroundSourceType = GetSetting(nameof(BackgroundSourceType), BackgroundSourceTypeEnum.AlbumImage.ToString(),
                s => s == BackgroundSourceTypeEnum.AlbumImage.ToString() ? BackgroundSourceTypeEnum.AlbumImage : BackgroundSourceTypeEnum.LocalImage);

            localBackgroundImagePath = GetSetting(nameof(LocalBackgroundImagePath), String.Empty);
            backgroundBlurDegree = IsBlurUsability ? GetSetting(nameof(BackgroundBlurDegree), 5D) : 0D;
            backgroundOpacity = GetSetting(nameof(BackgroundOpacity), 0.3);
            isDisplayLyricsPreview = GetSetting(nameof(isDisplayLyricsPreview), true);
            previewFontSize = GetSetting(nameof(PreviewFontSize), SystemInfo.DeviceType == "Windows.Mobile" ? 18D : 24D);
            previewBackgroundOpacity = GetSetting(nameof(PreviewBackgroundOpacity), 0.3);
        }
        
        public static Settings GetSettingsObject()
        {
            if (obj is null)
                lock (looker)
                    if (obj is null)
                        obj = new Settings();
            return obj;
        }

        public void RenameSetting(string oldKey, string newKey)
        {
            if (SettingObject.Values.ContainsKey(oldKey))
            {
                SettingObject.Values[newKey] = SettingObject.Values[oldKey];
                SettingObject.Values.Remove(oldKey);
            }
        }

        public void ChangeSetting(string key,object oldValue,object newValue)
        {
            if (SettingObject.Values.ContainsKey(key) && SettingObject.Values[key] == oldValue)
                SettingObject.Values[key] = newValue;
        }
        
        public T GetSetting<T>(string key, T defaultValue)
        {
            if (!SettingObject.Values.ContainsKey(key))
                SettingObject.Values[key] = defaultValue;
            return (T)SettingObject.Values[key];
        }

        public T GetSetting<T>(string key, string defaultValue, Func<string, T> convert)
        {
            if (!SettingObject.Values.ContainsKey(key))
                SettingObject.Values[key] = defaultValue;
            return convert(SettingObject.Values[key].ToString());
        }

        public void SetSetting<T>(ref T field, T value, object settingValue = null, [CallerMemberName] string propertyName = null)
        {
            if (Object.Equals(field, value))
                return;

            field = value;
            RaisePropertyChanged(propertyName);
            SettingObject.Values[propertyName] = settingValue ?? value;
        }
    }
}
