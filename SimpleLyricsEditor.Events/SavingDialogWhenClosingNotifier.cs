using System;
using System.Windows.Input;

namespace SimpleLyricsEditor.Events
{
    public static class SavingDialogWhenClosingNotifier
    {
        public static event EventHandler ValidatingRequested;
        public static event EventHandler ShowingDialogRequested;

        public static void RequestValidating()
        {
            ValidatingRequested?.Invoke(null, EventArgs.Empty);
        }

        public static void RequestToShowDialog()
        {
            ShowingDialogRequested?.Invoke(null, EventArgs.Empty);
        }
    }
}