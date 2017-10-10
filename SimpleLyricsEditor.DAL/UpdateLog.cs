using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleLyricsEditor.DAL
{
    public class UpdateLog : INotifyPropertyChanged
    {
        private string _content;

        public UpdateLog(string version, string date, string fileName)
        {
            Version = version;
            Date = date;
            FileName = fileName;
        }

        public string Version { get; }
        public string Date { get; }
        public string FileName { get; }

        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}