﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using SimpleLyricsEditor.Control;
using SimpleLyricsEditor.Core;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.Events;
using SimpleLyricsEditor.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SimpleLyricsEditor.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MainViewModel _viewModel;
        private readonly Settings _settings = Settings.Current;
        private bool _isPressCtrl;
        private bool _isPressShift;
        private bool _lyricsListGotFocus;

        public MainPage()
        {
            this.InitializeComponent();
            _viewModel = this.DataContext as MainViewModel;
            _viewModel.SelectedItems = Lyrics_ListView.SelectedItems;
            _viewModel.UndoOperations.CollectionChanged += UndoOperations_CollectionChanged;

            GlobalKeyNotifier.KeyDown += WindowKeyDown;
            GlobalKeyNotifier.KeyUp += WindowKeyUp;
        }

        private void WindowKeyDown(object sender, GlobalKeyEventArgs e)
        {
            _isPressCtrl = e.IsPressCtrl;
            _isPressShift = e.IsPressShift;

            if (e.IsPressShift)
            {
                AddButton_Transform.Rotation = 0;
            }

            if (!e.IsInputing && e.Key == VirtualKey.I)
                LyricsContent_TextBox.Focus(FocusState.Pointer);

            if (!e.IsInputing)
            {
                switch (e.Key)
                {
                    case VirtualKey.Space:
                        this.Focus(FocusState.Pointer);

                        if (Lyrics_ListView.SelectedItems.Any())
                            _viewModel.Move(Player.Position);
                        else
                            _viewModel.Add(-1, Player.Position, _isPressShift);
                        break;
                    case VirtualKey.C:
                        _viewModel.Copy(Player.Position);
                        break;
                    case VirtualKey.Delete:
                        _viewModel.Remove();
                        break;
                    case VirtualKey.M:
                        _viewModel.Modify();
                        break;
                    case VirtualKey.S:
                        _viewModel.Sort();
                        break;
                    case VirtualKey.Up:
                        this.Focus(FocusState.Pointer);
                        if (!_lyricsListGotFocus)
                            Lyrics_ListView.SelectedIndex =
                                Lyrics_ListView.SelectedIndex > -1
                                ? Lyrics_ListView.SelectedIndex - 1
                                : Lyrics_ListView.Items.Count - 1;
                        break;
                    case VirtualKey.Down:
                        this.Focus(FocusState.Pointer);
                        if (!_lyricsListGotFocus)
                            Lyrics_ListView.SelectedIndex =
                                Lyrics_ListView.SelectedIndex < Lyrics_ListView.Items.Count - 1
                                ? Lyrics_ListView.SelectedIndex + 1
                                : -1;
                        break;
                }
            }
        }

        private void WindowKeyUp(object sender, GlobalKeyEventArgs e)
        {
            _isPressCtrl = e.IsPressCtrl;
            _isPressShift = e.IsPressShift;

            if (!e.IsPressShift)
            {
                AddButton_Transform.Rotation = 180;
            }
        }

        private void UndoOperations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SinglePreview.Reposition(Player.Position);
        }

        private void Player_SourceChanged(AudioPlayer sender, MusicChangeEventArgs args)
        {
            if (_settings.BackgroundSourceType == BackgroundSourceTypeEnum.AlbumImage)
                BlurBackground.SetSource(args.Source.AlbumImage);

            SinglePreview.Reposition(Player.Position);
        }

        private void Player_PositionChanged(AudioPlayer sender, PositionChangeEventArgs args)
        {
            if (args.IsUserChange)
                SinglePreview.Reposition(args.Position);
            else
                SinglePreview.RefreshLyric(args.Position);
        }

        private void LyricsContent_TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            App.IsInputing = true;
        }

        private void LyricsContent_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            App.IsInputing = false;
        }

        private void LyricsContent_TextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key == VirtualKey.Enter && sender is TextBox tb)
            {
                if (_isPressCtrl)
                {
                    _viewModel.LyricContent = tb.Text;
                    _viewModel.Add(Lyrics_ListView.SelectedIndex, Player.Position, _isPressShift);
                }
                else if (_isPressShift || !Lyrics_ListView.SelectedItems.Any())
                {
                    int selectId = tb.SelectionStart;
                    tb.Text = tb.Text.Remove(selectId, tb.SelectionLength).Insert(selectId, " " + Environment.NewLine);
                    tb.SelectionStart = selectId + 2;
                }
                else if (Lyrics_ListView.SelectedItems.Any())
                {
                    _viewModel.LyricContent = tb.Text;
                    _viewModel.Modify();
                }
            }
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Add(Lyrics_ListView.SelectedIndex, Player.Position, _isPressShift);
        }

        private void Copy_Button_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Copy(Player.Position);
        }

        private void Remove_Button_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Remove();
        }

        private void Move_Button_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Move(Player.Position);
        }
        
        private void Modify_Button_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Modify();
        }

        private void Lyrics_ListView_GotFocus(object sender, RoutedEventArgs e)
        {
            _lyricsListGotFocus = true;
        }

        private void Lyrics_ListView_LostFocus(object sender, RoutedEventArgs e)
        {
            _lyricsListGotFocus = false;
        }

        private void Lyrics_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LyricsContent_TextBox.Text = Lyrics_ListView.SelectedIndex > -1
                ? (Lyrics_ListView.Items[Lyrics_ListView.SelectedIndex] as Lyric).Content
                : String.Empty;
        }
    }
}
