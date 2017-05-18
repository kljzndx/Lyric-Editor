using GalaSoft.MvvmLight;
using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Information;
using Microsoft.Services.Store.Engagement;
using SimpleLyricEditor.EventArgss;
using SimpleLyricEditor.Models;
using SimpleLyricEditor.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;


namespace SimpleLyricEditor.ViewModels
{
    public class Main_ViewModel : ViewModelBase
    {
        public event EventHandler<LyricItemChangeEventAegs> LyricItemChanged;

        public Settings Settings = Settings.GetSettingsObject();

        public StoreServicesCustomEventLogger storeLogger => StoreServicesCustomEventLogger.GetDefault();

        //当前播放进度
        private TimeSpan thisTime;
        public TimeSpan ThisTime { get => thisTime; set => Set(ref thisTime, value); }

        //歌词文件标签
        private LyricTags tags = new LyricTags();
        public LyricTags Tags { get => tags; set => Set(ref tags, value); }

        //歌词列表
        private ObservableCollection<LyricItem> lyrics = new ObservableCollection<LyricItem>();
        public ObservableCollection<LyricItem> Lyrics { get => lyrics; set => Set(ref lyrics, value); }

        //当前是否是多行编辑状态
        private bool isMultilineEditMode = false;
        public bool IsMultilineEditMode { get => isMultilineEditMode; set => Set(ref isMultilineEditMode, value); }
        
        //当前选中项序号
        private int selectedIndex = -1;
        public int SelectedIndex { get => selectedIndex; set => Set(ref selectedIndex, value); }

        //当前选中项目
        private IList<object> selectedItems;
        public IList<object> SelectedItems { get => selectedItems; set => Set(ref selectedItems, value); }

        //歌词输入框的内容
        private string lyricContent = String.Empty;
        public string LyricContent { get => lyricContent; set => Set(ref lyricContent, value); }

        //歌词文件
        public StorageFile LyricFile { get; set; }

        private bool isDisplayScrollLyricsPreview;
        public bool IsDisplayScrollLyricsPreview { get => isDisplayScrollLyricsPreview; set => Set(ref isDisplayScrollLyricsPreview, value); }

        public string Version { get; } = AppInfo.Version;

        public Main_ViewModel()
        {
            CoreWindow.GetForCurrentThread().KeyDown += Window_KeyDown;
            LyricFileTools.LyricFileChanged +=
                async (s, e) =>
                {
                    LyricFile = e.NewFile;
                    Lyrics = LyricTools.LrcParse(await LyricFileTools.ReadFileAsync(LyricFile), tags);
                    LyricItemChanged?.Invoke(this, new LyricItemChangeEventAegs(LyricItemOperationType.Refresh));
                };

            if (Settings.GetSetting("BootCount", 1) % 10 == 0 && !Settings.GetSetting("IsReviewsed", false))
                GetReviews();

            Settings.SettingObject.Values["BootCount"] = (int)Settings.SettingObject.Values["BootCount"] + 1;
        }

        public void InputBoxGotFocus()
        {
            App.IsInputBoxGotFocus = true;
        }

        public void InputBoxLostFocus()
        {
            App.IsInputBoxGotFocus = false;
        }

        public async void GetReviews()
        {
            TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs> headler =
                async (s, e) =>
                  {
                      await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9mx4frgq4rqs"));
                      Settings.SettingObject.Values["IsReviewsed"] = true;
                      storeLogger.Log("评价应用");
                  };

            ContentDialog dialog = new ContentDialog()
            {
                Title = CharacterLibrary.GetReviewsDialog.GetString("Title"),
                Content = CharacterLibrary.GetReviewsDialog.GetString("Content"),
                PrimaryButtonText = CharacterLibrary.GetReviewsDialog.GetString("Good"),
                SecondaryButtonText = CharacterLibrary.GetReviewsDialog.GetString("NotGood")
            };
            dialog.PrimaryButtonClick += headler;
            dialog.SecondaryButtonClick += headler;
            await dialog.ShowAsync();
        }

        public async void Feedback()
        {
            if (!StoreServicesFeedbackLauncher.IsSupported() || !await StoreServicesFeedbackLauncher.GetDefault().LaunchAsync())
                await EmailEx.SendAsync("kljzndx@outlook.com", $"{AppInfo.Name} {AppInfo.Version} {(AppInfo.Name == "简易歌词编辑器" ? "反馈" : "Feedback")}", String.Empty);
            storeLogger.Log("反馈问题和建议");
        }

        public void Deselect()
        {
            SelectedIndex = -1;
        }

        #region Lyrics Operations

        public void LyricsSort()
        {
            for (int i = lyrics.Count; i > 0; i--)
                for (int j = 0; j < i - 1; j++)
                    if (lyrics[j].CompareTo(lyrics[j + 1]) > 0)
                        lyrics.Move(j, j + 1);

            LyricItemChanged?.Invoke(this, new LyricItemChangeEventAegs(LyricItemOperationType.Refresh));
            storeLogger.Log("歌词项排序");
        }

