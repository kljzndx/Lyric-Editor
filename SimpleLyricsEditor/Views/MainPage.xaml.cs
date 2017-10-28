using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using HappyStudio.UwpToolsLibrary.Auxiliarys;
using SimpleLyricsEditor.Control;
using SimpleLyricsEditor.Core;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.Events;
using SimpleLyricsEditor.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SimpleLyricsEditor.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly Settings _settings = Settings.Current;

        private BitmapSource _backgroundImageSource;
        private bool _isPressCtrl;
        private bool _isPressShift;
        private bool _lyricsListGotFocus;
        private MainViewModel _viewModel;

        public MainPage()
        {
            InitializeComponent();
            _viewModel = DataContext as MainViewModel;
            _viewModel.SelectedItems = Lyrics_ListView.SelectedItems;
            _viewModel.UndoOperations.CollectionChanged += UndoOperations_CollectionChanged;
            _settings.PropertyChanged += Settings_PropertyChanged;

            GlobalKeyNotifier.KeyDown += WindowKeyDown;
            GlobalKeyNotifier.KeyUp += WindowKeyUp;

            ImageFileNotifier.FileChanged += ImageFileChanged;
        }

        private void GoToLyricTime(Lyric lyric)
        {
            if (lyric.Time <= Player.Duration)
                Player.SetPosition(lyric.Time);
        }

        private async Task GetBackgroundImage()
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.TryGetItemAsync("Background.img") as StorageFile;
            if (file != null)
            {
                BitmapImage image = new BitmapImage();
                image.SetSource(await file.OpenAsync(FileAccessMode.Read));
                _backgroundImageSource = image;
                BlurBackground.Source = image;
            }
            else if (!String.IsNullOrEmpty(_settings.BackgroundImagePath))
            {
                _settings.BackgroundImagePath = String.Empty;
            }
        }

        private async Task ExtractionOldVersionBackgroundImageFile()
        {
            if(!_settings.SettingObject.Values.ContainsKey("LocalBackgroundImagePath"))
                return;

            try
            {
                StorageFile oldfile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync("BackgroundImage");
                StorageApplicationPermissions.FutureAccessList.Remove("BackgroundImage");

                await oldfile.CopyAsync(ApplicationData.Current.LocalFolder, "Background.img", NameCollisionOption.ReplaceExisting);

                _settings.RenameSettingKey("LocalBackgroundImagePath", nameof(_settings.BackgroundImagePath));
            }
            catch (FileNotFoundException)
            {
                _settings.SettingObject.Values.Remove("LocalBackgroundImagePath");
                StorageApplicationPermissions.FutureAccessList.Remove("BackgroundImage");
                await MessageBox.ShowAsync(
                    CharacterLibrary.ErrorInfo.GetString("NotFoundFile"),
                    CharacterLibrary.ErrorInfo.GetString("NotFoundBackgroundImageFile"),
                    CharacterLibrary.MessageBox.GetString("Close")
                );
            }
        }

        private void WindowKeyDown(object sender, GlobalKeyEventArgs e)
        {
            _isPressCtrl = e.IsPressCtrl;
            _isPressShift = e.IsPressShift;

            if (e.IsPressCtrl)
                switch (e.Key)
                {
                    case VirtualKey.Z:
                        if (_viewModel.CanUndo)
                            _viewModel.Undo(1);
                        break;
                    case VirtualKey.Y:
                        if (_viewModel.CanRedo)
                            _viewModel.Redo(1);
                        break;
                }

            if (e.IsPressShift)
                AddLyrics_Button_Transform.Rotation = 0;
            
            if (!e.IsInputing)
                switch (e.Key)
                {
                    case VirtualKey.I:
                        LyricsContent_TextBox.Focus(FocusState.Pointer);
                        break;
                    case VirtualKey.Space:
                        Focus(FocusState.Pointer);

                        if (Lyrics_ListView.SelectedItems.Any())
                            _viewModel.Move(Player.Position);
                        else
                            _viewModel.Add(-1, Player.Position, _isPressShift);
                        break;
                    case VirtualKey.C:
                        _viewModel.Copy(Player.Position);
                        break;
                    case VirtualKey.Delete:
                        _viewModel.Remove();
                        break;
                    case VirtualKey.M:
                        _viewModel.Modify();
                        break;
                    case VirtualKey.S:
                        _viewModel.Sort(_viewModel.LyricItems);
                        break;
                    case VirtualKey.G:
                        if (Lyrics_ListView.SelectedItem is Lyric lyric)
                            GoToLyricTime(lyric);
                        break;
                    case VirtualKey.Up:
                        Focus(FocusState.Pointer);
                        if (!_lyricsListGotFocus)
                            Lyrics_ListView.SelectedIndex =
                                Lyrics_ListView.SelectedIndex > -1
                                    ? Lyrics_ListView.SelectedIndex - 1
                                    : Lyrics_ListView.Items.Count - 1;
                        break;
                    case VirtualKey.Down:
                        Focus(FocusState.Pointer);
                        if (!_lyricsListGotFocus)
                            Lyrics_ListView.SelectedIndex =
                                Lyrics_ListView.SelectedIndex < Lyrics_ListView.Items.Count - 1
                                    ? Lyrics_ListView.SelectedIndex + 1
                                    : -1;
                        break;
                }

            switch (e.Key)
            {
                case VirtualKey.Escape:
                    this.Focus(FocusState.Pointer);
                    break;
            }
        }

        private void WindowKeyUp(object sender, GlobalKeyEventArgs e)
        {
            _isPressCtrl = e.IsPressCtrl;
            _isPressShift = e.IsPressShift;

            if (!e.IsPressShift)
                AddLyrics_Button_Transform.Rotation = 180;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // 提取V2系列版本的背景图
            await ExtractionOldVersionBackgroundImageFile();
            await GetBackgroundImage();

            if (_settings.MultilineEditModeEnabled)
                Lyrics_ListView.SelectionMode = ListViewSelectionMode.Multiple;
        }

        private void UndoOperations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SinglePreview.Reposition(Player.Position);
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(_settings.IsFollowSongAlbumCover)))
                if (_settings.IsFollowSongAlbumCover)
                {
                    if (!Player.Source.Equals(Music.Empty))
                        BlurBackground.Source = Player.Source.AlbumImage;
                }
                else if (BlurBackground.Source == Player.Source.AlbumImage)
                {
                    BlurBackground.Source = _backgroundImageSource;
                }
        }

        private void ImageFileChanged(object sender, ImageFileChangeEventArgs e)
        {
            _backgroundImageSource = e.Source;
            BlurBackground.Source = _backgroundImageSource;
        }

        #region Player

        private void Player_Playing(AudioPlayer sender, EventArgs args)
        {
            AdsVisibilityNotifier.HideAds();
        }

        private void Player_Paused(AudioPlayer sender, EventArgs args)
        {
            AdsVisibilityNotifier.DisplayAds();
        }

        private void Player_SourceChanged(AudioPlayer sender, MusicChangeEventArgs args)
        {
            if (_settings.IsFollowSongAlbumCover)
                BlurBackground.Source = args.Source.AlbumImage;

            SinglePreview.Reposition(Player.Position);
        }

        private void Player_PositionChanged(AudioPlayer sender, PositionChangeEventArgs args)
        {
            if (args.IsUserChange)
                SinglePreview.Reposition(args.Position);
            else
                SinglePreview.RefreshLyric(args.Position);
        }

        #endregion

        #region Lyrics content input box

        private void LyricsContent_TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            App.IsInputing = true;
        }

        private void LyricsContent_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            App.IsInputing = false;
        }

        private void LyricsContent_TextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && sender is TextBox tb)
                if (_isPressCtrl)
                {
                    _viewModel.LyricContent = tb.Text;
                    _viewModel.Add(Lyrics_ListView.SelectedIndex, Player.Position, _isPressShift);
                }
                else if (_isPressShift || !Lyrics_ListView.SelectedItems.Any())
                {
                    int selectId = tb.SelectionStart;
                    tb.Text = tb.Text.Remove(selectId, tb.SelectionLength).Insert(selectId, " " + Environment.NewLine);
                    tb.SelectionStart = selectId + 2;
                }
                else if (Lyrics_ListView.SelectedItems.Any())
                {
                    _viewModel.LyricContent = tb.Text;
                    _viewModel.Modify();
                    Lyrics_ListView.SelectedIndex =
                        Lyrics_ListView.SelectedIndex < Lyrics_ListView.Items.Count - 1
                            ? Lyrics_ListView.SelectedIndex + 1
                            : -1;
                }
        }

        #endregion

        #region Lyrics edit tools

        #region Select tools

        private void Select_Reverse_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (ListViewItem item in Lyrics_ListView.ItemsPanelRoot.Children.Cast<ListViewItem>().ToList())
                item.IsSelected = !item.IsSelected;
        }

        private void Select_BeforeItem_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var items = Lyrics_ListView.ItemsPanelRoot.Children.Cast<ListViewItem>().ToList();

            for (var i = Lyrics_ListView.SelectedIndex; i >= 0; i--)
                items[i].IsSelected = true;
        }

        private void Select_AfterItem_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var items = Lyrics_ListView.ItemsPanelRoot.Children.Cast<ListViewItem>().ToList();

            for (var i = Lyrics_ListView.SelectedIndex; i < Lyrics_ListView.Items.Count - 1; i++)
                items[i].IsSelected = true;
        }

        private void Select_Paragraph_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            int sltId = Lyrics_ListView.SelectedIndex;
            var listItems = Lyrics_ListView.ItemsPanelRoot.Children.Cast<ListViewItem>().ToList();
            var sourceItems = Lyrics_ListView.Items.Cast<Lyric>().ToList();

            for (int i = sltId - 1; i >= 0; i--)
                if (!String.IsNullOrEmpty(sourceItems[i].Content))
                    listItems[i].IsSelected = true;
                else
                    break;

            for (int i = sltId; i < sourceItems.Count - 1; i++)
                if (!String.IsNullOrEmpty(sourceItems[i].Content))
                    listItems[i].IsSelected = true;
                else
                    break;
        }

        #endregion

        private void MultilineEditMode_Button_Click(object sender, RoutedEventArgs e)
        {
            Lyrics_ListView.SelectionMode = ListViewSelectionMode.Multiple;
            _settings.MultilineEditModeEnabled = true;
        }

        private void ExitMultilineEditMode_Button_Click(object sender, RoutedEventArgs e)
        {
            Lyrics_ListView.SelectionMode = ListViewSelectionMode.Single;
            _settings.MultilineEditModeEnabled = false;
        }

        private void AddLyrics_Button_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Add(Lyrics_ListView.SelectedIndex, Player.Position, _isPressShift);
        }

        private void CopyLyrics_Button_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Copy(Player.Position);
        }

        private void RemoveLyrics_Button_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Remove();
        }

        private void MoveTime_Button_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Move(Player.Position);
        }

        private void ModifyContent_Button_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Modify();
        }

        private void LyricsSort_Button_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Sort(_viewModel.LyricItems);
        }

        #endregion

        #region List view

        private void Lyrics_ListView_GotFocus(object sender, RoutedEventArgs e)
        {
            _lyricsListGotFocus = true;
        }

        private void Lyrics_ListView_LostFocus(object sender, RoutedEventArgs e)
        {
            _lyricsListGotFocus = false;
        }

        private void Lyrics_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LyricsContent_TextBox.Text = Lyrics_ListView.SelectedIndex > -1
                ? (Lyrics_ListView.Items[Lyrics_ListView.SelectedIndex] as Lyric).Content
                : String.Empty;

            if (e.AddedItems.FirstOrDefault() is Lyric lyric)
                Lyrics_ListView.ScrollIntoView(lyric);
        }

        private void LyricTime_Button_Click(object sender, RoutedEventArgs e)
        {
            Lyric lyric = (sender as Button).DataContext as Lyric;

            Lyrics_ListView.SelectedItem = lyric;
            GoToLyricTime(lyric);
        }

        #endregion
    }
}