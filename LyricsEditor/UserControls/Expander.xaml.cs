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
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LyricsEditor.UserControls
{
    [ContentProperty(Name = "ExpandContent")]
    public sealed partial class Expander : UserControl
    {
        
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(Expander), new PropertyMetadata(""));
        
        public bool IsExpanded
        {
            get => (bool)GetValue(IsExpandedProperty);
            set
            {
                SetValue(IsExpandedProperty, value);
                Fitnes_RotateTransform.Angle = value ? 90 : 0;
            }
        }

        // Using a DependencyProperty as the backing store for IsExpanded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(Expander), new PropertyMetadata(false));
        
        

        public object ExpandContent
        {
            get { return (object)GetValue(ExpandContentProperty); }
            set { SetValue(ExpandContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExpandContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExpandContentProperty =
            DependencyProperty.Register(nameof(ExpandContent), typeof(object), typeof(Expander), new PropertyMetadata(null));
        

        public Expander()
        {
            this.InitializeComponent();

            Title_Button.Click += (s, e) => IsExpanded = !IsExpanded;
        }
        
    }
}
