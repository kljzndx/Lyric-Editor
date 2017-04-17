using GalaSoft.MvvmLight;
using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Information;
using SimpleLyricEditor.Models;
using SimpleLyricEditor.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ObservableCollection<Lyric> lyrics = new ObservableCollection<Lyric>();
        public ObservableCollection<Lyric> Lyrics { get => lyrics; set => Set(ref lyrics, value); }

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

        //指示输入框是否已获得焦点
        public bool IsInputBoxGotFocus { get; private set; }

        public string Version { get; } = AppInfo.Version;

        public Main_ViewModel()
        {
            CoreWindow.GetForCurrentThread().KeyDown += Window_KeyDown;
            LyricFileTools.LyricFileChanged +=
                async (s, e) =>
                {
                    Lyrics = LyricTools.LrcParse(await LyricFileTools.ReadFileAsync(e.File), tags);
                    LyricsListChanged?.Invoke(this, EventArgs.Empty);
                };

            if (Settings.GetSetting("BootCount", 1) >= 10 && !Settings.GetSetting("IsReviewsed", false))
                GetReviews();

            Settings.SettingObject.Values["BootCount"] = (int)Settings.SettingObject.Values["BootCount"] + 1;
        }

        public void InputBoxGotFocus()
        {
            IsInputBoxGotFocus = true;
        }

        public void InputBoxLostFocus()
        {
            IsInputBoxGotFocus = false;
        }

        public async void GetReviews()
        {
            TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs> headler =
                async (s, e) =>
                  {
                      await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9mx4frgq4rqs"));
                      Settings.SettingObject.Values["IsReviewsed"] = true;
                  };

            ContentDialog dialog = new ContentDialog();
            dialog.Title = CharacterLibrary.GetReviewsDialog.GetString("Title");
            dialog.Content = CharacterLibrary.GetReviewsDialog.GetString("Content");
            dialog.PrimaryButtonText = CharacterLibrary.GetReviewsDialog.GetString("Good");
            dialog.SecondaryButtonText = CharacterLibrary.GetReviewsDialog.GetString("NotGood");
            dialog.PrimaryButtonClick += headler;
            dialog.SecondaryButtonClick += headler;
            await dialog.ShowAsync();
        }

        public async void Feedback()
        {
            await EmailEx.SendAsync("kljzndx@outlook.com", $"{AppInfo.Name} {AppInfo.Version} {(AppInfo.Name == "简易歌词编辑器" ? "反馈" : "Feedback")}", String.Empty);
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
                lyrics.Add(new Lyric { Time = thisTime, Content = item.Trim() });

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
                lyrics.Add(new Lyric { Time = isBig ? item.Time + interpolate : item.Time - interpolate, Content = item.Content.Trim() });

            LyricsSort();
        }

        public void DelLyric()
        {
            if (App.IsPressShift)
                lyrics.Clear();
            else
                foreach (Lyric item in selectedItems)
                    lyrics.Remove(item);

            if (!lyrics.Any())
                LyricFile = null;

            LyricsListChanged?.Invoke(this, EventArgs.Empty);
        }

        public async void ChangeTime()
        {
            try
            {
                (selectedItems.SingleOrDefault() as Lyric).Time = thisTime;
                LyricsListChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (NullReferenceException)
            {
                return;
            }
            catch (InvalidOperationException)
            {
                await MessageBox.ShowAsync(CharacterLibrary.ErrorDialog.GetString("ChangeTimeError"), CharacterLibrary.ErrorDialog.GetString("Close"));
            }
        }

        public void ChangeContent()
        {
            foreach (Lyric sltItem in selectedItems)
                sltItem.Content = lyricContent.Split('\r')[0].Trim();

            LyricsListChanged?.Invoke(this, EventArgs.Empty);
        }
#endregion
        #region Lyric File Operations
        public async void OpenLyricFile()
        {
            LyricFile = await LyricFileTools.OpenFileAsync();
        }

        public async void SaveLyricFile()
        {
            LyricFile = await LyricFileTools.SaveFileAsync(tags, lyrics, LyricFile);
        }

        public async void SaveAsLyricFile()
        {
            LyricFile = await LyricFileTools.SaveFileAsync(tags, lyrics, null);
        }
#endregion

        private void Window_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Space && !selectedItems.Any() && !IsInputBoxGotFocus)
                AddLyric();

            if (args.VirtualKey == VirtualKey.C && !IsInputBoxGotFocus)
                CopyLyrics();

            if (args.VirtualKey == VirtualKey.Space && selectedItems.Any() && !IsInputBoxGotFocus)
            {
                ChangeTime();
                SelectedIndex = selectedIndex < lyrics.Count - 1 ? selectedIndex + 1 : -1;
            }

            if (args.VirtualKey == VirtualKey.Delete && !IsInputBoxGotFocus)
                DelLyric();

            if (args.VirtualKey == VirtualKey.S && !IsInputBoxGotFocus)
                LyricsSort();

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
                    default:
                        break;
                }
            }
        }

    }
}
