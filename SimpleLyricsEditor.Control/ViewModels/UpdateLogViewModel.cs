using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using SimpleLyricsEditor.BLL;
using SimpleLyricsEditor.DAL;
using SimpleLyricsEditor.DAL.Factory;

namespace SimpleLyricsEditor.Control.ViewModels
{
    public class UpdateLogViewModel : ViewModelBase
    {
        private readonly UpdateLogFilesReader _filesReader = new UpdateLogFilesReader();

        private UpdateLogDialogUI _dialogUi;

        private List<UpdateLog> _allLogs;

        private UpdateLog _currentUpdateLog;

        public UpdateLogDialogUI DialogUi
        {
            get => _dialogUi;
            set => Set(ref _dialogUi, value);
        }

        public List<UpdateLog> AllLogs
        {
            get => _allLogs;
            set => Set(ref _allLogs, value);
        }

        public UpdateLog CurrentUpdateLog
        {
            get => _currentUpdateLog;
            set => Set(ref _currentUpdateLog, value);
        }

        public async Task GetDialogUIAsync()
        {
            string xml = await _filesReader.ReadDialogUI();
            DialogUi = new UpdateLogDialogUiDeserializer().Deserialization(xml);
        }

        public async Task GetAllLogsAsync()
        {
            string json = await _filesReader.GetAllLogsJson();
            AllLogs = new UpdateLogDeserializer().Deserialization(json).ToList();
        }

        public async Task ReadLogContentAsync()
        {
            if (String.IsNullOrEmpty(CurrentUpdateLog.Content))
                CurrentUpdateLog.Content = await _filesReader.GetLogContent(CurrentUpdateLog.FileName);
        }
    }
}