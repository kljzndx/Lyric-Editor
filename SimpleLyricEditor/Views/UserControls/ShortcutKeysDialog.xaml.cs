using HappyStudio.UwpToolsLibrary.Information;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricEditor.Views.UserControls
{
    public sealed partial class ShortcutKeysDialog : UserControl
    {
        private ObservableCollection<Models.ShortcutKey> lyricEditOperation = new ObservableCollection<Models.ShortcutKey>();
        private ObservableCollection<Models.ShortcutKey> audioPlayerOperation = new ObservableCollection<Models.ShortcutKey>();
        private ObservableCollection<Models.ShortcutKey> fileOperation = new ObservableCollection<Models.ShortcutKey>();
        public ShortcutKeysDialog()
        {
            this.InitializeComponent();
            Main_Grid.Visibility = Visibility.Collapsed;
            MainGrid_Transform.ScaleX = 0;
            MainGrid_Transform.ScaleY = 0;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Data/ShortcutKeys.xml"));
            string content = await FileIO.ReadTextAsync(file);
            string Language = AppInfo.Name == "简易歌词编辑器" ? "zh_CN" : "en_US";
            
            XDocument doc = XDocument.Parse(content);
            foreach (var item in doc.Element("ShortcutKeys").Elements("ShortcutKey"))
            {
                switch (item.Attribute("Class").Value)
                {
                    case "歌词操作":
                        lyricEditOperation.Add(new Models.ShortcutKey { Condition = item.Element("Condition").Element(Language).Value, Function = item.Element("Function").Element(Language).Value });
                        break;
                    case "播放器操作":
                        audioPlayerOperation.Add(new Models.ShortcutKey { Condition = item.Element("Condition").Element(Language).Value, Function = item.Element("Function").Element(Language).Value });
                        break;
                    case "文件操作":
                        fileOperation.Add(new Models.ShortcutKey { Condition = item.Element("Condition").Element(Language).Value, Function = item.Element("Function").Element(Language).Value });
                        break;
                }
            }
        }

        private void Disappear_Storyboard_Completed(object sender, object e)
        {
            Main_Grid.Visibility = Visibility.Collapsed;
        }

        public void Show()
        {
            Main_Grid.Visibility = Visibility.Visible;
            PopUp_Storyboard.Begin();
        }
    }
}
