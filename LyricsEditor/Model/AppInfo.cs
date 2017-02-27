using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;

namespace LyricsEditor.Model
{
    public static class AppInfo
    {
        private static Setting settings = Setting.GetSettingObject();
        public static string AppVersion
        {
            get
            {
                var version = Package.Current.Id.Version;
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
        }
        public static string LanguageCode
        {
            get
            {
                string appName = ResourceLoader.GetForCurrentView("Main").GetString("AppName");
                if (appName == "简易歌词编辑器")
                    return "zh-CN";
                else
                    return "en-US";
            }
        }
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


        public static string PrintInfo()
        {
            return $"应用版本： {AppVersion}\n当前语言： {LanguageCode}";
        }
    }
}
