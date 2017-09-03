using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using GalaSoft.MvvmLight;
using SimpleLyricsEditor.BLL;
using SimpleLyricsEditor.BLL.LyricsOperations;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.DAL.Factory;
using SimpleLyricsEditor.Events;
using SimpleLyricsEditor.IBLL;

namespace SimpleLyricsEditor.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string _lyricContent;

        public MainViewModel()
        {
            _lyricsTags = LyricsTagFactory.CreateTags();
            LyricItems = new ObservableCollection<Lyric>();
            UndoOperations = new ObservableCollection<LyricsOperationBase>();
            RedoOperations = new ObservableCollection<LyricsOperationBase>();
            UndoOperations.CollectionChanged += UndoOperations_CollectionChanged;
            RedoOperations.CollectionChanged += RedoOperations_CollectionChanged;

            LyricsFileChangeNotification.FileChanged += LyricsFileChanged;
            LyricsFileSaveNotification.RunSaved += LyricsFileRunSaved;
        }

        private List<LyricsTag> _lyricsTags;

        public List<LyricsTag> LyricsTags
        {
            get => _lyricsTags;
            set => Set(ref _lyricsTags, value);
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
            var opt = CreateOperation(new Remove(SelectedItems.Cast<Lyric>(), LyricItems));
            opt.Do();
        }

        public void Move(TimeSpan time)
        {
            var opt = CreateOperation(new Move(time, SelectedItems.Cast<Lyric>()));
            opt.Do();
        }

        public void Modify()
        {
            var opt = CreateOperation(new Modify(SelectedItems.Cast<Lyric>(), _lyricContent));
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

        private async void LyricsFileChanged(object sender, FileChangeEventArgs e)
        {
            string fileContent = await LyricsFileIO.ReadText(e.File);
            var lines = fileContent.Replace('\r', '\n').Split('\n').Select(l => l.Trim());
            var tuple = LyricsSerializer.Deserialization(lines);

            LyricItems.Clear();
            foreach (Lyric lyric in tuple.lyrics)
                LyricItems.Add(lyric);

            LyricsTags = tuple.tags.ToList();
        }

        private async void LyricsFileRunSaved(object sender, FileChangeEventArgs e)
        {
            string content = LyricsSerializer.Serialization(LyricItems, LyricsTags.Where(t => !String.IsNullOrWhiteSpace(t.TagValue)));
            await LyricsFileIO.WriteText(e.File, content);
        }
    }
}