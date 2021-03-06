﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
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

        private IRandomAccessStream _backgroundImageData;
        private bool _isPressCtrl;
        private bool _isPressShift;
        private bool _lyricsListGotFocus;
        private MainViewModel _viewModel;
        private Lyric _backLyric;
        private readonly ApplicationView _currentView;

        private Action _inputBoxSubmitAction;

        public MainPage()
        {
            InitializeComponent();

            _currentView = ApplicationView.GetForCurrentView();
            ButtonsInMultilineEditMode_StackPanel.Visibility = Visibility.Collapsed;

            _viewModel = DataContext as MainViewModel;
            _viewModel.LyricItems.CollectionChanged += LyricItems_CollectionChanged;
            _viewModel.SelectedItems = Lyrics_ListView.SelectedItems;
            _viewModel.UndoOperations.CollectionChanged += UndoOperations_CollectionChanged;
            _settings.PropertyChanged += Settings_PropertyChanged;

            GlobalKeyNotifier.KeyDown += WindowKeyDown;
            GlobalKeyNotifier.KeyUp += WindowKeyUp;

            MusicFileNotifier.FileChangeRequested += MusicFileChangeRequested;
            ImageFileNotifier.FileChanged += ImageFileChanged;

            if (!ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationViewMode") ||
                !ApiInformation.IsEnumNamedValuePresent(typeof(ApplicationViewMode).FullName, "CompactOverlay") ||
                !_currentView.IsViewModeSupported(ApplicationViewMode.CompactOverlay))
            {
                MiniMode_StackPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void AddLyrics()
        {
            _viewModel.Add(Lyrics_ListView.SelectedIndex, Player.Position, LyricsContent_TextBox.Text, _isPressShift);
            LyricsContent_TextBox.Text = String.Empty;
        }

        private void ModifyLyrics()
        {
            _viewModel.Modify(LyricsContent_TextBox.Text);

            Lyrics_ListView.SelectedIndex =
                Lyrics_ListView.SelectedIndex < Lyrics_ListView.Items.Count - 1
                ? Lyrics_ListView.SelectedIndex + 1
                : -1;
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
                var data = await file.OpenAsync(FileAccessMode.Read);
                _backgroundImageData = data;
                BlurBackground.Source = data;
            }
            else if (!String.IsNullOrEmpty(_settings.BackgroundImagePath))
            {
                _settings.BackgroundImagePath = String.Empty;
            }
        }

        private async Task ExtractionOldVersionBackgroundImageFile()
        {
            if(!_settings.SettingContainer.Values.ContainsKey("LocalBackgroundImagePath"))
                return;

            if (String.IsNullOrEmpty(_settings.SettingContainer.Values["LocalBackgroundImagePath"].ToString()))
            {
                _settings.SettingContainer.Values.Remove("LocalBackgroundImagePath");
                return;
            }

            try
            {
                StorageFile oldfile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync("BackgroundImage");
                StorageApplicationPermissions.FutureAccessList.Remove("BackgroundImage");

                await oldfile.CopyAsync(ApplicationData.Current.LocalFolder, "Background.img", NameCollisionOption.ReplaceExisting);

                _settings.RenameSettingKey("LocalBackgroundImagePath", nameof(_settings.BackgroundImagePath));
            }
            catch (FileNotFoundException)
            {
                _settings.SettingContainer.Values.Remove("LocalBackgroundImagePath");
                StorageApplicationPermissions.FutureAccessList.Remove("BackgroundImage");
                await MessageBox.ShowAsync(
                    CharacterLibrary.ErrorInfo.GetString("NotFoundFile"),
                    CharacterLibrary.ErrorInfo.GetString("NotFoundBackgroundImageFile"),
                    CharacterLibrary.MessageBox.GetString("Close")
                );
            }
        }

        private async void WindowKeyDown(object sender, GlobalKeyEventArgs e)
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
            
            if (!e.IsInputing)
                switch (e.Key)
                {
                    case VirtualKey.I:
                        LyricsContent_TextBox.Focus(FocusState.Keyboard);
                        break;
                    case VirtualKey.Space:
                        Focus(FocusState.Pointer);

                        if (Lyrics_ListView.SelectedItems.Any())
                        {
                            _viewModel.Move(Player.Position);

                            Lyrics_ListView.SelectedIndex =
                                Lyrics_ListView.SelectedIndex < Lyrics_ListView.Items.Count - 1
                                    ? Lyrics_ListView.SelectedIndex + 1
                                    : -1;
                        }
                        else
                            AddLyrics();
                        break;
                    case VirtualKey.C:
                        _viewModel.Copy(Player.Position);
                        break;
                    case VirtualKey.Delete:
                        int selectedId = Lyrics_ListView.SelectedIndex;

                        _viewModel.Remove();

                        if (selectedId < Lyrics_ListView.Items.Count)
                            Lyrics_ListView.SelectedIndex = selectedId;

                        break;
                    case VirtualKey.M:
                        if (Lyrics_ListView.SelectedItem is Lyric)
                        {
                            InputSubmitOperations_ComboBox.SelectedIndex = 1;
                            LyricsContent_TextBox.Focus(FocusState.Keyboard);
                        }
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
                case VirtualKey.I:
                    if (_isPressCtrl)
                        await LyricsFileInfo_ContentDialog.ShowAsync();
                    break;
            }
        }

        private void WindowKeyUp(object sender, GlobalKeyEventArgs e)
        {
            _isPressCtrl = e.IsPressCtrl;
            _isPressShift = e.IsPressShift;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // 提取V2系列版本的背景图
            await ExtractionOldVersionBackgroundImageFile();
            await GetBackgroundImage();
        }

        private void LyricItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SinglePreview.Reposition(Player.Position);
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
                        BlurBackground.Source = Player.Source.AlbumImageData;
                }
                else if (BlurBackground.Source == Player.Source.AlbumImageData)
                {
                    BlurBackground.Source = _backgroundImageData;
                }
        }

        #region Notifier

        
        private async void MusicFileChangeRequested(object sender, FileChangeEventArgs e)
        {
            Player.SetSource(await Music.Parse(e.File));
        }

        private void ImageFileChanged(object sender, ImageFileChangeEventArgs e)
        {
            _backgroundImageData = e.Data;
            BlurBackground.Source = _backgroundImageData;
        }


        #endregion

        #region Player

        private void Player_Playing(AudioPlayer sender, EventArgs args)
        {
            AdsVisibilityNotifier.HideAdsRequest();
        }

        private void Player_Paused(AudioPlayer sender, EventArgs args)
        {
            AdsVisibilityNotifier.DisplayAdsRequest();
        }

        private void Player_SourceChanged(AudioPlayer sender, MusicChangeEventArgs args)
        {
            if (_settings.IsFollowSongAlbumCover)
                BlurBackground.Source = args.Source.AlbumImageData;

            SinglePreview.Reposition(Player.Position);
            MultilinePreview.Reposition(Player.Position);

            #region Replace Lyrics Tags

            _viewModel.LyricsTags[0].TagValue = args.Source.Name;
            _viewModel.LyricsTags[1].TagValue = args.Source.Artist;
            _viewModel.LyricsTags[2].TagValue = args.Source.Album;

            #endregion
        }

        private void Player_PositionChanged(AudioPlayer sender, PositionChangeEventArgs args)
        {
            if (args.IsUserChange)
            {
                SinglePreview.Reposition(args.Position);
                MultilinePreview.Reposition(args.Position);
            }
            else
            {
                SinglePreview.RefreshLyric(args.Position);
                MultilinePreview.RefreshLyric(args.Position);
            }
        }

        #endregion

        private void LyricsOperations_Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.OriginalSource.Equals(sender))
                Lyrics_ListView.SelectedItem = null;
        }

        #region Input submit box

        private void LyricsOperations_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.FirstOrDefault() is ComboBoxItem cbi)
                if (cbi.Equals(AddLyrics_ComboBoxItem))
                    _inputBoxSubmitAction = AddLyrics;
                else
                    _inputBoxSubmitAction = ModifyLyrics;
        }

        private void LyricsContent_TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            App.IsInputing = true;
        }

        private void LyricsContent_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            App.IsInputing = false;
        }

        private void LyricsContent_TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && sender is TextBox tb)
            {
                if (_isPressCtrl)
                {
                    _inputBoxSubmitAction.Invoke();
                }
                else if (!String.IsNullOrWhiteSpace(tb.Text))
                {
                    int selectIndex = tb.SelectionStart;

                    tb.Text = tb.Text.Remove(selectIndex, tb.SelectionLength).Insert(selectIndex, ' ' + Environment.NewLine);
                    tb.SelectionStart = selectIndex + 2;
                }
                else
                    this.Focus(FocusState.Pointer);
            }
        }

        private void Submit_Button_Click(object sender, RoutedEventArgs e)
        {
            _inputBoxSubmitAction.Invoke();
        }

        #endregion

        #region Lyrics edit tools

        private void MoveTime_Button_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Move(Player.Position);
        }

        private void RemoveLyrics_Button_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Remove();
        }

        private void CopyLyrics_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Lyrics_ListView.SelectedItems.Any())
                _viewModel.Copy(Player.Position);
        }

        private void Undo_Button_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Undo(1);
        }

        private void Redo_Button_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Redo(1);
        }

        private void LyricsSort_Button_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Sort(_viewModel.LyricItems);
        }

        #endregion

        #region Tools

        private void MultilineEditMode_ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Lyrics_ListView.SelectionMode = ListViewSelectionMode.Multiple;
            ButtonsInMultilineEditMode_StackPanel.Visibility = Visibility.Visible;
            ButtonsInMultilineEditMode_Expand_Storyboard.Begin();
        }

        private void MultilineEditMode_ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Lyrics_ListView.SelectionMode = ListViewSelectionMode.Single;
            ButtonsInMultilineEditMode_Fold_Storyboard.Begin();
        }

        private void ButtonsInMultilineEditMode_Fold_Storyboard_Completed(object sender, object e)
        {
            ButtonsInMultilineEditMode_StackPanel.Visibility = Visibility.Collapsed;
        }

        #region Select tools

        private void Select_Reverse_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedRanges = Lyrics_ListView.SelectedRanges.ToList();

            Lyrics_ListView.SelectAll();
            foreach (ItemIndexRange range in selectedRanges)
                Lyrics_ListView.DeselectRange(range);
        }

        private void Select_BeforeItem_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = Lyrics_ListView.SelectedIndex;

            if (selectedIndex != -1)
            {
                Lyrics_ListView.SelectedIndex = -1;
                Lyrics_ListView.SelectRange(new ItemIndexRange(0, (uint) (selectedIndex + 1)));
            }
            else
                Lyrics_ListView.SelectAll();
        }

        private void Select_AfterItem_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (Lyrics_ListView.SelectedIndex == -1)
            {
                Lyrics_ListView.SelectAll();
                return;
            }

            int itemsCount = Lyrics_ListView.Items.Count;

            int selectIndex = Lyrics_ListView.SelectedIndex;
            uint selectCount = (uint) (itemsCount - selectIndex);

            Lyrics_ListView.SelectedItem = null;
            Lyrics_ListView.SelectRange(new ItemIndexRange(selectIndex, selectCount));
        }

        private void Select_Paragraph_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = Lyrics_ListView.SelectedIndex;
            int paragraphStart = 0;
            int paragraphLength = -1;

            var lyrics = _viewModel.LyricItems.ToList();

            foreach (var lyric in lyrics.Where(l => String.IsNullOrEmpty(l.Content)))
            {
                int currentId = lyrics.IndexOf(lyric);

                if (currentId <= selectedIndex)
                    paragraphStart = currentId + 1;
                else
                {
                    paragraphLength = currentId - paragraphStart;
                    break;
                }
            }

            if (paragraphLength == -1)
                paragraphLength =  lyrics.Count - paragraphStart;

            Lyrics_ListView.SelectedItem = null;
            Lyrics_ListView.SelectRange(new ItemIndexRange(paragraphStart, (uint) paragraphLength));
        }

        #endregion

        private async void LyricsFileInfo_Button_Click(object sender, RoutedEventArgs e)
        {
            LyricsFileInfo_Button.IsEnabled = false;
            await LyricsFileInfo_ContentDialog.ShowAsync();
            LyricsFileInfo_Button.IsEnabled = true;
        }

        private void PreviewMode_ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            foreach (Lyric item in _viewModel.LyricItems)
                MultilinePreview.Lyrics.Add(new Lyric(item));

            MultilinePreview.IsEnabled = true;
            MultilinePreview.Reposition(Player.Position);
            SinglePreview.IsEnabled = false;
        }

        private void PreviewMode_ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            SinglePreview.IsEnabled = true;
            SinglePreview.Reposition(Player.Position);

            MultilinePreview.Lyrics.Clear();
            MultilinePreview.IsEnabled = false;
        }

        private async void EnterMiniMode_Button_Click(object sender, RoutedEventArgs e)
        {
            ViewModePreferences preferences = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
            preferences.CustomSize = new Size(500, 500);
            preferences.ViewSizePreference = ViewSizePreference.Custom;
            bool modeSwitched = await _currentView.TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, preferences);

            if (!modeSwitched)
                throw new Exception("Can not enter Mini Mode");

            _viewModel.IsMiniMode = true;
        }

        private async void ExitMiniMode_Button_Click(object sender, RoutedEventArgs e)
        {
            await _currentView.TryEnterViewModeAsync(ApplicationViewMode.Default);
            _viewModel.IsMiniMode = false;
        }

        private void LyricTime_Button_Click(object sender, RoutedEventArgs e)
        {
            Lyric lyric = (sender as Button).DataContext as Lyric;

            if (Lyrics_ListView.SelectionMode == ListViewSelectionMode.Multiple ||
                Lyrics_ListView.SelectionMode == ListViewSelectionMode.Extended)
            { 
                var range = new ItemIndexRange(Lyrics_ListView.Items.IndexOf(lyric), 1);

                if (Lyrics_ListView.SelectedItems.Contains(lyric))
                    Lyrics_ListView.DeselectRange(range);
                else
                    Lyrics_ListView.SelectRange(range);
            }
            else if (Lyrics_ListView.SelectionMode != ListViewSelectionMode.None)
                Lyrics_ListView.SelectedItem = lyric;

            GoToLyricTime(lyric);
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

        private void Lyrics_ListView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            if (sender is ListView listView &&
                e.OriginalSource is Panel panel &&
                panel.DataContext is MainViewModel)
            {
                listView.SelectedItem = null;
            }
        }

        private void Lyrics_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.LastOrDefault() is Lyric currentLyric)
            {
                Lyrics_ListView.ScrollIntoView(currentLyric);

                LyricsContent_TextBox.Text = currentLyric.Content;
            }

            if (e.RemovedItems.Any())
                foreach (Lyric item in e.RemovedItems)
                    item.IsSelected = false;

            if (Lyrics_ListView.SelectedItems.LastOrDefault() is Lyric selectedLyric)
            {
                if (_backLyric != null)
                    _backLyric.IsSelected = false;
                _backLyric = selectedLyric;
                selectedLyric.IsSelected = true;

                InputSubmitOperations_ComboBox.SelectedItem = ModifyLyrics_ComboBoxItem;
            }
            else
            {
                LyricsContent_TextBox.Text = String.Empty;
                InputSubmitOperations_ComboBox.SelectedItem = AddLyrics_ComboBoxItem;
            }
        }

        #endregion

        private void LyricsPreview_Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Lyrics_ListView.SelectedItem = null;
        }

        private void MultilinePreview_ItemClick(object sender, ItemClickEventArgs e)
        {
            Lyric lyric = (Lyric) e.ClickedItem;
            GoToLyricTime(lyric);
        }

        #region Lyrics File Info Dialog Box
        
        private void LyricsFileInfo_ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            App.IsInputing = true;
        }

        private void LyricsFileInfo_ContentDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            App.IsInputing = false;
        }

        #endregion
    }
}