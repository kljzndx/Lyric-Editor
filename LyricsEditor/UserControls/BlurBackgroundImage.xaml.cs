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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LyricsEditor.UserControls
{
    public sealed partial class BlurBackgroundImage : UserControl
    {
        /* This is TaoMengxu done by dynamically loaded DLL
       
            //private static readonly Type typeBlur = GetBlurType();
            //private static Type GetBlurType()
            //{
            //    Type _typeBlur = null;
            //    try
            //    {
            //        _typeBlur = Assembly.Load(new AssemblyName("Microsoft.Toolkit.Uwp.UI.Animations"))
            //            .GetType("Microsoft.Toolkit.Uwp.UI.Animations.Behaviors.Blur");
            //    }
            //    catch (Exception)
            //    {
            //        // Cannot load the assembly or get the type
            //        // we rethrow the exception for debugging
            //        // throw;
            //    }
            //    return _typeBlur;
            //}

            //private void SetBlurToSelf()
            //{
            //    if (typeBlur != null)
            //    {
            //        var blurObj = typeBlur.GetConstructor(Type.EmptyTypes).Invoke(null);
            //        var valueprop = typeBlur.GetProperty("Value");
            //        valueprop.SetValue(blurObj, settings.BackgroundBlurDegree);
            //        typeBlur.GetProperty("Duration").SetValue(blurObj, 0);
            //        typeBlur.GetProperty("AutomaticallyStart").SetValue(blurObj, true);
            //        typeBlur.GetMethod("Attach").Invoke(blurObj, new object[] { image });

            //        settings.PropertyChanged += (s, e) => 
            //        {
            //            if (e.PropertyName == "BackgroundBlurDegree")
            //                valueprop.SetValue(blurObj, settings.BackgroundBlurDegree);
            //        };
            //    }
            //    //var blur = new Blur()
            //    //{
            //    //    Value = settings.BackgroundBlurDegree,
            //    //    Duration = 0,
            //    //    AutomaticallyStart = true
            //    //};
            //    //blur.Attach(image);
            //}
        */
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(nameof(ImageSource), typeof(BitmapImage), typeof(BlurBackgroundImage), new PropertyMetadata(new BitmapImage()));
        private Setting settings;
        private BitmapImage albumImage = new BitmapImage();
        private StorageFile imageFile;
        
        public BitmapImage ImageSource
        {
            get { return (BitmapImage)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
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
                await ImageSource.SetSourceAsync(await imageFile.OpenAsync(FileAccessMode.Read));
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

    }
}
