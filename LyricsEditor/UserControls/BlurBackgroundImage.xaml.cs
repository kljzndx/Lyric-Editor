using System;
using System.Reflection;
using LyricsEditor.Model;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LyricsEditor.UserControls
{
    public sealed partial class BlurBackgroundImage : UserControl
    {
        private static Type typeBlur = GetBlurType();

        private static Type GetBlurType()
        {
            Type typeBlur = null;
            try
            {
                typeBlur = Assembly.Load(new AssemblyName("Microsoft.Toolkit.Uwp.UI.Animations"))
                    .GetType("Microsoft.Toolkit.Uwp.UI.Animations.Behaviors.Blur");
            }
            catch (Exception)
            {
                // Cannot load the assembly or get the type
                // we rethrow the exception for debugging
                throw;
            }
            return typeBlur;
        }

        private void SetBlurToSelf()
        {
            if (typeBlur != null)
            {
                var blurObj = typeBlur.GetConstructor(new Type[] { }).Invoke(new object[] { });
                typeBlur.GetProperty("Value").SetValue(blurObj, settings.BackgroundBlurDegree);
                typeBlur.GetProperty("Duration").SetValue(blurObj, 0);
                typeBlur.GetProperty("AutomaticallyStart").SetValue(blurObj, true);
                typeBlur.GetMethod("Attach").Invoke(blurObj, new object[] { image });
            }
            //var blur = new Blur()
            //{
            //    Value = settings.BackgroundBlurDegree,
            //    Duration = 0,
            //    AutomaticallyStart = true
            //};
            //blur.Attach(image);
        }


        private Setting settings;
        public BlurBackgroundImage()
        {
            this.InitializeComponent();
            settings = Setting.GetSettingObject();
            this.SetBlurToSelf();
        }
    }
}
