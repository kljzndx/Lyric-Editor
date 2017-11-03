using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SimpleLyricsEditor.BLL;
using SimpleLyricsEditor.Control.ViewModels;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.DAL.Factory;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricsEditor.Control
{
    public sealed partial class UpdateLogDialog : UserControl
    {
        private readonly UpdateLogViewModel _viewModel = new UpdateLogViewModel();

        public UpdateLogDialog()
        {
            InitializeComponent();
        }
        
        public event EventHandler Hided;
        
        public void Show()
        {
            Visibility = Visibility.Visible;
            Show_Storyboard.Begin();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.GetDialogUIAsync();
            await _viewModel.GetAllLogsAsync();
            _viewModel.CurrentUpdateLog = _viewModel.AllLogs.First();
        }

        private async void AllVersions_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.CurrentUpdateLog = e.AddedItems.First() as UpdateLog;

            await _viewModel.ReadLogContentAsync();

            Content_Pivot.SelectedIndex = 0;
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Hide_Storyboard.Begin();
        }

        private void Hide_Storyboard_Completed(object sender, object e)
        {
            Visibility = Visibility.Collapsed;
            Hided?.Invoke(this, EventArgs.Empty);
        }
    }
}