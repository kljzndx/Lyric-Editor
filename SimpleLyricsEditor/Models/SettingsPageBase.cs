using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
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
        }

        ~SettingsPageBase()
        {
            _settings = null;
        }
    }
}