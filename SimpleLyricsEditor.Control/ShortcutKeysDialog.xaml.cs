using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using HappyStudio.UwpToolsLibrary.Control;
using SimpleLyricsEditor.DAL.Factory;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricsEditor.Control
{
    public sealed partial class ShortcutKeysDialog : UserControl
    {
        public ShortcutKeysDialog()
        {
            this.InitializeComponent();
        }

        public void Show()
        {
            this.Visibility = Visibility.Visible;
            Show_Storyboard.Begin();
        }

        private async void ShortcutKeysDialog_Loaded(object sender, RoutedEventArgs e)
        {
            StorageFile keysFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Data/ShortcutKeysDocument/ShortcutKeys.xml"));
            string keysFileContent = await FileIO.ReadTextAsync(keysFile);
            var keysClass = ShortcutKeysDeserializer.Deserialization(keysFileContent);

            foreach (var item in keysClass)
            {
                ListView listView = new ListView
                {
                    ItemsSource = item.Value.ToList(),
                    Style = Keys_ListViewStyle
                };

                Expander expander = new Expander
                {
                    Header = item.Key,
                    ExpandContent = listView
                };

                KeysClass_StackPanel.Children.Add(expander);
            }
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Hide_Storyboard.Begin();
        }

        private void Hide_Storyboard_Completed(object sender, object e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}
