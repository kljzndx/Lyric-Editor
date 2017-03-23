using LyricsEditor.Information;
using LyricsEditor.Model;
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

namespace LyricsEditor.UserControls
{
    public sealed partial class ShortcutKeysDescribedPanel : UserControl
    {
        private ObservableCollection<ShortcutKey> lyricEditClass = new ObservableCollection<ShortcutKey>();
        private ObservableCollection<ShortcutKey> audioPlayerClass = new ObservableCollection<ShortcutKey>();
        private ObservableCollection<ShortcutKey> fileClass = new ObservableCollection<ShortcutKey>();

        public ShortcutKeysDescribedPanel()
        {
            this.InitializeComponent();

            if (SystemInfo.DeviceType == "Windows.Mobile")
            {
                Root_Grid.Width = 320;
            }
        }

        public void PopUp()
        {
            Root_Grid.Visibility = Visibility.Visible;
            PopUp_Animation.Begin();
        }
        
        private void Disappear_Animation_Completed(object sender, object e)
        {
            Root_Grid.Visibility = Visibility.Collapsed;
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Disappear_Animation.Begin();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Data/ShortcutKeys.xml"));
            string content = await FileIO.ReadTextAsync(file);

            XDocument dcm = XDocument.Parse(content);

            foreach (var item in dcm.Element("ShortcutKeys").Elements("ShortcutKey"))
            {
                string theClass = item.Attribute("Class").Value;
                var shortcutKey = new ShortcutKey();
                string language = AppInfo.LanguageCode == "zh-CN" ? "zh_CN" : "en_US";

                shortcutKey.Condition = item.Element("Condition").Element(language).Value;
                shortcutKey.Function = item.Element("Function").Element(language).Value;

                switch (theClass)
                {
                    case "歌词操作":
                        lyricEditClass.Add(shortcutKey);
                        break;
                    case "播放器操作":
                        audioPlayerClass.Add(shortcutKey);
                        break;
                    case "文件操作":
                        fileClass.Add(shortcutKey);
                        break;
                }
            }

            LyricEditClass_Expander.Header += $" ({lyricEditClass.Count})";
            AudioPlayerClass_Expander.Header += $" ({audioPlayerClass.Count})";
            FileClass_Expander.Header += $" ({fileClass.Count})";
        }
    }
}
