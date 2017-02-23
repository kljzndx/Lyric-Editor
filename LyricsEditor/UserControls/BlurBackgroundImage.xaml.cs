using System;
using System.Reflection;
using LyricsEditor.Model;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LyricsEditor.UserControls
{
    public sealed partial class BlurBackgroundImage : UserControl
    {
        private static readonly Type typeBlur = GetBlurType();
        private static Type GetBlurType()
        {
            Type _typeBlur = null;
            try
            {
                _typeBlur = Assembly.Load(new AssemblyName("Microsoft.Toolkit.Uwp.UI.Animations"))
                    .GetType("Microsoft.Toolkit.Uwp.UI.Animations.Behaviors.Blur");
            }
            catch (Exception)
            {
                // Cannot load the assembly or get the type
                // we rethrow the exception for debugging
                //throw;
            }
            return _typeBlur;
        }

        private void SetBlurToSelf()
        {
            if (typeBlur != null)
            {
                var blurObj = typeBlur.GetConstructor(Type.EmptyTypes).Invoke(null);
                var valueprop = typeBlur.GetProperty("Value");
                valueprop.SetValue(blurObj, settings.BackgroundBlurDegree);
                typeBlur.GetProperty("Duration").SetValue(blurObj, 0);
                typeBlur.GetProperty("AutomaticallyStart").SetValue(blurObj, true);
                typeBlur.GetMethod("Attach").Invoke(blurObj, new object[] { image });

                settings.PropertyChanged += (s, e) => 
                {
                    if (e.PropertyName == "BackgroundBlurDegree")
                        valueprop.SetValue(blurObj, settings.BackgroundBlurDegree);
                };
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
            this.settings = Setting.GetSettingObject();
            this.SetBlurToSelf();
        }
    }
}
