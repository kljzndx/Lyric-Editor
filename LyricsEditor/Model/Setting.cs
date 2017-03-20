using LyricsEditor.Auxiliary;
using LyricsEditor.Information;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace LyricsEditor.Model
{

    class Setting : BindableBase
    {
        private ApplicationDataContainer settingsObject = ApplicationData.Current.LocalSettings;
        private static Setting thisObject;
        private static readonly object locker = new object();
        private double backgroundBlurDegree, backgroundOpacity, volume, playSpeed, previewFontSize;
        private BackgroundImageTypeEnum backgroundImageType;
        private string userDefinedBackgroundImagePath;
        private bool isDisplayBackgroundImage, blurAvailability;
        private ElementTheme theme;
        private LyricChanageButtonBehavior chanageButtonBehavior;
        private LyricIDTag idTag = new LyricIDTag();

        public double BackgroundBlurDegree { get => backgroundBlurDegree; set => SetSetting(ref backgroundBlurDegree, value); }
        public double BackgroundOpacity { get => backgroundOpacity; set => SetSetting(ref backgroundOpacity, value); }
        public double Volume { get => volume; set { if (volume >= 0D && volume <= 1D) SetSetting(ref volume, value); } }
        public double PlaySpeed { get => playSpeed; set => SetSetting(ref playSpeed, value); }
        public double PreviewFontSize { get => previewFontSize; set => SetSetting(ref previewFontSize, value); }
        public string UserDefinedBackgroundImagePath { get => userDefinedBackgroundImagePath; set => SetSetting(ref userDefinedBackgroundImagePath, value); }
        public bool IsDisplayBackgroundImage { get => isDisplayBackgroundImage; set => SetSetting(ref isDisplayBackgroundImage, value); }
        public bool BlurAvailability { get => blurAvailability; }
        public ElementTheme Theme { get => theme; set => SetSetting(ref theme, value, value.ToString()); }
        public LyricChanageButtonBehavior ChanageButtonBehavior { get => chanageButtonBehavior; set => SetSetting(ref chanageButtonBehavior, value, value.ToString()); }
        public LyricIDTag IdTag { get => idTag; set => SetProperty(ref idTag, value); }
        public BackgroundImageTypeEnum BackgroundImageType { get => backgroundImageType; set => SetSetting(ref backgroundImageType, value, value.ToString()); }

        public ApplicationDataContainer SettingsObject { get => settingsObject; }
        

        private Setting()
        {
            RenameSettingKry("IsDisplayAlbumImageBackground", nameof(IsDisplayBackgroundImage));
            CreateSetting(nameof(BackgroundBlurDegree), 5D);
            CreateSetting(nameof(BackgroundOpacity), 0.3);
            CreateSetting(nameof(Volume), 1D);
            CreateSetting(nameof(PlaySpeed), 1D);
            CreateSetting(nameof(PreviewFontSize), SystemInfo.DeviceType == "Windows.Mobile" ? 18D : 24D);
            CreateSetting(nameof(UserDefinedBackgroundImagePath), String.Empty);
            CreateSetting(nameof(BackgroundImageType), BackgroundImageTypeEnum.AlbumImage.ToString());
            CreateSetting(nameof(IsDisplayBackgroundImage), true);
            CreateSetting(nameof(Theme), ElementTheme.Dark.ToString());
            CreateSetting(nameof(ChanageButtonBehavior), LyricChanageButtonBehavior.LetMeChoose.ToString());

            blurAvailability = SystemInfo.SystemVersion >= 14393;
            backgroundBlurDegree = blurAvailability ? GetSetting<double>(nameof(BackgroundBlurDegree)) : 0D;
            backgroundOpacity = GetSetting<double>(nameof(BackgroundOpacity));
            volume = GetSetting<double>(nameof(Volume));
            playSpeed = GetSetting<double>(nameof(PlaySpeed));
            previewFontSize = GetSetting<double>(nameof(PreviewFontSize));
            userDefinedBackgroundImagePath = GetSetting<string>(nameof(UserDefinedBackgroundImagePath));
            backgroundImageType = GetSetting<BackgroundImageTypeEnum>(nameof(BackgroundImageType), s => s == BackgroundImageTypeEnum.AlbumImage.ToString() ? BackgroundImageTypeEnum.AlbumImage : BackgroundImageTypeEnum.UserDefined);
            isDisplayBackgroundImage = GetSetting<bool>(nameof(IsDisplayBackgroundImage));
            theme = GetSetting<ElementTheme>(nameof(Theme), s => s == ElementTheme.Default.ToString() ? ElementTheme.Default : (s == ElementTheme.Light.ToString() ? ElementTheme.Light : ElementTheme.Dark));
            chanageButtonBehavior = GetSetting<LyricChanageButtonBehavior>(nameof(ChanageButtonBehavior),
                (s) =>
                {
                    if (s == LyricChanageButtonBehavior.ChanageTime.ToString())
                        return LyricChanageButtonBehavior.ChanageTime;
                    else if (s == LyricChanageButtonBehavior.ChanageLyric.ToString())
                        return LyricChanageButtonBehavior.ChanageLyric;
                    else if (s == LyricChanageButtonBehavior.BothTimeAndLyric.ToString())
                        return LyricChanageButtonBehavior.BothTimeAndLyric;
                    else
                        return LyricChanageButtonBehavior.LetMeChoose;
                }
            );
        }
        public static Setting GetSettingObject()
        {
            if (thisObject is null)
            {
                lock (locker)
                {
                    if (thisObject is null)
                    {
                        thisObject = new Setting();
                    }
                }
            }
            return thisObject;
        }
        public T GetSetting<T>(string key)
        {
            return (T)settingsObject.Values[$"{key[0].ToString().ToUpperInvariant()}{key.Remove(0,1)}"];
        }

        public T GetSetting<T>(string key,Func<string, T> converter)
        {
            return converter.Invoke(settingsObject.Values[$"{key[0].ToString().ToUpperInvariant()}{key.Remove(0, 1)}"].ToString());
        }

        public void CreateSetting(string key, object value)
        {
            if (!settingsObject.Values.ContainsKey(key))
            {
                settingsObject.Values[key] = value;
            }
        }

        public void RenameSettingKry(string oldKey, string newKey)
        {
            if (!settingsObject.Values.ContainsKey(oldKey))
                return;

            settingsObject.Values[newKey] = settingsObject.Values[oldKey];
            settingsObject.Values.Remove(oldKey);
        }

        private void SetSetting<T>(ref T Field, T value, object settingValue = null, [CallerMemberName] string propertyName = null)
        {
            settingValue = settingValue ?? value;
            SetProperty<T>(ref Field, value, propertyName);
            settingsObject.Values[propertyName] = settingValue;
        }
        
    }
}
