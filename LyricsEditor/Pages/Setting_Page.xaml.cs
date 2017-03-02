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
        private StorageFile backgroundImageFile;
        public Setting_Page()
        {
            this.InitializeComponent();
            AppTheme_ComboBox.SelectedIndex = settings.Theme == ApplicationTheme.Light ? 0 : 1;

        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (settings.UserDefinedBackgroundImagePath != String.Empty)
            {
                backgroundImageFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync("BackgroundImage");
            }

            if (settings.BackgroundImageType == BackgroundImageTypeEnum.AlbumImage)
                BackgroundType_Album_RadioButton.IsChecked = true;
            else
                BackgroundType_UserDefined_RadioButton.IsChecked = true;
        }

        private void AppTheme_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var theme = (sender as ComboBox).SelectedIndex == 0 ? ApplicationTheme.Light : ApplicationTheme.Dark;
            settings.Theme = theme;
            AppTheme_Massage_TextBlock.Visibility = Application.Current.RequestedTheme != settings.Theme ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BackgroundType_Album_RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            settings.BackgroundImageType = BackgroundImageTypeEnum.AlbumImage;
            BackgroundImagePath_Grid.Visibility = Visibility.Collapsed;
        }

        private async void BackgroundType_UserDefined_RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (settings.UserDefinedBackgroundImagePath == String.Empty)
                if (!await ChanageBackgroundImageFileAsync())
                {
                    BackgroundType_Album_RadioButton.IsChecked = true;
                    return;
                }
                    

            //避免打开设置面板时重新加载背景图
            if (settings.BackgroundImageType != BackgroundImageTypeEnum.UserDefined)
                await ChanageBackgroundImageAsync();

            BackgroundImagePath_Grid.Visibility = Visibility.Visible;
            settings.BackgroundImageType = BackgroundImageTypeEnum.UserDefined;
        }

        
        private async void ChanageBackgroundImageFile_Rectangle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await ChanageBackgroundImageFileAsync();
            await ChanageBackgroundImageAsync();
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
            backgroundImageFile = _imageFile;
            return true;
        }

        private async Task ChanageBackgroundImageAsync()
        {
            var image = new BitmapImage();
            image.SetSource(await backgroundImageFile.OpenAsync(FileAccessMode.Read));
            settings.BackgroundImage = image;
        }

    }
}
