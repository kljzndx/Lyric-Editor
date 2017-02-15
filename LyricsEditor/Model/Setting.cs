using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.Xaml;

namespace LyricsEditor.Model
{
    class Setting : Auxiliary
    {
        private ApplicationDataContainer settingsObject = ApplicationData.Current.LocalSettings;
        private static Setting thisObject;
        private static readonly object locker = new object();
        private double backgroundBlurDegree, backgroundOpacity, volume;
        private bool isDisplayAlbumImageBackground, blurAvailability;
        private ApplicationTheme theme;
        private LyricChanageButtonBehavior chanageButtonBehavior;
        private LyricIDTag idTag = new LyricIDTag();

        public double BackgroundBlurDegree
        {
            get => backgroundBlurDegree;
            set
            {
                SetSetting(ref backgroundBlurDegree, value);
            }
        }
        public double BackgroundOpacity
        {
            get => backgroundOpacity;
            set
            {
                SetSetting(ref backgroundOpacity, value);
            }
        }
        public double Volume
        {
            get => volume;
            set
            {
                SetSetting(ref volume, value);
            }
        }
        public bool IsDisplayAlbumImageBackground
        {
            get => isDisplayAlbumImageBackground;
            set
            {
                SetSetting(ref isDisplayAlbumImageBackground, value);
            }
        }
        public bool BlurAvailability { get => blurAvailability; set => blurAvailability = value; }
        public ApplicationTheme Theme
        {
            get => theme;
            set
            {
                SetSetting(ref theme, value,value.ToString());
            }
        }
        public LyricChanageButtonBehavior ChanageButtonBehavior
        {
            get => chanageButtonBehavior;
            set
            {
                SetSetting(ref chanageButtonBehavior, value, value.ToString());
            }
        }
        public LyricIDTag IdTag
        {
            get => idTag;
            set
            {
                SetProperty(ref idTag, value);
            }
        }

        public ApplicationDataContainer SettingsObject { get => settingsObject; }
        

        private Setting()
        {
            CreateSetting("BackgroundBlurDegree", 5D);
            CreateSetting("BackgroundOpacity", 0.3);
            CreateSetting("Volume", 1D);
            CreateSetting("IsDisplayAlbumImageBackground", true);
            CreateSetting("Theme", ApplicationTheme.Dark.ToString());
            CreateSetting("ChanageButtonBehavior", LyricChanageButtonBehavior.LetMeChoose.ToString());

            blurAvailability = GetOsVersion() >= 14393;
            backgroundBlurDegree = blurAvailability ? GetSetting<double>("BackgroundBlurDegree") : 0D;
            backgroundOpacity = GetSetting<double>("BackgroundOpacity");
            volume = GetSetting<double>("Volume");
            isDisplayAlbumImageBackground = GetSetting<bool>("IsDisplayAlbumImageBackground");
            theme = GetSetting<ApplicationTheme>("Theme", (s) => s == ApplicationTheme.Light.ToString() ? ApplicationTheme.Light : ApplicationTheme.Dark);
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

        public void CreateSetting(string key,object value)
        {
            if (!settingsObject.Values.ContainsKey(key))
            {
                settingsObject.Values[key] = value;
            }
        }
        private void SetSetting<T>(ref T Field, T value, object settingValue = null, [CallerMemberName] string propertyName = null)
        {
            if (settingValue is null)
                settingValue = value;
            SetProperty<T>(ref Field, value, propertyName);
            settingsObject.Values[propertyName] = settingValue;
        }
        private ulong GetOsVersion()
        {
            ulong version = Convert.ToUInt64(AnalyticsInfo.VersionInfo.DeviceFamilyVersion);
            //return $"{version >> 48 & 0xFFFF}.{version >> 32 & 0xFFFF}.{version >> 16 & 0xFFFF}.{version & 0xFFFF}";
            return version >> 16 & 0xFFFF;
        }
    }
}
