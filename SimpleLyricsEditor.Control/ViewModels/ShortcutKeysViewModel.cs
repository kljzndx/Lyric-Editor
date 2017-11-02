using GalaSoft.MvvmLight;
using SimpleLyricsEditor.DAL;

namespace SimpleLyricsEditor.Control.ViewModels
{
    public class ShortcutKeysViewModel : ViewModelBase
    {
        private ShortcutKeysDialogUI _dialogUi;

        public ShortcutKeysDialogUI DialogUi
        {
            get => _dialogUi;
            set => Set(ref _dialogUi, value);
        }
    }
}