using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
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
        private static readonly PageModel SettingsPageModel =
            new PageModel(CharacterLibrary.SettingsRoot.GetString("Title"), typeof(SettingsRootPage));

        private readonly Settings _settings = Settings.Current;
        private StorageFile _lyricsFile;
        private StorageFile _musicFile;

        public UiFramework()
        {
            InitializeComponent();
            GlobalKeyNotifier.KeyDown += WindowKeyDown;

            if (ApiInformation.IsEventPresent(typeof(FlyoutBase).FullName, "Closing"))
                OpenFile_MenuFlyout.Closing += (s, e) => OpenFile_AppBarToggleButton.IsChecked = false;
            else
                OpenFile_MenuFlyout.Closed += (s, e) => OpenFile_AppBarToggleButton.IsChecked = false;
        }

        private bool IsDark => _settings.PageTheme == ElementTheme.Dark || _settings.PageTheme == ElementTheme.Default &&
                               Application.Current.RequestedTheme == ApplicationTheme.Dark;

        private async Task OpenMusicFile()
        {
            _musicFile = await MusicFileOpenPicker.PickFile();
            if (_musicFile != null)
                MusicFileNotifier.ChangeFile(_musicFile);
        }

        private async Task OpenLyricsFile()
        {
            var file = await LyricsFileOpenPicker.PickFile();
            if (file == null)
                return;

            _lyricsFile = file;
            LyricsFileChangeNotifier.ChangeFile(_lyricsFile);
        }

        private async Task SaveFile()
        {
            if (_lyricsFile == null)
            {
                var file = await LyricsFileSavePicker.PickFile();
                if (file != null)
                    _lyricsFile = file;
                else
                    return;
            }

            LyricsFileSaveNotifier.SaveFile(_lyricsFile);
        }

        private async Task SaveAs()
        {
            var file = await LyricsFileSavePicker.PickFile();
            if (file == null)
                return;

            _lyricsFile = file;
            LyricsFileSaveNotifier.SaveFile(_lyricsFile);
        }

        private async void WindowKeyDown(object sender, GlobalKeyEventArgs e)
        {
            if (e.IsPressCtrl)
                switch (e.Key)
                {
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
        }

        private void NewFile_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            _lyricsFile = null;
            LyricsFileChangeNotifier.ChangeFile(_lyricsFile);
        }

        private void OpenFile_AppBarToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            OpenFile_MenuFlyout.ShowAt(sender as FrameworkElement);

            OpenFile_AppBarToggleButton.Foreground = new SolidColorBrush(Colors.White);
        }

        private void OpenFile_AppBarToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!IsDark)
                OpenFile_AppBarToggleButton.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void OpenFile_MenuFlyout_Opening(object sender, object e)
        {
            OpenFile_MenuFlyout.MenuFlyoutPresenterStyle =
                IsDark ? Dark_MenuFlyoutPresenter : Light_MenuFlyoutPresenter;
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

        private void Settings_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Root_SplitView.IsPaneOpen = !Root_SplitView.IsPaneOpen;

            SecondaryView_Frame.Navigate(typeof(SecondaryViewRootPage), SettingsPageModel);
        }
    }
}