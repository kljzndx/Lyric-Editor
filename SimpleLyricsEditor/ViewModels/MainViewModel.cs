using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;
using GalaSoft.MvvmLight;
using SimpleLyricsEditor.BLL;
using SimpleLyricsEditor.BLL.LyricsOperations;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.DAL.Factory;
using SimpleLyricsEditor.Events;
using SimpleLyricsEditor.Views;

namespace SimpleLyricsEditor.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private static XmlDocument _savedToastXmlDocument;

        private bool _isMiniMode;
        private List<LyricsTag> _lyricsTags;

        private StorageFile _optionFile;

        public MainViewModel()
        {
            _lyricsTags = new LyricsTagFactory().CreateTags();
            LyricItems = new ObservableCollection<Lyric>();
            UndoOperations = new ObservableCollection<LyricsOperationBase>();
            RedoOperations = new ObservableCollection<LyricsOperationBase>();

            LyricItems.CollectionChanged += LyricItems_CollectionChanged;
            UndoOperations.CollectionChanged += UndoOperations_CollectionChanged;
            RedoOperations.CollectionChanged += RedoOperations_CollectionChanged;

            LyricsFileNotifier.FileChanged += LyricsFileChanged;
            LyricsFileNotifier.SaveRequested += LyricsFileSaveRequested;
        }

        public bool IsMiniMode
        {
            get => _isMiniMode;
            set => Set(ref _isMiniMode, value);
        }

        public List<LyricsTag> LyricsTags
        {
            get => _lyricsTags;
            set => Set(ref _lyricsTags, value);
        }

        public ObservableCollection<Lyric> LyricItems { get; }
        public IList<object> SelectedItems { get; set; }

        public ObservableCollection<LyricsOperationBase> UndoOperations { get; }
        public ObservableCollection<LyricsOperationBase> RedoOperations { get; }

        public bool IsLyricsItemAny => LyricItems.Count >= 2;
        public bool CanUndo => UndoOperations.Any();
        public bool CanRedo => RedoOperations.Any();
        
        public void CreateOperation(LyricsOperationBase operation)
        {
            if (UndoOperations.Count >= 20)
                UndoOperations.Remove(UndoOperations.Last());

            operation.Do();
            UndoOperations.Insert(0, operation);
            RedoOperations.Clear();
        }

        public void Undo(int count)
        {
            for (var i = 0; i < count; i++)
            {
                UndoOperations.First().Undo();
                RedoOperations.Insert(0, UndoOperations.First());
                UndoOperations.Remove(UndoOperations.First());
            }
        }

        public void Redo(int count)
        {
            for (var i = 0; i < count; i++)
            {
                RedoOperations.First().Do();
                UndoOperations.Insert(0, RedoOperations.First());
                RedoOperations.Remove(RedoOperations.First());
            }
        }

        public void Add(int index, TimeSpan time, string content, bool isReverseAdd)
        {
            CreateOperation(new Add(time, content, index, isReverseAdd, LyricItems));
        }

        public void Copy(TimeSpan newTime)
        {
            if (!SelectedItems.Any())
                return;

            CreateOperation(new Copy(SelectedItems.Cast<Lyric>(), newTime, LyricItems));
        }

        public void Remove()
        {
            if (!SelectedItems.Any())
                return;

            CreateOperation(new Remove(SelectedItems.Cast<Lyric>(), LyricItems));
        }

        public void Move(TimeSpan time)
        {
            if (!SelectedItems.Any())
                return;

            CreateOperation(new Move(time, SelectedItems.Cast<Lyric>(), LyricItems));
        }

        public void Modify(string newContent)
        {
            if (!SelectedItems.Any())
                return;

            CreateOperation(new Modify(SelectedItems.Cast<Lyric>(), newContent, LyricItems));
        }

        public void Sort(IEnumerable<Lyric> items)
        {
            CreateOperation(new Sort(items, LyricItems));
        }

        private void LyricItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(IsLyricsItemAny));
        }

        private void UndoOperations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(CanUndo));
            if (_optionFile != null)
                LyricsFileNotifier.SendSaveRequest(_optionFile);
        }

        private void RedoOperations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(CanRedo));
        }

        private async void LyricsFileChanged(object sender, FileChangeEventArgs e)
        {
            if (e.File == null)
            {
                _optionFile = null;
                LyricItems.Clear();
                LyricsTags = new LyricsTagFactory().CreateTags();
                UndoOperations.Clear();
                RedoOperations.Clear();
                return;
            }

            _optionFile = e.File;
            var fileContent = await LyricsFileIO.ReadText(e.File);
            var lines = fileContent.Replace('\r', '\n').Split('\n').Select(l => l.Trim());
            var tuple = LyricsSerializer.Deserialization(lines);

            LyricItems.Clear();
            foreach (var lyric in tuple.lyrics)
                LyricItems.Add(lyric);

            LyricsTags = tuple.tags.ToList();

            if (UndoOperations.Any())
                UndoOperations.Clear();
            if (RedoOperations.Any())
                RedoOperations.Clear();
        }

        private async void LyricsFileSaveRequested(object sender, FileChangeEventArgs e)
        {
            var content = LyricsSerializer.Serialization(LyricItems,
                LyricsTags.Where(t => !string.IsNullOrWhiteSpace(t.TagValue)));
            await LyricsFileIO.WriteText(e.File, content);

            if (_optionFile != e.File)
            {
                _optionFile = e.File;
                if (_savedToastXmlDocument == null)
                {
                    StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Data/ToastNotification/Saved.xml"));
                    string xmlContent = await FileIO.ReadTextAsync(file);
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(xmlContent);
                    _savedToastXmlDocument = xdoc;
                }

                ToastNotification notification = new ToastNotification(_savedToastXmlDocument)
                {
                    ExpirationTime = DateTimeOffset.Now.AddSeconds(10)
                };
                ToastNotificationManager.CreateToastNotifier().Show(notification);
            }
        }
    }
}