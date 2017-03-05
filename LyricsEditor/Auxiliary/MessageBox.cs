using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace LyricsEditor.Auxiliary
{
    public static class MessageBox
    {
        public static async Task ShowMessageBoxAsync(string content, string buttonContent)
        {
            MessageDialog box = new MessageDialog(content);
            box.Commands.Add(new UICommand(buttonContent));
            await box.ShowAsync();
        }
        public static async Task ShowMessageBoxAsync(string title, string content, string buttonContent)
        {
            MessageDialog box = new MessageDialog(content, title);
            box.Commands.Add(new UICommand(buttonContent));
            await box.ShowAsync();
        }
        public static async Task ShowMessageBoxAsync(string title, string content, Dictionary<string, UICommandInvokedHandler> buttons)
        {
            MessageDialog box = new MessageDialog(content, title);
            foreach (var item in buttons)
                box.Commands.Add(new UICommand(item.Key, item.Value));
            await box.ShowAsync();
        }
        public static async Task ShowMessageBoxAsync(string title, string content, Dictionary<string, UICommandInvokedHandler> buttons, string lastButtonContent)
        {
            MessageDialog box = new MessageDialog(content, title);
            foreach (var item in buttons)
                box.Commands.Add(new UICommand(item.Key, item.Value));
            box.Commands.Add(new UICommand(lastButtonContent));
            await box.ShowAsync();
        }
    }
}
