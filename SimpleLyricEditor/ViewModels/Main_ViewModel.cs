using GalaSoft.MvvmLight;
using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Information;
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
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;


namespace SimpleLyricEditor.ViewModels
{
    public class Main_ViewModel : ViewModelBase
    {
        public event EventHandler<LyricItemChangeEventAegs> LyricItemChanged;

        public Settings Settings = Settings.GetSettingsObject();

        private StorageFile tempFile;
        private ThreadPoolTimer saveTempFile_Timer;

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

        private DateTime adClickDate;
        public DateTime AdClickDate { get => adClickDate; set => Settings.SetSetting(ref adClickDate, value, value.Date.ToString("d")); }

        private bool isDisplayAd;
        public bool IsDisplayAd { get => isDisplayAd; set => Set(ref isDisplayAd, value); }

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

            GetTempFile();
            if (Settings.GetSetting("IsCollapse", false))
                ReadTempFile();
            saveTempFile_Timer = ThreadPoolTimer.CreatePeriodicTimer((t) => SaveTempFile(), TimeSpan.FromSeconds(30));
            Settings.SettingObject.Values["IsCollapse"] = false;

            //Settings.SettingObject.Values.Remove(nameof(AdClickDate));
            adClickDate = Settings.GetSetting(nameof(AdClickDate), new DateTime(2017, 6, 14).ToString("d"), s => DateTime.Parse(s));
            isDisplayAd = adClickDate != DateTime.Now.Date;
        }

        #region Lyrics Operations

        public void LyricsSort()
        {
            for (int i = lyrics.Count; i > 0; i--)
                for (int j = 0; j < i - 1; j++)
                    if (lyrics[j].CompareTo(lyrics[j + 1]) > 0)
                        lyrics.Move(j, j + 1);

            LyricItemChanged?.Invoke(this, new LyricItemChangeEventAegs(LyricItemOperationType.Refresh));
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
        }

        public void CopyLyrics()
        {
            if (!selectedItems.Any())
                return;

            //取出时间最早的歌词
            TimeSpan firstTime = GetEarliestLyricTime();

            bool isBig = thisTime >= firstTime;
            TimeSpan interpolate = isBig ? thisTime - firstTime : firstTime - thisTime;
            foreach (Lyric item in SelectedItems)
                lyrics.Add(new LyricItem { Time = isBig ? item.Time + interpolate : item.Time - interpolate, Content = item.Content.Trim() });

            LyricsSort();
        }

        private TimeSpan GetEarliestLyricTime()
        {
            var firstTime = TimeSpan.MaxValue;
            foreach (Lyric item in selectedItems)
                if (firstTime.CompareTo(item.Time) == 1)
                    firstTime = item.Time;
            return firstTime;
        }

        public void MoveLyrics()
        {
            if (!selectedItems.Any())
                return;

            TimeSpan fiastTime = GetEarliestLyricTime();

            bool isBig = fiastTime >= thisTime;
            TimeSpan interpolate = isBig ? fiastTime - thisTime : thisTime - fiastTime;

            foreach (Lyric item in selectedItems)
                item.Time = isBig ? item.Time - interpolate : item.Time + interpolate;

            if (selectedItems.Count != 1)
                LyricsSort();

            LyricItemChanged?.Invoke(this, new LyricItemChangeEventAegs(LyricItemOperationType.ChangeTime));
        }

        public async void DelLyrics()
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
        }

        public void ModifyLyricsContent()
        {
            if (!selectedItems.Any())
                return;

            foreach (LyricItem sltItem in selectedItems)
                sltItem.Content = lyricContent.Trim();

            LyricItemChanged?.Invoke(this, new LyricItemChangeEventAegs(LyricItemOperationType.ChangeContent));
        }

        #endregion
        #region Lyric File Operations
        public async void OpenLyricFile()
        {
            await LyricFileTools.OpenFileAsync();
        }

        public async void SaveLyricFile()
        {
            LyricsSort();
            await LyricFileTools.SaveFileAsync(tags, lyrics, LyricFile);
        }

        public async void SaveAsLyricFile()
        {
            LyricsSort();
            await LyricFileTools.SaveFileAsync(tags, lyrics, null);
        }

        public void AssociatedTags(Music music)
        {
            tags.SongName = music.Name;
            tags.Artist = music.Artist;
            tags.Album = music.Album;
        }

        #endregion
        #region Lyric Temp File Operations

        private async void GetTempFile()
        {
            if (await ApplicationData.Current.TemporaryFolder.TryGetItemAsync("temp.lrc") is IStorageItem file)
                tempFile = file as StorageFile;
            else
                tempFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync("temp.lrc");
        }

        private async void SaveTempFile()
        {
            if (lyrics.Any())
            {
                await LyricFileTools.SaveFileAsync(tags, lyrics, tempFile);

                if (LyricFile is StorageFile && Settings.IsAutoSave)
                    await LyricFileTools.SaveFileAsync(tags, lyrics, LyricFile);
            }
        }

        private async void ReadTempFile()
        {
            string content = await LyricFileTools.ReadFileAsync(tempFile);
            Lyrics = LyricTools.LrcParse(content, tags);
        }

        #endregion
        #region ohter

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

        public void HideAd()
        {
            AdClickDate = DateTime.Now.Date;
            IsDisplayAd = false;
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
                            MoveLyrics();
                            SelectedIndex = selectedIndex < lyrics.Count - 1 ? selectedIndex + 1 : -1;
                        }
                        else
                            AddLyric();
                        break;
                    case VirtualKey.C:
                        CopyLyrics();
                        break;
                    case VirtualKey.M:
                        ModifyLyricsContent();
                        break;
                    case VirtualKey.Delete:
                        int sid = selectedIndex;
                        DelLyrics();
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
