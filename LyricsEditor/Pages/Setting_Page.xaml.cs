using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using LyricsEditor.Model;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace LyricsEditor.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Setting_Page : Page
    {
        private Setting settings = Setting.GetSettingObject();
        public Setting_Page()
        {
            this.InitializeComponent();
            AppTheme_ComboBox.SelectedIndex = settings.Theme == ElementTheme.Default ? 0 : (settings.Theme == ElementTheme.Light ? 1 : 2);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (settings.BackgroundImageType == BackgroundImageTypeEnum.AlbumImage)
                BackgroundType_Album_RadioButton.IsChecked = true;
            else
                BackgroundType_UserDefined_RadioButton.IsChecked = true;
        }

        private void AppTheme_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int sltid = (sender as ComboBox).SelectedIndex;
            var theme = sltid == 0 ? ElementTheme.Default : (sltid == 1 ? ElementTheme.Light : ElementTheme.Dark);
            settings.Theme = theme;
        }

        private void BackgroundType_Album_RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            settings.BackgroundImageType = BackgroundImageTypeEnum.AlbumImage;
            BackgroundImagePath_Grid.Visibility = Visibility.Collapsed;
        }

        private async void BackgroundType_UserDefined_RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (settings.UserDefinedBackgroundImagePath == String.Empty && !await ChanageBackgroundImageFileAsync())
            {
                BackgroundType_Album_RadioButton.IsChecked = true;
                return;
            }
            BackgroundImagePath_Grid.Visibility = Visibility.Visible;
            settings.BackgroundImageType = BackgroundImageTypeEnum.UserDefined;
        }


        private async void ChanageBackgroundImageFile_Rectangle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await ChanageBackgroundImageFileAsync();
        }

        public async Task<bool> ChanageBackgroundImageFileAsync()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            var _imageFile = await picker.PickSingleFileAsync();
            if (_imageFile is null)
                return false;
            StorageApplicationPermissions.FutureAccessList.AddOrReplace("BackgroundImage", _imageFile);
            settings.UserDefinedBackgroundImagePath = _imageFile.Path;
            return true;
        }

    }
}
