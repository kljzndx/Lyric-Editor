using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricsEditor.Control
{
    public sealed partial class InputContentDialog : ContentDialog
    {
        public static readonly DependencyProperty UserInputProperty = DependencyProperty.Register(
            nameof(UserInput), typeof(string), typeof(InputContentDialog), new PropertyMetadata(String.Empty));

        public string UserInput
        {
            get => (string) GetValue(UserInputProperty);
            set => SetValue(UserInputProperty, value);
        }

        public InputContentDialog()
        {
            this.InitializeComponent();
        }
    }
}
