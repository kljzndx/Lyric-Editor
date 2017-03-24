using LyricsEditor.Auxiliary;
using LyricsEditor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;

namespace LyricsEditor.Information
{
    public static class AppInfo
    {
        private static Setting settings = Setting.GetSettingObject();
        public static string AppName { get; } = CharacterLibrary.Main.GetString("AppName") + " For Win10 UWP";
        public static string AppVersion
        {
            get
            {
                var version = Package.Current.Id.Version;
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
        }
        public static string LanguageCode { get => AppName == "简易歌词编辑器 For Win10 UWP" ? "zh-CN" : "en-US"; }
        public static int BootCount
        {
            get
            {
                settings.CreateSetting("BootCount", 1);
                return (int)settings.SettingsObject.Values["BootCount"];
            }
            set
            {
                settings.SettingsObject.Values["BootCount"] = value;
            }
        }
        public static bool IsReviewsed
        {
            get
            {
                settings.CreateSetting(nameof(IsReviewsed), false);
                return (bool)settings.SettingsObject.Values["IsReviewsed"];
            }
            set => settings.SettingsObject.Values["IsReviewsed"] = value;
        }

        public static string PrintInfo()
        {
            return $"应用版本： {AppVersion}\n当前语言： {LanguageCode}";
        }
    }
}
