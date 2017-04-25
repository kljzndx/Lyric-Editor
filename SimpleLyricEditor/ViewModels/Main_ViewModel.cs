using GalaSoft.MvvmLight;
using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Information;
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
        public event EventHandler LyricsListChanged;

        public Settings Settings = Settings.GetSettingsObject();

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

        public string Version { get; } = AppInfo.Version;

        public Main_ViewModel()
        {
            CoreWindow.GetForCurrentThread().KeyDown += Window_KeyDown;
            LyricFileTools.LyricFileChanged +=
                async (s, e) =>
                {
                    LyricFile = e.NewFile;
                    Lyrics = LyricTools.LrcParse(await LyricFileTools.ReadFileAsync(LyricFile), tags);
                    LyricsListChanged?.Invoke(this, EventArgs.Empty);
                };

            if (Settings.GetSetting("BootCount", 1) >= 10 && !Settings.GetSetting("IsReviewsed", false))
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
            await EmailEx.SendAsync("kljzndx@outlook.com", $"{AppInfo.Name} {AppInfo.Version} {(AppInfo.Name == "简易歌词编辑器" ? "反馈" : "Feedback")}", String.Empty);
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

            LyricsListChanged?.Invoke(this, EventArgs.Empty);
        }

        public void AddLyric()
        {
            var contents = lyricContent.Split('\r');
            foreach (var item in contents)
                lyrics.Add(new LyricItem { Time = thisTime, Content = item.Trim() });

            if (selectedIndex != -1 && contents.Length == 1)
            {
                lyrics.Move(lyrics.Count - 1, selectedIndex + 1);
                SelectedIndex++;
            }

            LyricsListChanged?.Invoke(this, EventArgs.Empty);
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

            LyricsSort();
        }

        public void DelLyric()
        {
            if (App.IsPressShift)
                lyrics.Clear();
            else
                foreach (LyricItem item in selectedItems)
                    lyrics.Remove(item);

            if (!lyrics.Any())
                LyricFile = null;

            LyricsListChanged?.Invoke(this, EventArgs.Empty);
        }

        public async void ChangeTime()
        {
            try
            {
                (selectedItems.SingleOrDefault() as LyricItem).Time = thisTime;
                LyricsListChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (NullReferenceException)
            {
                return;
            }
            catch (InvalidOperationException)
            {
                await MessageBox.ShowAsync(CharacterLibrary.ErrorDialog.GetString("SelectedMultipleItemsError"), CharacterLibrary.ErrorDialog.GetString("Close"));
            }
        }

        public void ChangeContent()
        {
            foreach (LyricItem sltItem in selectedItems)
                sltItem.Content = lyricContent.Split('\r')[0].Trim();
            
            LyricsListChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
        #region Lyric File Operations
        public async void OpenLyricFile()
        {
            await LyricFileTools.OpenFileAsync();
        }

        public async void SaveLyricFile()
        {
            await LyricFileTools.SaveFileAsync(tags, lyrics, LyricFile);
        }

        public async void SaveAsLyricFile()
        {
            await LyricFileTools.SaveFileAsync(tags, lyrics, null);
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
                        DelLyric();
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
