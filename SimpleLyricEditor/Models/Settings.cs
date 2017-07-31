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
using SimpleLyricEditor.Attributes;
using System.Reflection;

namespace SimpleLyricEditor.Models
{
    public class Settings : ObservableObject
    {
        private static readonly object looker = new Object();
        private static Settings obj;
        public readonly ApplicationDataContainer SettingObject = ApplicationData.Current.LocalSettings;

        //播放速率
        [SettingFieldByNormal(nameof(PlaybackRate), 1D)]
        private double playbackRate;
        public double PlaybackRate { get => playbackRate; set => SetSetting(ref playbackRate, value); }

        //音量
        [SettingFieldByNormal(nameof(Volume), 1D)]
        private double volume;
        public double Volume { get => volume; set => SetSetting(ref volume, value); }

        [SettingFieldByNormal(nameof(Channel), 0D)]
        private double channel;
        public double Channel { get => channel; set => SetSetting(ref channel, value); }

        //默认歌词文件作者名
        [SettingFieldByNormal(nameof(DefaultLyricAuthor), "快乐工作室")]
        private string defaultLyricAuthor;
        public string DefaultLyricAuthor { get => defaultLyricAuthor; set => SetSetting(ref defaultLyricAuthor, value); }

        //页面主题
        [SettingFieldByEnum(nameof(PageTheme), "Dark", typeof(ElementTheme))]
        private ElementTheme pageTheme;
        public ElementTheme PageTheme { get => pageTheme; set => SetSetting(ref pageTheme, value, value.ToString()); }

        //背景图显示
        [SettingFieldByNormal(nameof(isDisplayBackGround), true)]
        private bool isDisplayBackGround;
        public bool IsDisplayBackground { get => isDisplayBackGround; set => SetSetting(ref isDisplayBackGround, value); }

        //背景源类型
        [SettingFieldByEnum(nameof(BackgroundSourceType), "AlbumImage", typeof(BackgroundSourceTypeEnum))]
        private BackgroundSourceTypeEnum backgroundSourceType;
        public BackgroundSourceTypeEnum BackgroundSourceType { get => backgroundSourceType; set => SetSetting(ref backgroundSourceType, value, value.ToString()); }

        //用户指定的图片的路径
        [SettingFieldByNormal(nameof(LocalBackgroundImagePath), "")]
        private string localBackgroundImagePath;
        public string LocalBackgroundImagePath { get => localBackgroundImagePath; set => SetSetting(ref localBackgroundImagePath, value); }

        //模糊可用性
        public bool IsBlurUsability => SystemInfo.BuildVersion >= 14393;

        //背景模糊度
        [SettingFieldByNormal(nameof(BackgroundBlurDegree), 5D)]
        private double backgroundBlurDegree;
        public double BackgroundBlurDegree
        {
            get
            {
                if (IsBlurUsability)
                    return backgroundBlurDegree;
                else
                    return 0D;
            }
            set => SetSetting(ref backgroundBlurDegree, value);
        }

        //毛玻璃可用性
        public bool IsFrostedGlassUsability => SystemInfo.BuildVersion >= 15063;

        //毛玻璃不透明度
        [SettingFieldByNormal(nameof(FrostedGlassOpacity), 0.7)]
        private double frostedGlassOpacity;
        public double FrostedGlassOpacity
        {
            get
            {
                if (IsFrostedGlassUsability)
                    return frostedGlassOpacity;
                else
                    return 0D;
            }
            set => SetSetting(ref frostedGlassOpacity, value);
        }

        //背景不透明度
        [SettingFieldByNormal(nameof(BackgroundOpacity), 0.3)]
        private double backgroundOpacity;
        public double BackgroundOpacity { get => backgroundOpacity; set => SetSetting(ref backgroundOpacity, value); }

        //预览区域显示
        [SettingFieldByNormal(nameof(IsDisplayLyricsPreview), true)]
        private bool isDisplayLyricsPreview;
        public bool IsDisplayLyricsPreview { get => isDisplayLyricsPreview; set => SetSetting(ref isDisplayLyricsPreview, value); }

        //单行歌词预览区域字体大小
        [SettingFieldByNormal(nameof(SingleLineLyricPreviewFontSize), 20D)]
        private double singleLineLyricPreviewFontSize;
        public double SingleLineLyricPreviewFontSize { get => singleLineLyricPreviewFontSize; set => SetSetting(ref singleLineLyricPreviewFontSize, value); }

        [SettingFieldByNormal(nameof(ScrollLyricsPreviewFontSize), 15D)]
        private double scrollLyricsPreviewFontSize;
        public double ScrollLyricsPreviewFontSize { get => scrollLyricsPreviewFontSize; set => SetSetting(ref scrollLyricsPreviewFontSize, value); }

        //预览区域背景透明度
        [SettingFieldByNormal(nameof(PreviewBackgroundOpacity), 0.3)]
        private double previewBackgroundOpacity;
        public double PreviewBackgroundOpacity { get => previewBackgroundOpacity; set => SetSetting(ref previewBackgroundOpacity, value); }

        [SettingFieldByEnum(nameof(SelectItemAlwaysStaysIn), "ViewableArea", typeof(SelectItemAlwaysStaysIn_Enum))]
        private SelectItemAlwaysStaysIn_Enum selectItemAlwaysStaysIn;
        public SelectItemAlwaysStaysIn_Enum SelectItemAlwaysStaysIn { get => selectItemAlwaysStaysIn; set => SetSetting(ref selectItemAlwaysStaysIn, value, value.ToString()); }

        [SettingFieldByNormal(nameof(IsAutoSave), true)]
        private bool _isAutoSave;
        public bool IsAutoSave
        {
            get => _isAutoSave;
            set => SetSetting(ref _isAutoSave, value);
        }
        
        private Settings()
        {
            //旧设置迁移
            RenameSettingKey("PlaySpeed", nameof(PlaybackRate));
            RenameSettingKey("BackgroundImageType", nameof(BackgroundSourceType));
            RenameSettingValue(nameof(BackgroundSourceType), "UserDefined", "LocalImage");
            RenameSettingKey("UserDefinedBackgroundImagePath", nameof(LocalBackgroundImagePath));
            RenameSettingKey("PreviewFontSize", nameof(SingleLineLyricPreviewFontSize));
            RenameSettingValue(nameof(DefaultLyricAuthor), "", "快乐工作室");

            Serialization();
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

        private void Serialization()
        {
            var currentObjctTypeInfo = this.GetType().GetTypeInfo();
            var fields = currentObjctTypeInfo.DeclaredFields;
            foreach (FieldInfo item in fields)
            {
                SettingFieldAttributeBase settingInfo = item.GetCustomAttribute<SettingFieldAttributeBase>();

                if (settingInfo is null || item.IsInitOnly || item.IsLiteral)
                    continue;

                if (settingInfo.Convert is null)
                    item.SetValue(this, GetSetting(settingInfo.SettingName, settingInfo.DefaultValue));
                else
                    item.SetValue(this, GetSetting(settingInfo.SettingName, settingInfo.DefaultValue.ToString(), settingInfo.Convert));
            }
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
