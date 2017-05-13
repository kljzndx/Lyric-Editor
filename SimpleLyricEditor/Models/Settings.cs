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

        //播放速率
        private double playbackRate;
        public double PlaybackRate { get => playbackRate; set => SetSetting(ref playbackRate, value); }

        //音量
        private double volume;
        public double Volume { get => volume; set => SetSetting(ref volume, value); }

        private double channel;
        public double Channel { get => channel; set => SetSetting(ref channel, value); }

        //默认歌词文件作者名
        private string defaultLyricAuthor;
        public string DefaultLyricAuthor { get => defaultLyricAuthor; set => SetSetting(ref defaultLyricAuthor, value); }

        //页面主题
        private ElementTheme pageTheme;
        public ElementTheme PageTheme { get => pageTheme; set => SetSetting(ref pageTheme, value, value.ToString()); }
        
        //背景图显示
        private bool isDisplayBackGround;
        public bool IsDisplayBackground { get => isDisplayBackGround; set => SetSetting(ref isDisplayBackGround, value); }

        //背景源类型
        private BackgroundSourceTypeEnum backgroundSourceType;
        public BackgroundSourceTypeEnum BackgroundSourceType { get => backgroundSourceType; set => SetSetting(ref backgroundSourceType, value, value.ToString()); }

        //用户指定的图片的路径
        private string localBackgroundImagePath;
        public string LocalBackgroundImagePath { get => localBackgroundImagePath; set => SetSetting(ref localBackgroundImagePath, value); }

        //模糊可用性
        public bool IsBlurUsability { get; } = SystemInfo.BuildVersion >= 14393;

        //背景模糊度
        private double backgroundBlurDegree;
        public double BackgroundBlurDegree { get => backgroundBlurDegree; set => SetSetting(ref backgroundBlurDegree, value); }

        //背景透明度
        private double backgroundOpacity;
        public double BackgroundOpacity { get => backgroundOpacity; set => SetSetting(ref backgroundOpacity, value); }
        
        //预览区域显示
        private bool isDisplayLyricsPreview;
        public bool IsDisplayLyricsPreview { get => isDisplayLyricsPreview; set => SetSetting(ref isDisplayLyricsPreview, value); }

        //单行歌词预览区域字体大小
        private double singleLineLyricPreviewFontSize;
        public double SingleLineLyricPreviewFontSize { get => singleLineLyricPreviewFontSize; set => SetSetting(ref singleLineLyricPreviewFontSize, value); }

        private double scrollLyricsPreviewFontSize;
        public double ScrollLyricsPreviewFontSize { get => scrollLyricsPreviewFontSize; set => SetSetting(ref scrollLyricsPreviewFontSize, value); }

        //预览区域背景透明度
        private double previewBackgroundOpacity;
        public double PreviewBackgroundOpacity { get => previewBackgroundOpacity; set => SetSetting(ref previewBackgroundOpacity, value); }

        private SelectItemAlwaysStaysIn_Enum selectItemAlwaysStaysIn;
        public SelectItemAlwaysStaysIn_Enum SelectItemAlwaysStaysIn { get => selectItemAlwaysStaysIn; set => SetSetting(ref selectItemAlwaysStaysIn, value, value.ToString()); }

        private Settings()
        {
            //旧设置迁移
            RenameSettingKey("PlaySpeed", nameof(PlaybackRate));
            RenameSettingKey("BackgroundImageType", nameof(BackgroundSourceType));
            RenameSettingValue(nameof(BackgroundSourceType), "UserDefined", "LocalImage");
            RenameSettingKey("UserDefinedBackgroundImagePath", nameof(LocalBackgroundImagePath));
            RenameSettingKey("PreviewFontSize", nameof(SingleLineLyricPreviewFontSize));
            
            //初始化设置
            playbackRate = GetSetting(nameof(PlaybackRate), 1D);
            volume = GetSetting(nameof(Volume), 1D);
            channel = GetSetting(nameof(Channel), 0D);
            defaultLyricAuthor = GetSetting(nameof(DefaultLyricAuthor), String.Empty);
            pageTheme = GetSetting(nameof(PageTheme), ElementTheme.Dark.ToString(), s => (ElementTheme)Enum.Parse(typeof(ElementTheme), s));
            isDisplayBackGround = GetSetting(nameof(IsDisplayBackground), true);
            backgroundSourceType = GetSetting(nameof(BackgroundSourceType), BackgroundSourceTypeEnum.AlbumImage.ToString(), s => (BackgroundSourceTypeEnum)Enum.Parse(typeof(BackgroundSourceTypeEnum), s));
            localBackgroundImagePath = GetSetting(nameof(LocalBackgroundImagePath), String.Empty);
            backgroundBlurDegree = IsBlurUsability ? GetSetting(nameof(BackgroundBlurDegree), 5D) : 0D;
            backgroundOpacity = GetSetting(nameof(BackgroundOpacity), 0.3);
            isDisplayLyricsPreview = GetSetting(nameof(isDisplayLyricsPreview), true);
            singleLineLyricPreviewFontSize = GetSetting(nameof(SingleLineLyricPreviewFontSize), SystemInfo.DeviceType == "Windows.Mobile" ? 18D : 24D);
            scrollLyricsPreviewFontSize = GetSetting(nameof(ScrollLyricsPreviewFontSize), 15D);
            previewBackgroundOpacity = GetSetting(nameof(PreviewBackgroundOpacity), 0.3);
            selectItemAlwaysStaysIn = GetSetting(nameof(SelectItemAlwaysStaysIn), SelectItemAlwaysStaysIn_Enum.ViewableArea.ToString(), s => (SelectItemAlwaysStaysIn_Enum)Enum.Parse(typeof(SelectItemAlwaysStaysIn_Enum), s));
        }
        
        public static Settings GetSettingsObject()
        {
            if (obj is null)
                lock (looker)
                    if (obj is null)
                        obj = new Settings();
            return obj;
        }

        public void RenameSettingKey(string oldKey, string newKey)
        {
            if (SettingObject.Values.ContainsKey(oldKey))
            {
                SettingObject.Values[newKey] = SettingObject.Values[oldKey];
                SettingObject.Values.Remove(oldKey);
            }
        }

        public void RenameSettingValue(string key, object oldValue, object newValue)
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
