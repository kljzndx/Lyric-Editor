using System;
using System.Windows.Input;

namespace SimpleLyricsEditor.Events
{
    public static class SavingDialogWhenClosingNotifier
    {
        public static event EventHandler ValidatingRequested;
        public static event EventHandler<bool> ValidatingResultReturned;

        public static void RequestValidating()
        {
            ValidatingRequested?.Invoke(null, EventArgs.Empty);
        }

        public static void ReturnValidatingResult(bool result)
        {
            ValidatingResultReturned?.Invoke(null, result);
        }
    }
}