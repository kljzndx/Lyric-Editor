using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricsEditor.Control
{
    public sealed partial class InputContentDialog : ContentDialog
    {
        public static readonly DependencyProperty UserInputProperty = DependencyProperty.Register(
            nameof(UserInput), typeof(string), typeof(InputContentDialog), new PropertyMetadata(String.Empty));

        public InputContentDialog()
        {
            InitializeComponent();
        }

        public string UserInput
        {
            get => (string) GetValue(UserInputProperty);
            set => SetValue(UserInputProperty, value);
        }
    }
}