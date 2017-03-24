using LyricsEditor.Information;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
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
    public sealed partial class GetReviewsPanel : UserControl
    {
        public GetReviewsPanel()
        {
            this.InitializeComponent();
        }

        public void StartDisplay()
        {
            Root_Grid.Visibility = Visibility.Visible;
            Display_Storyboard.Begin();
        }

        private async void GetReviews(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9mx4frgq4rqs"));
            Hide_Storyboard.Begin();
        }

        private void Hide_Storyboard_Completed(object sender, object e)
        {
            Root_Grid.Visibility = Visibility.Collapsed;
            AppInfo.IsReviewsed = true;
        }

    }
}
