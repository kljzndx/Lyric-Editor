using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using SimpleLyricsEditor.Core;

namespace SimpleLyricsEditor.Models
{
    public abstract class SettingsPageBase : Page
    {
        protected Settings _settings = Settings.Current;

        protected SettingsPageBase()
        {
            this.Transitions = new TransitionCollection
            {
                new NavigationThemeTransition()
            };
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        ~SettingsPageBase()
        {
            _settings = null;
        }
    }
}