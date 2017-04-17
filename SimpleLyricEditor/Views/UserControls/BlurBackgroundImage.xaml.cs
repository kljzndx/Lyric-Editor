using SimpleLyricEditor.EventArgss;
using SimpleLyricEditor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricEditor.Views.UserControls
{
    public sealed partial class BlurBackgroundImage : UserControl
    {
        private Settings settings = Settings.GetSettingsObject();
        private BitmapImage albumImage = new BitmapImage();
        private StorageFile imageFile;

        public BitmapImage ImageSource
        {
            get { return (BitmapImage)GetValue(ImageSourceProperty); }
            set
            {
                var g = ImageSource;
                
                SetValue(ImageSourceProperty, value);

                if (g.PixelHeight > 0)
                    FadeOut_Storyboard.Begin();
                else
                    SwitchImage();
            }
        }

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(BitmapImage), typeof(BlurBackgroundImage), new PropertyMetadata(new BitmapImage()));


        public BlurBackgroundImage()
        {
            this.InitializeComponent();
            settings.PropertyChanged += Settings_PropertyChanged;
        }

        private async void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LocalBackgroundImagePath")
            {
                imageFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync("BackgroundImage");
                if (settings.BackgroundSourceType == BackgroundSourceTypeEnum.LocalImage)
                    await ChangeImage();
            }

            if (e.PropertyName == "BackgroundSourceType")
            {
                if (settings.BackgroundSourceType == BackgroundSourceTypeEnum.LocalImage)
                    await ChangeImage();
                else
                    ImageSource = albumImage;
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (settings.LocalBackgroundImagePath != String.Empty)
                imageFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync("BackgroundImage");
            else if (settings.BackgroundSourceType == BackgroundSourceTypeEnum.LocalImage)
                settings.BackgroundSourceType = BackgroundSourceTypeEnum.AlbumImage;

            if (settings.BackgroundSourceType == BackgroundSourceTypeEnum.LocalImage)
                await ChangeImage();
        }

        private async System.Threading.Tasks.Task ChangeImage()
        {
            var img = new BitmapImage();
            if (imageFile is StorageFile)
                img.SetSource(await imageFile.OpenAsync(FileAccessMode.Read));
            ImageSource = img;
        }

        private void SwitchImage()
        {
            this.Bindings.Update();
            FadeIn_Storyboard.Begin();
        }

        public void AudioPlayer_MusicFileChanged(object sender, MusicFileChangeEventArgs e)
        {
            albumImage = e.NewMusic.AlbumImage;
            if (settings.BackgroundSourceType == BackgroundSourceTypeEnum.AlbumImage)
                ImageSource = albumImage;
        }

    }
}
