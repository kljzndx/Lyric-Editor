using HappyStudio.UwpToolsLibrary.Auxiliarys;
using SimpleLyricEditor.Models;
using SimpleLyricEditor.Tools;
using SimpleLyricEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace SimpleLyricEditor
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Main_ViewModel model = App.Locator.Main;
        public MainPage()
        {
            this.InitializeComponent();

            model.SelectedItems = Lyrics_ListView.SelectedItems;

            audioPlayer.PositionChanged +=
                (s, e) =>
                {
                    if (e.IsUserChange)
                        lyricsPreview.RepositionLyric();
                    else
                        lyricsPreview.PreviewLyric();
                };

            audioPlayer.MusicFileChanged +=
                (s, e) =>
                {
                    model.AssociatedTags(e.NewMusic);
                    lyricsPreview.RepositionLyric();
                };

            model.LyricsListChanged += (s, e) => lyricsPreview.RepositionLyric();


            CoreWindow window = CoreWindow.GetForCurrentThread();
            window.KeyDown += Window_KeyDown;
            window.KeyUp += Window_KeyUp;

        }

        private void HideFastMenu()
        {
            FastMenu_Grid.Visibility = Visibility.Collapsed;
        }

        private async void GotoThisLyricTime()
        {
            try
            {
                audioPlayer.ChangeTime((Lyrics_ListView.SelectedItems.SingleOrDefault() as Lyric).Time, true);
            }
            catch (NullReferenceException)
            {
                return;
            }
            catch (InvalidOperationException)
            {
                await MessageBox.ShowAsync(CharacterLibrary.ErrorDialog.GetString("SelectedMultipleItemsError"), CharacterLibrary.ErrorDialog.GetString("Close"));
            }
        }
        
        private void Window_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Shift)
                DelLyric_Button.Content = "\uE107";
            if (!App.IsInputBoxGotFocus)
            {
                switch (args.VirtualKey)
                {
                    case VirtualKey.Space:
                        this.Focus(FocusState.Pointer);
                        break;
                    case VirtualKey.G:
                        GotoThisLyricTime();
                        break;
                }
            }
        }

        private void Window_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Shift)
                DelLyric_Button.Content = "\uE10A";
        }

        #region 歌词编辑区
        private void LyricsContent_TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && sender is TextBox t)
            {
                e.Handled = true;

                model.LyricContent = t.Text;

                if (App.IsPressCtrl)
                    model.AddLyric();
                else if (Lyrics_ListView.SelectedItems.Any())
                {
                    model.ChangeContent();
                    Lyrics_ListView.SelectedIndex = Lyrics_ListView.SelectedIndex < Lyrics_ListView.Items.Count - 1 ? Lyrics_ListView.SelectedIndex + 1 : -1;
                }
                else
                {
                    t.Text += "\n";
                    t.Select(t.Text.Length, 0);
                }
            }
        }

        private void MultilineEditMode_Button_Click(object sender, RoutedEventArgs e)
        {
            Lyrics_ListView.SelectionMode = ListViewSelectionMode.Multiple;
            model.IsMultilineEditMode = true;
        }

        private void ExitMultilineEditMode_Button_Click(object sender, RoutedEventArgs e)
        {
            Lyrics_ListView.SelectionMode = ListViewSelectionMode.Single;
            model.IsMultilineEditMode = false;
        }

        private void Select_Reverse_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (ListViewItem item in Lyrics_ListView.ItemsPanelRoot.Children)
                item.IsSelected = !item.IsSelected;
        }

        private void Select_BeforeItem_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (Lyrics_ListView.SelectedItem is Lyric)
                foreach (ListViewItem item in Lyrics_ListView.ItemsPanelRoot.Children)
                {
                    if (item.IsSelected)
                        break;
                    item.IsSelected = true;
                }
        }

        private void Select_AfterItem_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (Lyrics_ListView.SelectedItem is Lyric)
                for (int i = Lyrics_ListView.Items.Count - 1; i > 0; i--)
                {
                    ListViewItem l = (ListViewItem)Lyrics_ListView.ItemsPanelRoot.Children[i];
                    if (l.IsSelected)
                        break;
                    l.IsSelected = true;
                }
        }

        private void Select_Paragraph_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            //首先确认当前选定项是否有内容
            if (Lyrics_ListView.SelectedItem is Lyric thisSelectedItem && !String.IsNullOrWhiteSpace(thisSelectedItem.Content))
            {
                //当前选定项ID
                int sID = Lyrics_ListView.SelectedIndex;

                //遍历并选定接下来的项目 直到碰到没有内容的项目
                for (int i = sID; i < Lyrics_ListView.Items.Count; i++)
                {
                    if (String.IsNullOrWhiteSpace((Lyrics_ListView.Items[i] as Lyric).Content))
                        break;
                    (Lyrics_ListView.ItemsPanelRoot.Children[i] as ListViewItem).IsSelected = true;
                }

                //跟上面的一样，不过这次是反向遍历
                for (int i = sID; i > 0; i--)
                {
                    if (String.IsNullOrWhiteSpace((Lyrics_ListView.Items[i] as Lyric).Content))
                        break;
                    (Lyrics_ListView.ItemsPanelRoot.Children[i] as ListViewItem).IsSelected = true;
                }
            }
        }
        #endregion
        #region 歌词列表
        private void Lyrics_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string content = String.Empty;

            foreach (LyricItem item in e.RemovedItems)
                item.IsSelected = false;

            foreach (LyricItem item in (sender as ListView).SelectedItems)
            {
                content += item.Content + "\r\n";
                item.IsSelected = true;
            }

            model.LyricContent = content.Trim();
        }

        private void LyricItemTemplate_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            GotoThisLyricTime();
        }

        private void LyricItemTemplate_GotoThisTime_Click(object sender, EventArgs e)
        {
            audioPlayer.ChangeTime((sender as Lyric).Time, true);
        }
        #endregion
        #region 低栏
        private void MenuFlyout_Opened(object sender, object e)
        {
            var menu = sender as MenuFlyout;
            Style s = new Style()
            {
                TargetType = typeof(MenuFlyoutPresenter)
            };
            s.Setters.Add(new Setter { Property = RequestedThemeProperty, Value = model.Settings.PageTheme });
            menu.MenuFlyoutPresenterStyle = s;
        }

        private void Settings_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            HidePanel_SplitView.IsPaneOpen = !HidePanel_SplitView.IsPaneOpen;
        }
        #endregion

        private void Main_Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Link;
            e.DragUIOverride.Caption = CharacterLibrary.DragOrDrop.GetString("Caption");
        }

        private async void Main_Grid_Drop(object sender, DragEventArgs e)
        {
            if (!e.DataView.Contains(StandardDataFormats.StorageItems))
                return;
            var items = await e.DataView.GetStorageItemsAsync();

            bool isMusicFound = false;
            bool isLyricFound = false;

            foreach (var item in items)
            {
                if (item is StorageFile file)
                {
                    switch (file.FileType)
                    {
                        case ".mp3":
                        case ".flac":
                        case ".wav":
                        case ".aac":
                        case ".m4a":
                            if (!isMusicFound)
                            {
                                audioPlayer.ChangeMusic(await Music.ParseAsync(file));
                                isMusicFound = true;
                            }
                            break;
                        case ".txt":
                        case ".lrc":
                            if (!isLyricFound)
                            {
                                LyricFileTools.ChangeFile(file);
                                isLyricFound = true;
                            }
                            break;
                    }
                }
                if (isMusicFound && isLyricFound)
                    break;
            }
        }
        
    }
}
