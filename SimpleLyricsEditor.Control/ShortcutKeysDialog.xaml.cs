using System;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls;
using SimpleLyricsEditor.BLL;
using SimpleLyricsEditor.DAL.Factory;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricsEditor.Control
{
    public sealed partial class ShortcutKeysDialog : UserControl
    {
        public ShortcutKeysDialog()
        {
            InitializeComponent();
        }

        public void Show()
        {
            Visibility = Visibility.Visible;
            Show_Storyboard.Begin();
        }

        private async void ShortcutKeysDialog_Loaded(object sender, RoutedEventArgs e)
        {
            string keysFileContent = await new ShortcutKeysDocumentFileReader().GetFileContent();
            ViewModel.DialogUi = new ShortcutKeysDialogUiDeserializer().Deserialization(keysFileContent);
            var keysClass = new ShortcutKeysDeserializer().Deserialization(keysFileContent);

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
                    Content = listView,
                    Style = KeysClass_ExpanderStyle
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
            Visibility = Visibility.Collapsed;
        }
    }
}