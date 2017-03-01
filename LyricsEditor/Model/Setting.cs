using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace LyricsEditor.Model
{

    class Setting : Auxiliary
    {
        private ApplicationDataContainer settingsObject = ApplicationData.Current.LocalSettings;
        private static Setting thisObject;
        private static readonly object locker = new object();
        private double backgroundBlurDegree, backgroundOpacity, volume;
        private BackgroundImageTypeEnum backgroundImageType;
        private string userDefinedBackgroundImagePath;
        private bool isDisplayBackgroundImage, blurAvailability;
        private ApplicationTheme theme;
        private LyricChanageButtonBehavior chanageButtonBehavior;
        private LyricIDTag idTag = new LyricIDTag();
        private BitmapImage backgroundImage = new BitmapImage();

        public double BackgroundBlurDegree { get => backgroundBlurDegree; set => SetSetting(ref backgroundBlurDegree, value); }
        public double BackgroundOpacity { get => backgroundOpacity; set => SetSetting(ref backgroundOpacity, value); }
        public double Volume { get => volume; set => SetSetting(ref volume, value); }
        public string UserDefinedBackgroundImagePath { get => userDefinedBackgroundImagePath; set => SetSetting(ref userDefinedBackgroundImagePath, value); }
        public bool IsDisplayBackgroundImage { get => isDisplayBackgroundImage; set => SetSetting(ref isDisplayBackgroundImage, value); }
        public bool BlurAvailability { get => blurAvailability; }
        public ApplicationTheme Theme { get => theme; set => SetSetting(ref theme, value, value.ToString()); }
        public LyricChanageButtonBehavior ChanageButtonBehavior { get => chanageButtonBehavior; set => SetSetting(ref chanageButtonBehavior, value, value.ToString()); }
        public LyricIDTag IdTag { get => idTag; set => SetProperty(ref idTag, value); }
        public BitmapImage BackgroundImage { get => backgroundImage; set => SetProperty(ref backgroundImage, value); }
        public BackgroundImageTypeEnum BackgroundImageType { get => backgroundImageType; set => SetSetting(ref backgroundImageType, value, value.ToString()); }

        public ApplicationDataContainer SettingsObject { get => settingsObject; }

        private Setting()
        {
            RenameSettingKry("IsDisplayAlbumImageBackground", "IsDisplayBackgroundImage");
            CreateSetting("BackgroundBlurDegree", 5D);
            CreateSetting("BackgroundOpacity", 0.3);
            CreateSetting("UserDefinedBackgroundImagePath", String.Empty);
            CreateSetting(nameof(BackgroundImageType), BackgroundImageTypeEnum.AlbumImage.ToString());
            CreateSetting("Volume", 1D);
            CreateSetting("IsDisplayBackgroundImage", true);
            CreateSetting("Theme", ApplicationTheme.Light.ToString());
            CreateSetting("ChanageButtonBehavior", LyricChanageButtonBehavior.LetMeChoose.ToString());

            blurAvailability = SystemInfo.SystemVersion >= 14393;
            backgroundBlurDegree = blurAvailability ? GetSetting<double>("BackgroundBlurDegree") : 0D;
            backgroundOpacity = GetSetting<double>("BackgroundOpacity");
            userDefinedBackgroundImagePath = GetSetting<string>("UserDefinedBackgroundImagePath");
            backgroundImageType = GetSetting<BackgroundImageTypeEnum>(nameof(BackgroundImageType), s => s == BackgroundImageTypeEnum.AlbumImage.ToString() ? BackgroundImageTypeEnum.AlbumImage : BackgroundImageTypeEnum.UserDefined);
            volume = GetSetting<double>("Volume");
            isDisplayBackgroundImage = GetSetting<bool>("IsDisplayBackgroundImage");
            theme = GetSetting<ApplicationTheme>("Theme", s => s == ApplicationTheme.Light.ToString() ? ApplicationTheme.Light : ApplicationTheme.Dark);
            chanageButtonBehavior = GetSetting<LyricChanageButtonBehavior>("ChanageButtonBehavior",
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
            return (T)settingsObject.Values[key];
        }

        public T GetSetting<T>(string key, Func<string, T> converter)
        {
            return converter.Invoke(settingsObject.Values[key].ToString());
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
