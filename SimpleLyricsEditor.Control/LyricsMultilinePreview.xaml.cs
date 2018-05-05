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
using SimpleLyricsEditor.Control.Models;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.Events;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleLyricsEditor.Control
{
    public sealed partial class LyricsMultilinePreview : LyricsPreviewBase
    {
        private static readonly object InterpolationLocker = new object();
        
        public LyricsMultilinePreview()
        {
            this.InitializeComponent();
            _itemsCountOnView = ComputeItemsCountOnView();
        }

        private int _itemsCountOnView;

        public event ItemClickEventHandler ItemClick;

        private int ComputeItemsCountOnView()
        {
            return (int) Root_Viewer.ActualHeight / 44;
        }
        
        private double ComputeScrollVerticalOffset(Lyric item)
        {
            int itemId = Lyrics.IndexOf(item);
            if (itemId + 1 < _itemsCountOnView / 2)
                return 0;

            int itemHeight = Main_ListView.ContainerFromItem(item) is ListViewItem listViewItem
                ? (int) listViewItem.DesiredSize.Height
                : 44;

            return (itemHeight * itemId) - ((Root_Viewer.ActualHeight / 2) - (itemHeight / 2));
        }

        private void LyricsMultilinePreview_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            lock (InterpolationLocker)
                _itemsCountOnView = ComputeItemsCountOnView();
        }

        private void LyricsMultilinePreview_Refreshed(LyricsPreviewBase sender, LyricsPreviewRefreshEventArgs args)
        {
            foreach (Lyric lyric in Lyrics.Where(l => l.IsSelected))
                lyric.IsSelected = false;

            if (args.CurrentLyric.Equals(Lyric.Empty))
            {
                Root_Viewer.ChangeView(null, 0, null);
                return;
            }

            args.CurrentLyric.IsSelected = true;

            Root_Viewer.ChangeView(null, ComputeScrollVerticalOffset(args.CurrentLyric), null);
        }

        private void Main_ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ItemClick?.Invoke(this, e);
        }
    }
}
