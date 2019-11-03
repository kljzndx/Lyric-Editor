using System;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Information;
using Microsoft.Advertising.WinRT.UI;
using Microsoft.Services.Store.Engagement;
using SimpleLyricsEditor.BLL.Pickers;
using SimpleLyricsEditor.Core;
using SimpleLyricsEditor.Events;
using SimpleLyricsEditor.Models;
using SimpleLyricsEditor.Views.SettingsPages;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SimpleLyricsEditor.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UiFramework : Page
    {
        private static readonly PageModel SettingsPageModel = new PageModel(typeof(SettingsRootPage));

        private static readonly PageModel AboutPageModel = new PageModel(typeof(AboutPage));

        private readonly Settings _settings = Settings.Current;
        private StorageFile _lyricsFile;
        private StorageFile _musicFile;
        private string _lyricsFileName;

        private int BootTimes
        {
            get => _settings.GetSetting("BootTimes", 0);
            set => _settings.SettingContainer.Values["BootTimes"] = value;
        }

        private string UpdateLogVersion
        {
            get => _settings.GetSetting("UpdateLogVersion", String.Empty);
            set => _settings.SettingContainer.Values["UpdateLogVersion"] = value;
        }


        public UiFramework()
        {
            InitializeComponent();

            GlobalKeyNotifier.KeyDown += OnWindowKeyDown;
            MusicFileNotifier.FileChangeRequested += OnMusicFileFileChanged;
            LyricsFileNotifier.FileChanged += OnLyricsFileChanged;
            AdsVisibilityNotifier.DisplayRequested += AdsVisibilityNotifier_DisplayRequested;
            AdsVisibilityNotifier.HideRequested += AdsVisibilityNotifier_HideRequested;

            if (ApiInformation.IsEventPresent(typeof(FlyoutBase).FullName, "Closing"))
                OpenFile_MenuFlyout.Closing += (s, e) => OpenFile_AppBarToggleButton.IsChecked = false;
            else
                OpenFile_MenuFlyout.Closed += (s, e) => OpenFile_AppBarToggleButton.IsChecked = false;
        }

        #region Lyrics file operations

        private async Task OpenMusicFile()
        {
            var file = await MusicFileOpenPicker.PickFile();
            if (file != null)
                MusicFileNotifier.ChangeFileRequest(file);
        }

        private async Task OpenLyricsFile()
        {
            var file = await LyricsFileOpenPicker.PickFile();
            if (file == null)
                return;
            
            LyricsFileNotifier.ChangeFile(file);
        }

        private async Task SaveFile()
        {
            if (_lyricsFile == null)
            {
                var file = await LyricsFileSavePicker.PickFile(_lyricsFileName);
                if (file != null)
                    _lyricsFile = file;
                else
                    return;
            }

            LyricsFileNotifier.SendSaveRequest(_lyricsFile);
        }

        private async Task SaveAs()
        {
            var file = await LyricsFileSavePicker.PickFile(_lyricsFileName);
            if (file == null)
                return;

            LyricsFileNotifier.SendSaveRequest(file);
        }

        #endregion

        private static async Task WriteReview()
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9mx4frgq4rqs"));
        }

        private void SecondaryViewNavigate(PageModel pm)
        {
            Root_SplitView.IsPaneOpen = !Root_SplitView.IsPaneOpen;

            SecondaryViewRootPage.Current.Navigate(pm);
        }

        private async void OnWindowKeyDown(object sender, GlobalKeyEventArgs e)
        {
            if (e.IsPressCtrl)
                switch (e.Key)
                {
                    case VirtualKey.N:
                        LyricsFileNotifier.ChangeFile(null);
                        break;
                    case VirtualKey.M:
                        await OpenMusicFile();
                        break;
                    case VirtualKey.L:
                        await OpenLyricsFile();
                        break;
                    case VirtualKey.S:
                        if (e.IsPressShift)
                            await SaveAs();
                        else
                            await SaveFile();
                        break;
                }
            else
                switch (e.Key)
                {
                    case VirtualKey.F1:
                        ShortcutKeysDialog.Show();
                        break;
                }

        }

        private async void UiFramework_Loaded(object sender, RoutedEventArgs e)
        {
            if (!StoreServicesFeedbackLauncher.IsSupported())
                FeedbackInFeedbackHub_MenuFlyoutItem.Visibility = Visibility.Collapsed;

            if (UpdateLogVersion != AppInfo.Version)
            {
                UpdateLogDialog.Show();
                BootTimes = 1;
            }
            else
            {
                BootTimes = BootTimes < Int32.MaxValue ? BootTimes + 1 : 1;

                if (BootTimes == 10)
                    await GetReviews_ContentDialog.ShowAsync();
            }
        }

        #region Notifiers
        
        private void OnMusicFileFileChanged(object sender, FileChangeEventArgs e)
        {
            _musicFile = e.File;
            _lyricsFileName = _musicFile.DisplayName;
        }

        private void OnLyricsFileChanged(object sender, FileChangeEventArgs e)
        {
            if (e.File != null && e.File.FileType == ".lrc")
                _lyricsFile = e.File;
            else
                _lyricsFile = null;
        }

        private void AdsVisibilityNotifier_DisplayRequested(object sender, EventArgs e)
        {
            MsAdControl.Visibility = Visibility.Visible;
            MsAdControl.Resume();
            AdsFadeIn_Storyboard.Begin();
            AdsFadeOut_Storyboard.Stop();
        }

        private void AdsVisibilityNotifier_HideRequested(object sender, EventArgs e)
        {
            AdsFadeOut_Storyboard.Begin();
            AdsFadeIn_Storyboard.Stop();
        }

        private void AdsFadeOut_Storyboard_Completed(object sender, object e)
        {
            MsAdControl.Suspend();
            MsAdControl.Visibility = Visibility.Collapsed;
        }

        #endregion

        private void UpdateLogDialog_Hided(object sender, EventArgs e)
        {
            _settings.SettingContainer.Values["UpdateLogVersion"] = AppInfo.Version;
        }

        #region Fast menu

        private async void OpenMusicFile_Button_Click(object sender, RoutedEventArgs e)
        {
            await OpenMusicFile();
        }

        private async void OpenLyricsFile_Button_Click(object sender, RoutedEventArgs e)
        {
            await OpenLyricsFile();
        }

        private void HideThisMenu_Button_Click(object sender, RoutedEventArgs e)
        {
            FastMenuFadeOut_Storyboard.Begin();
        }

        private void FastMenuFadeOut_Storyboard_Completed(object sender, object e)
        {
            FastMenu_StackPanel.Visibility = Visibility.Collapsed;
        }

        #endregion
        #region Ads
        
        private void MsAdControl_ErrorOccurred(object sender, AdErrorEventArgs e)
        {
            MsAdControl.Visibility = Visibility.Collapsed;
        }

        private void MsAdControl_OnAdRefreshed(object sender, RoutedEventArgs e)
        {
            MsAdControl.Visibility = Visibility.Visible;
        }

        #endregion
        #region Get Reviews Content Dialog

        private async void GetReviews_ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await WriteReview();
        }

        private async void GetReviews_ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await WriteReview();
        }

        #endregion
        #region Bottom Bar

        private void NewFile_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            LyricsFileNotifier.ChangeFile(null);
        }

        private void OpenFile_AppBarToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            OpenFile_MenuFlyout.ShowAt(sender as FrameworkElement);
        }
        
        private void MenuFlyoutOfBottomBar_Opening(object sender, object e)
        {
            (sender as MenuFlyout).MenuFlyoutPresenterStyle =
                _settings.IsLightTheme ? Light_MenuFlyoutPresenter : Dark_MenuFlyoutPresenter;
        }

        private async void OpenMusicFile_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            await OpenMusicFile();
        }

        private async void OpenLyricsFile_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            await OpenLyricsFile();
        }

        private async void Save_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await SaveFile();
        }

        private async void SaveAs_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await SaveAs();
        }

        #region More Menu

        private void ShortcutKeysList_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ShortcutKeysDialog.Show();
        }

        private void UpdateLog_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateLogDialog.Show();
        }

        #region Feedback

        private async void FeedbackInFeedbackHub_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (!await StoreServicesFeedbackLauncher.GetDefault().LaunchAsync())
                await MessageBox.ShowAsync(
                    CharacterLibrary.MessageBox.GetString("Error"),
                    CharacterLibrary.ErrorInfo.GetString("ShowFeedbackHubError"),
                    CharacterLibrary.MessageBox.GetString("Close"));
        }

        private async void FeedbackInGitHub_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/kljzndx/Lyric-Editor/issues"));
        }

        private async void FeedbackInEmail_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            await EmailEx.SendAsync("kljzndx@outlook.com",
                AppInfo.Name + ' ' + AppInfo.Version + ' ' + CharacterLibrary.Email.GetString("Feedback"),
                String.Empty);
        }

        #endregion

        private async void GitHub_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/kljzndx/Lyric-Editor"));
        }

        private void Settings_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            SecondaryViewNavigate(SettingsPageModel);
        }

        private void About_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            SecondaryViewNavigate(AboutPageModel);
        }

        #endregion

        #endregion
    }
}