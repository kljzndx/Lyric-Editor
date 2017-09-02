using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SimpleLyricsEditor.BLL;
using SimpleLyricsEditor.Events;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SimpleLyricsEditor.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UiFramework : Page
    {
        private StorageFile _musicFile;
        private StorageFile _lyricsFile;

        public UiFramework()
        {
            this.InitializeComponent();
        }

        private void Settings_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Root_SplitView.IsPaneOpen = !Root_SplitView.IsPaneOpen;
        }

        private async void OpenMusicFile_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            _musicFile = await MusicFileOpenPicker.PickFile();
            if (_musicFile != null)
                MusicFileNotification.ChangeFile(_musicFile);
        }

        private async void OpenLyricsFile_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            StorageFile file = await LyricsFileOpenPicker.PickFile();
            if (file == null)
                return;

            _lyricsFile = file;
            LyricsFileChangeNotification.ChangeFile(_lyricsFile);
        }
    }
}
