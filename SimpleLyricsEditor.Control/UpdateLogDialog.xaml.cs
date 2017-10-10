using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SimpleLyricsEditor.BLL;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.DAL.Factory;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricsEditor.Control
{
    public sealed partial class UpdateLogDialog : UserControl
    {
        private static readonly DependencyProperty SelectedLogProperty = DependencyProperty.Register(
            nameof(SelectedLog), typeof(UpdateLog), typeof(UpdateLogDialog), new PropertyMetadata(null));

        private readonly ObservableCollection<UpdateLog> _allLogs = new ObservableCollection<UpdateLog>();

        public UpdateLogDialog()
        {
            InitializeComponent();
        }

        public event EventHandler Hided;

        private UpdateLog SelectedLog
        {
            get => (UpdateLog) GetValue(SelectedLogProperty);
            set => SetValue(SelectedLogProperty, value);
        }

        private async Task ReadLogContent(UpdateLog log)
        {
            if (String.IsNullOrEmpty(log.Content))
                log.Content = await UpdateLogFilesIO.GetLogContent(log.FileName);
        }

        public void Show()
        {
            this.Visibility = Visibility.Visible;
            Show_Storyboard.Begin();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            string json = await UpdateLogFilesIO.GetAllLogsJson();
            var logs = UpdateLogDeserializer.Deserialization(json);

            foreach (UpdateLog log in logs)
                _allLogs.Add(log);

            SelectedLog = _allLogs.First();
            await ReadLogContent(SelectedLog);
        }

        private async void AllVersions_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedLog = e.AddedItems.First() as UpdateLog;

            await ReadLogContent(SelectedLog);

            Content_Pivot.SelectedIndex = 0;
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Hide_Storyboard.Begin();
        }

        private void Hide_Storyboard_Completed(object sender, object e)
        {
            this.Visibility = Visibility.Collapsed;
            Hided?.Invoke(this, EventArgs.Empty);
        }
    }
}