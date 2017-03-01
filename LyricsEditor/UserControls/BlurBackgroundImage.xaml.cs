using System;
using System.Reflection;
using LyricsEditor.Model;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.AccessCache;

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

        private Setting settings;
        private BitmapImage albumImage = new BitmapImage();
        public BlurBackgroundImage()
        {
            this.InitializeComponent();
            this.settings = Setting.GetSettingObject();
            this.settings.PropertyChanged += 
                (s, e) => 
                {
                    if (e.PropertyName == "BackgroundImageType" && settings.BackgroundImageType == BackgroundImageTypeEnum.AlbumImage)
                        settings.BackgroundImage = albumImage;
                };
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (settings.BackgroundImageType == BackgroundImageTypeEnum.UserDefined)
            {
                var imageFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync("BackgroundImage");
                await settings.BackgroundImage.SetSourceAsync(await imageFile.OpenAsync(Windows.Storage.FileAccessMode.Read));
            }
        }

        public void RefreshImage(object sender, MusicChanageEventArgs e)
        {
            albumImage = e._Music.AlbumImage;
            if (settings.BackgroundImageType == BackgroundImageTypeEnum.AlbumImage)
                settings.BackgroundImage = albumImage;
        }
        
    }
}
