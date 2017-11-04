using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricsEditor.Control
{
    public sealed partial class InputSubmitBox : UserControl
    {
        public static readonly DependencyProperty UserInputProperty = DependencyProperty.Register(
            nameof(UserInput), typeof(string), typeof(InputSubmitBox), new PropertyMetadata(String.Empty));

        public static readonly DependencyProperty SubmitButtonContentProperty = DependencyProperty.Register(
            nameof(SubmitButtonContent), typeof(string), typeof(InputSubmitBox), new PropertyMetadata("\uE10B"));

        private bool _isInputBoxGotFocus;
        private bool _isPressCtrl;

        public InputSubmitBox()
        {
            InitializeComponent();
            InputBox_Border.Visibility = Visibility.Collapsed;
            InputBox_Border.Opacity = 0;
            InputBox_Transform.ScaleX = 0;

            TextBox.AddHandler(KeyDownEvent, new KeyEventHandler(TextBox_KeyDown), true);
            TextBox.AddHandler(KeyUpEvent, new KeyEventHandler(TextBox_KeyUp), true);
        }

        public string UserInput
        {
            get => (string) GetValue(UserInputProperty);
            set => SetValue(UserInputProperty, value);
        }

        public string SubmitButtonContent
        {
            get => (string) GetValue(SubmitButtonContentProperty);
            set => SetValue(SubmitButtonContentProperty, value);
        }

        public event EventHandler Submited;

        public void ExpandInputBox()
        {
            InputBox_Border.Visibility = Visibility.Visible;

            Expand_Storyboard.Begin();
        }

        private void FoldInputBox()
        {
            if (!_isInputBoxGotFocus)
                Fold_Storyboard.Begin();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Submited?.Invoke(this, EventArgs.Empty);
        }
        
        private void Expand_Storyboard_Completed(object sender, object e)
        {
            TextBox.Focus(FocusState.Pointer);
        }

        private void Fold_Storyboard_Completed(object sender, object e)
        {
            InputBox_Border.Visibility = Visibility.Collapsed;
        }

        private void InputBox_Border_GotFocus(object sender, RoutedEventArgs e)
        {
            _isInputBoxGotFocus = true;
        }

        private void InputBox_Border_LostFocus(object sender, RoutedEventArgs e)
        {
            _isInputBoxGotFocus = false;
        }

        private void Root_Grid_LostFocus(object sender, RoutedEventArgs e)
        {
            Fold_Storyboard.Begin();
        }
        
        private void Root_Grid_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            ExpandInputBox();
        }

        private void Root_Grid_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ExpandInputBox();
        }

        private void Root_Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            FoldInputBox();
        }

        private void Root_Grid_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            FoldInputBox();
        }

        private void Root_Grid_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            FoldInputBox();
        }

        private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Control)
                _isPressCtrl = true;
        }

        private void TextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Control)
                _isPressCtrl = false;

            if (e.Key == VirtualKey.Enter && sender is TextBox tb)
            {
                if (_isPressCtrl)
                {
                    UserInput = tb.Text;
                    Submited?.Invoke(this, EventArgs.Empty);
                }
                // 换行
                else
                {
                    // 获取光标位置
                    int selectedId = tb.SelectionStart;

                                // 首先删除选中的内容
                    tb.Text = tb.Text.Remove(selectedId, tb.SelectionLength)
                                // 再添加换行符。。别问我前面为啥要加空格
                                .Insert(selectedId, ' ' + Environment.NewLine);

                    // 更改光标位置
                    tb.SelectionStart = selectedId + 2;
                }
            }
        }
    }
}