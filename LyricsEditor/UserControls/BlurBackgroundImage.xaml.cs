using System;
using System.Reflection;
using LyricsEditor.Model;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.AccessCache;
using Windows.Storage;
using System.Threading.Tasks;
using LyricsEditor.EventArg;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LyricsEditor.UserControls
{
    public sealed partial class BlurBackgroundImage : UserControl
    {
        private Setting settings;
        private BitmapImage albumImage = new BitmapImage();
        private StorageFile imageFile;

        private BitmapImage imageSource;

        public BitmapImage ImageSource
        {
            get { return imageSource; }
            set
            {
                bool b = imageSource != null;
                imageSource = value;
                if(b)
                    FadeOut_Storyboard.Begin();
                else
                {
                    image.Source = imageSource;
                    FadeIn_Storyboard.Begin();
                }
            }
        }

        public BlurBackgroundImage()
        {
            this.InitializeComponent();
            this.settings = Setting.GetSettingObject();
            this.settings.PropertyChanged += SettingItemChanged;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (settings.UserDefinedBackgroundImagePath != String.Empty)
                imageFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync("BackgroundImage");

            if (settings.BackgroundImageType == BackgroundImageTypeEnum.UserDefined)
            {
                var imageSource = new BitmapImage();
                await imageSource.SetSourceAsync(await imageFile.OpenAsync(FileAccessMode.Read));
                ImageSource = imageSource;
            }
        }

        private async void SettingItemChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "UserDefinedBackgroundImagePath")
            {
                imageFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync("BackgroundImage");
                if (settings.BackgroundImageType == BackgroundImageTypeEnum.UserDefined)
                    await UpdateImageSourceAsync();
            }

            if (e.PropertyName == "BackgroundImageType")
            {
                if (settings.BackgroundImageType == BackgroundImageTypeEnum.AlbumImage)
                    ImageSource = albumImage;
                else if (settings.BackgroundImageType == BackgroundImageTypeEnum.UserDefined)
                    await UpdateImageSourceAsync();
            }
        }

        public void RefreshAlbumImage(object sender, MusicChanageEventArgs e)
        {
            albumImage = e.NewMusic.AlbumImage;
            if (settings.BackgroundImageType == BackgroundImageTypeEnum.AlbumImage)
                ImageSource = albumImage;
        }

        private async Task UpdateImageSourceAsync()
        {
            var imageSource = new BitmapImage();
            if (imageFile != null)
                await imageSource.SetSourceAsync(await imageFile.OpenAsync(FileAccessMode.Read));
            ImageSource = imageSource;
        }

        private void FadeOut_Storyboard_Completed(object sender, object e)
        {
            image.Source = imageSource;
            FadeIn_Storyboard.Begin();
        }
    }
}
