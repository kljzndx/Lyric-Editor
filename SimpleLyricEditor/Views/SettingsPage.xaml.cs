using SimpleLyricEditor.ViewModels;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleLyricEditor.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private Settings_ViewModel model = App.Locator.Settings;

        public SettingsPage()
        {
            this.InitializeComponent();
            Theme_ComboBox.SelectedIndex = /* if */ model.Settings.PageTheme == ElementTheme.Default ? 0 : /* else if */ model.Settings.PageTheme == ElementTheme.Light ? 1 : 2;

            bool isAlbumImage = model.Settings.BackgroundSourceType == BackgroundSourceTypeEnum.AlbumImage;
            BackgroundImageSource_AlbumImage_RadioButton.IsChecked = isAlbumImage;
            BackgroundImageSource_LocalImage_RadioButton.IsChecked = !isAlbumImage;

            switch (model.Settings.SelectItemAlwaysStaysIn)
            {
                case SelectItemAlwaysStaysIn_Enum.Top:
                    SelectItemAlwaysStaysIn_ComboBox.SelectedIndex = 0;
                    break;
                case SelectItemAlwaysStaysIn_Enum.Center:
                    SelectItemAlwaysStaysIn_ComboBox.SelectedIndex = 1;
                    break;
                case SelectItemAlwaysStaysIn_Enum.Bottom:
                    SelectItemAlwaysStaysIn_ComboBox.SelectedIndex = 2;
                    break;
                case SelectItemAlwaysStaysIn_Enum.ViewableArea:
                    SelectItemAlwaysStaysIn_ComboBox.SelectedIndex = 3;
                    break;
                default:
                    throw new Exception("未找到与当前值对应的分支");
            }
        }
        
        private void Theme_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var theBox = sender as ComboBox;
            model.Settings.PageTheme = /* if */ theBox.SelectedIndex == 0 ? ElementTheme.Default : /* else if */ theBox.SelectedIndex == 1 ? ElementTheme.Light : ElementTheme.Dark;
        }

        private void SelectItemAlwaysStaysIn_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var se = SelectItemAlwaysStaysIn_Enum.Top;
            switch ((sender as ComboBox).SelectedIndex)
            {
                case 0:
                    se = SelectItemAlwaysStaysIn_Enum.Top;
                    break;
                case 1:
                    se = SelectItemAlwaysStaysIn_Enum.Center;
                    break;
                case 2:
                    se = SelectItemAlwaysStaysIn_Enum.Bottom;
                    break;
                case 3:
                    se = SelectItemAlwaysStaysIn_Enum.ViewableArea;
                    break;
                default:
                    throw new Exception("没有与此项匹配的分支");
            }
            model.Settings.SelectItemAlwaysStaysIn = se;
        }
    }
}
