using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using SimpleLyricsEditor.BLL.Pickers;
using SimpleLyricsEditor.Core;
using SimpleLyricsEditor.Events;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleLyricsEditor.Views.SettingsPages
{
    /// <summary>
    ///     可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BackgroundSettingsPage : Page
    {
        private readonly Settings _settings = Settings.Current;

        public BackgroundSettingsPage()
        {
            InitializeComponent();
        }
        
        private async void LocalImagePath_Rectangle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageFile file = await ImageFileOpenPicker.PickFile();
            if (file is null)
                return;

            _settings.LocalBackgroundImagePath = file.Path;
            StorageFile localFile = await file.CopyAsync(ApplicationData.Current.LocalFolder, "Background.img", NameCollisionOption.ReplaceExisting);
            await ImageFileNotifier.ChangeFile(localFile);
        }
    }
}