        public void AddLyric()
        {
            var contents = lyricContent.Split('\r');
            bool isPressShift = App.IsPressShift;
            List<LyricItem> addItems = new List<LyricItem>();

            foreach (var item in contents)
            {
                addItems.Add(new LyricItem() { Time = thisTime, Content = item.Trim() });

                if (selectedItems.Any())
                {
                    if (isPressShift)
                    {
                        lyrics.Insert(selectedIndex > 0 ? selectedIndex - 1 : 0, new LyricItem() { Time = ThisTime, Content = item.Trim() });
                        SelectedIndex--;
                    }
                    else
                    {
                        lyrics.Insert(selectedIndex + 1, new LyricItem() { Time = ThisTime, Content = item.Trim() });
                        SelectedIndex++;
                    }
                }
                else
                {
                    if (isPressShift)
                        lyrics.Insert(0, new LyricItem { Time = thisTime, Content = item.Trim() });
                    else
                        lyrics.Add(new LyricItem() { Time = thisTime, Content = item.Trim() });
                }
            }

            if (isPressShift && selectedItems.Any())
                SelectedIndex--;

            lyricContent = String.Empty;

            LyricItemChanged?.Invoke(this, new LyricItemChangeEventAegs(LyricItemOperationType.Add, addItems));
            storeLogger.Log("添加歌词");
        }

        public void CopyLyrics()
        {
            if (!selectedItems.Any())
                return;

            //取出时间最早的歌词
            var firstTime = TimeSpan.MaxValue;
            foreach (Lyric item in selectedItems)
                if (firstTime.CompareTo(item.Time) == 1)
                    firstTime = item.Time;

            bool isBig = thisTime >= firstTime;
            TimeSpan interpolate = isBig ? thisTime - firstTime : firstTime - thisTime;
            foreach (Lyric item in SelectedItems)
                lyrics.Add(new LyricItem { Time = isBig ? item.Time + interpolate : item.Time - interpolate, Content = item.Content.Trim() });

            storeLogger.Log("复制歌词");
            LyricsSort();
        }

        public async void DelLyric()
        {
            if (App.IsPressShift)
            {
                ContentDialog dialog = new ContentDialog()
                {
                    Title = CharacterLibrary.LyricsListClearDialog.GetString("Title"),
                    Content = CharacterLibrary.LyricsListClearDialog.GetString("Content"),
                    PrimaryButtonText = CharacterLibrary.LyricsListClearDialog.GetString("Yes"),
                    SecondaryButtonText = CharacterLibrary.LyricsListClearDialog.GetString("No")
                };
                dialog.PrimaryButtonClick += (s, e) => lyrics.Clear();
                await dialog.ShowAsync();
            }
            else
                foreach (LyricItem item in selectedItems)
                    lyrics.Remove(item);

            if (!lyrics.Any())
                LyricFile = null;
            
            LyricItemChanged?.Invoke(this, new LyricItemChangeEventAegs(LyricItemOperationType.Del));
            storeLogger.Log("删除歌词");
        }

        public async void ChangeTime()
        {
            if (!selectedItems.Any())
                return;

            try
            {
                (selectedItems.SingleOrDefault() as LyricItem).Time = thisTime;
                LyricItemChanged?.Invoke(this, new LyricItemChangeEventAegs(LyricItemOperationType.ChangeTime));
            }
            catch (NullReferenceException)
            {
                return;
            }
            catch (InvalidOperationException)
            {
                await MessageBox.ShowAsync(CharacterLibrary.ErrorDialog.GetString("SelectedMultipleItemsError"), CharacterLibrary.ErrorDialog.GetString("Close"));
            }
            storeLogger.Log("修改歌词时间");
        }

        public void ChangeContent()
        {
            if (!selectedItems.Any())
                return;

            foreach (LyricItem sltItem in selectedItems)
                sltItem.Content = lyricContent.Split('\r')[0].Trim();
            
            LyricItemChanged?.Invoke(this, new LyricItemChangeEventAegs(LyricItemOperationType.ChangeContent));
            storeLogger.Log("修改歌词内容");
        }

        #endregion
        #region Lyric File Operations
        public async void OpenLyricFile()
        {
            await LyricFileTools.OpenFileAsync();
            storeLogger.Log("打开歌词文件");
        }

        public async void SaveLyricFile()
        {
            LyricsSort();
            await LyricFileTools.SaveFileAsync(tags, lyrics, LyricFile);
            storeLogger.Log("保存歌词文件");
        }

        public async void SaveAsLyricFile()
        {
            LyricsSort();
            await LyricFileTools.SaveFileAsync(tags, lyrics, null);
            storeLogger.Log("另存为歌词文件");
        }
        
        public void AssociatedTags(Music music)
        {
            tags.SongName = music.Name;
            tags.Artist = music.Artist;
            tags.Album = music.Album;
        }

        #endregion

        private void Window_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (!App.IsInputBoxGotFocus)
            {
                switch (args.VirtualKey)
                {
                    case VirtualKey.Space:
                        if (selectedItems.Any())
                        {
                            ChangeTime();
                            SelectedIndex = selectedIndex < lyrics.Count - 1 ? selectedIndex + 1 : -1;
                        }
                        else
                            AddLyric();
                        break;
                    case VirtualKey.C:
                        CopyLyrics();
                        break;
                    case VirtualKey.Delete:
                        int sid = selectedIndex;
                        DelLyric();
                        SelectedIndex = sid < lyrics.Count ? sid : lyrics.Count - 1;
                        break;
                    case VirtualKey.S:
                        LyricsSort();
                        break;
                }
            }

            if (App.IsPressCtrl)
            {
                if (args.VirtualKey == VirtualKey.S && App.IsPressShift)
                { 
                    SaveAsLyricFile();
                    return;
                }

                switch (args.VirtualKey)
                {
                    case VirtualKey.L:
                        OpenLyricFile();
                        break;
                    case VirtualKey.S:
                        SaveLyricFile();
                        break;
                }
            }
        }

    }
}
