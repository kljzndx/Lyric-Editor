using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using GalaSoft.MvvmLight;
using SimpleLyricsEditor.BLL.LyricsOperations;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.IBLL;

namespace SimpleLyricsEditor.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string _lyricContent;

        public MainViewModel()
        {
            LyricItems = new ObservableCollection<Lyric>();
            UndoOperations = new ObservableCollection<LyricsOperationBase>();
            RedoOperations = new ObservableCollection<LyricsOperationBase>();
            UndoOperations.CollectionChanged += UndoOperations_CollectionChanged;
            RedoOperations.CollectionChanged += RedoOperations_CollectionChanged;
        }

        public ObservableCollection<Lyric> LyricItems { get; }
        public IList<object> SelectedItems { get; set; }

        public ObservableCollection<LyricsOperationBase> UndoOperations { get; }
        public ObservableCollection<LyricsOperationBase> RedoOperations { get; }

        public bool CanUndo => UndoOperations.Any();
        public bool CanRedo => RedoOperations.Any();

        public string LyricContent
        {
            get => _lyricContent;
            set => Set(ref _lyricContent, value);
        }

        public LyricsOperationBase CreateOperation(LyricsOperationBase operation)
        {
            UndoOperations.Insert(0, operation);
            RedoOperations.Clear();

            return operation;
        }

        public void Undo(int count)
        {
            for (int i = 0; i < count; i++)
            {
                UndoOperations.First().Undo();
                RedoOperations.Insert(0, UndoOperations.First());
                UndoOperations.Remove(UndoOperations.First());
            }
        }

        public void Redo(int count)
        {
            for (int i = 0; i < count; i++)
            {
                RedoOperations.First().Do();
                UndoOperations.Insert(0, RedoOperations.First());
                RedoOperations.Remove(RedoOperations.First());
            }
        }

        public void Add(int index, TimeSpan time, bool isReverseAdd)
        {
            var opt = CreateOperation(new Add(time, _lyricContent, index, isReverseAdd, LyricItems));
            opt.Do();
        }

        public void Remove()
        {
            var opt = CreateOperation(new Remove(SelectedItems.Cast<Lyric>().ToList(), LyricItems));
            opt.Do();
        }

        public void Move(TimeSpan time)
        {
            var opt = CreateOperation(new Move(time, SelectedItems.Cast<Lyric>().ToList()));
            opt.Do();
        }

        public void Modify()
        {
            var opt = CreateOperation(new Modify(SelectedItems.Cast<Lyric>().ToList(), _lyricContent));
            opt.Do();
        }
        
        private void UndoOperations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(CanUndo));
        }

        private void RedoOperations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(CanRedo));
        }
    }
}