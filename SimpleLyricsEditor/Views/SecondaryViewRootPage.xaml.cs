using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using HappyStudio.UwpToolsLibrary.Auxiliarys;
using SimpleLyricsEditor.Core;
using SimpleLyricsEditor.Models;
using SimpleLyricsEditor.ViewModels;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleLyricsEditor.Views
{
    /// <summary>
    ///     可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SecondaryViewRootPage : Page
    {
        private readonly Settings _settings = Settings.Current;

        public static SecondaryViewRootPage Current;

        public SecondaryViewRootPage()
        {
            InitializeComponent();
            Current = this;

            _settings.PropertyChanged += Settings_PropertyChanged;
        }

        public void Navigate(PageModel pm)
        {
            if (Main_Frame.SourcePageType == pm.PageType)
                return;

            Main_Frame.Navigate(pm.PageType, pm.Title);

            Main_Frame.BackStack.Clear();
            Main_Frame.ForwardStack.Clear();

            Back_Button.Visibility = Visibility.Collapsed;
            Title_TextBlock.Margin = new Thickness();
        }
        
        private void Back_Button_Click(object sender, RoutedEventArgs e)
        {
            Main_Frame.GoBack();
        }
        
        private void Main_Frame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Parameter is string title)
                Title_TextBlock.Text = title;

            if (Main_Frame.CanGoBack && Back_Button.Visibility == Visibility.Collapsed)
            {
                Back_Button.Visibility = Visibility.Visible;
                Title_TextBlock.Margin = new Thickness(45, 0, 0, 0);
                Enter_Storyboard.Begin();
            }
            else if (!Main_Frame.CanGoBack && Back_Button.Visibility == Visibility.Visible)
            {
                Title_TextBlock.Margin = new Thickness();
                Back_Storyboard.Begin();
            }
        }

        private async void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(_settings.PageTheme)) &&
                _settings.PageTheme != ElementTheme.Default &&
                Application.Current.RequestedTheme != (ApplicationTheme) ((int) _settings.PageTheme - 1))
            {
                Dictionary<string, UICommandInvokedHandler> buttons = new Dictionary<string, UICommandInvokedHandler>();
                if (ApiInformation.IsTypePresent("Windows.ApplicationModel.Core.CoreApplication"))
                    buttons.Add(CharacterLibrary.MessageBox.GetString("RebootApp"),
                        async u => await CoreApplication.RequestRestartAsync(String.Empty));

                await MessageBox.ShowAsync
                (
                    String.Empty, 
                    CharacterLibrary.ErrorInfo.GetString("RebootApp"),
                    buttons,
                    CharacterLibrary.MessageBox.GetString("Close")
                );
            }
        }

        private void Light_Button_Click(object sender, RoutedEventArgs e)
        {
            _settings.PageTheme = ElementTheme.Light;
        }

        private void Dark_Button_Click(object sender, RoutedEventArgs e)
        {
            _settings.PageTheme = ElementTheme.Dark;
        }

        private void Back_Storyboard_Completed(object sender, object e)
        {
            Back_Button.Visibility = Visibility.Collapsed;
        }
    }
}