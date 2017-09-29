using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SimpleLyricsEditor.Models;
using SimpleLyricsEditor.ViewModels;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleLyricsEditor.Views
{
    /// <summary>
    ///     可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SecondaryViewRootPage : Page
    {
        public SecondaryViewRootPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is PageModel pm)
                Main_Frame.Navigate(pm.PageType, pm.Title);

            Main_Frame.BackStack.Clear();
            Main_Frame.ForwardStack.Clear();
        }

        private void Back_Button_Click(object sender, RoutedEventArgs e)
        {
            Title_TextBlock.Margin = new Thickness();
            Main_Frame.GoBack();
        }

        private void Main_Frame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Parameter is string title)
                Title_TextBlock.Text = title;

            if (Main_Frame.CanGoBack && Back_Button.Visibility == Visibility.Collapsed)
            {
                Title_TextBlock.Margin = new Thickness(45, 0, 0, 0);
                Enter_Storyboard.Begin();
            }
            else if (!Main_Frame.CanGoBack && Back_Button.Visibility == Visibility.Visible)
            {
                Title_TextBlock.Margin = new Thickness();
                Back_Storyboard.Begin();
            }
        }
    }
}