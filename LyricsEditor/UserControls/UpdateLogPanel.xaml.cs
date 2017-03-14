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
using System.Collections.ObjectModel;
using LyricsEditor.Model;
using LyricsEditor.Information;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LyricsEditor.UserControls
{
    public sealed partial class UpdateLogPanel : UserControl
    {
        private Setting settings = Setting.GetSettingObject();
        private UpdateLog thelog = new UpdateLog();
        private ObservableCollection<UpdateLog> allLogs = new ObservableCollection<UpdateLog>();

        public UpdateLogPanel()
        {
            this.InitializeComponent();
            settings.CreateSetting("UpdateLogVersion", "1.4.0");
            if (SystemInfo.DeviceType == "Windows.Mobile")
                Root_Grid.Width = 320;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var allLog = await Tools.UpdateLogTools.GetUpdateLog();
            thelog = allLog.theVersionUpdateLog;
            allLogs = allLog.allUpdateLog;
            Bindings.Update();

            if (settings.SettingsObject.Values["UpdateLogVersion"].ToString() != AppInfo.AppVersion)
            {
                StartPopup();
            }
        }

        public void StartPopup()
        {
            Root_Grid.Visibility = Visibility.Visible;
            PopUp_Animation.Begin();
        }

        private void Disappear_Animation_Completed(object sender, object e)
        {
            Root_Grid.Visibility = Visibility.Collapsed;
            settings.SettingsObject.Values["UpdateLogVersion"] = AppInfo.AppVersion;
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Disappear_Animation.Begin();
        }
    }
}
