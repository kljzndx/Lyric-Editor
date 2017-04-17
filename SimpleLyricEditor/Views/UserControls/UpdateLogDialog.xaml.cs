using HappyStudio.UwpToolsLibrary.Information;
using SimpleLyricEditor.Models;
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
    public sealed partial class UpdateLogDialog : UserControl
    {
        private UpdateLog thisLog = new UpdateLog();
        private ObservableCollection<UpdateLog> logs = new ObservableCollection<UpdateLog>();
        private Settings settings = Settings.GetSettingsObject();

        public UpdateLogDialog()
        {
            this.InitializeComponent();
            Main_Grid.Visibility = Visibility.Collapsed;
            MainGrid_Transform.ScaleX = 0;
            MainGrid_Transform.ScaleY = 0;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (settings.GetSetting("UpdateLogVersion", "1.4.0") != AppInfo.Version)
                Show();

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Data/UpdateLog.xml"));
            string content = await FileIO.ReadTextAsync(file);

            XDocument doc = XDocument.Parse(content);

            foreach (XElement item in doc.Element("UpdateLogs").Elements("UpdateLog"))
                logs.Add(new UpdateLog { Version = item.Element("Version").Value, Content = item.Element("Content").Value.Trim(), Date = item.Element("Date").Value });

            thisLog = logs[0];
            this.Bindings.Update();
        }

        private void Disappear_Storyboard_Completed(object sender, object e)
        {
            Main_Grid.Visibility = Visibility.Collapsed;
            settings.SettingObject.Values["UpdateLogVersion"] = AppInfo.Version;
        }

        public void Show()
        {
            Main_Grid.Visibility = Visibility.Visible;
            PopUp_Storyboard.Begin();
        }

    }
}
