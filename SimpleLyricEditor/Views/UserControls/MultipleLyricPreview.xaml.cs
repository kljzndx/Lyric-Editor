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
using SimpleLyricEditor.ViewModels;
using SimpleLyricEditor.Extensions;
using SimpleLyricEditor.Models;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricEditor.Views.UserControls
{
    public sealed partial class MultipleLyricPreview : UserControl
    {
        // Using a DependencyProperty as the backing store for LyricsList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LyricsListProperty = DependencyProperty.Register("LyricsList", typeof(IList<LyricItem>), typeof(MultipleLyricPreview), new PropertyMetadata(new List<LyricItem>()));
        
        private Settings settings = Settings.GetSettingsObject();
        private bool isMouseEntered;

        public TimeSpan Position
        {
            get { return (TimeSpan)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(TimeSpan), typeof(MultipleLyricPreview), new PropertyMetadata(TimeSpan.Zero));



        public IList<LyricItem> LyricsList
        {
            get { return (IList<LyricItem>)GetValue(LyricsListProperty); }
            set { SetValue(LyricsListProperty, value); }
        }

        private LyricItem currentLyric;

        public LyricItem CurrentLyric
        {
            get { return currentLyric; }
            set
            {
                currentLyric = value;
                this.Bindings.Update();
            }
        }

        private int nextLyricID;

        public MultipleLyricPreview()
        {
            this.InitializeComponent();
        }

        public void RefreshLyric()
        {
            if (!LyricsList.Any())
                return;

            if (currentLyric is LyricItem)
                switch (this.Visibility)
                {
                    case Visibility.Visible:
                        currentLyric.IsSelected = true;
                        break;
                    case Visibility.Collapsed:
                        currentLyric.IsSelected = false;
                        break;
                }

            if (Position.ToLyricTimeString().Remove(7) == LyricsList[nextLyricID].Time.ToLyricTimeString().Remove(7))
            {
                CurrentLyric = LyricsList[nextLyricID];
                nextLyricID = nextLyricID < LyricsList.Count - 1 ? nextLyricID + 1 : 0;
            }
        }


        public void ScrollList(int currentLyricID)
        {
            //计算要提前多少项
            int interpolation = ((int)Main_ListView.ActualHeight / 44) / 2;

            int result = currentLyricID > interpolation ? currentLyricID - interpolation : 0;

            Main_ListView.ScrollIntoView(LyricsList[result], ScrollIntoViewAlignment.Leading);
        }

        public void Reposition()
        {
            if (!LyricsList.Any())
                return;
            
            if (Position.CompareTo(LyricsList.First().Time) <= -1)
            {
                CurrentLyric = null;
                nextLyricID = 0;
                return;
            }

            if (Position.CompareTo(LyricsList.Last().Time) >= 0)
            {
                CurrentLyric = LyricsList.Last();
                nextLyricID = 0;
                return;
            }

            for (int i = 0; i < LyricsList.Count; i++)
            {
                if (Position.CompareTo(LyricsList[i].Time) == -1)
                {
                    CurrentLyric = LyricsList[i-1];
                    nextLyricID = i;
                    break;
                }
            }
            
        }

        private void Main_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (LyricItem item in e.AddedItems)
                item.IsSelected = true;
            foreach (LyricItem item in e.RemovedItems)
                item.IsSelected = false;

            if (!isMouseEntered)
                ScrollList((sender as ListView).SelectedIndex);
        }

        private void Main_ListView_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            isMouseEntered = true;
        }

        private void Main_ListView_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            isMouseEntered = false;
        }
    }
}
