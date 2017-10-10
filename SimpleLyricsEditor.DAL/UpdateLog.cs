using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleLyricsEditor.DAL
{
    public class UpdateLog : INotifyPropertyChanged
    {
        private static readonly Uri AllLogsFolderUri = new Uri("ms-appx:///Data/UpdateLogs/");
        public static readonly Uri AllLogsFileUri = new Uri(AllLogsFolderUri, "AllLogsPath.json");
        
        private string _content;

        public UpdateLog(string version, string date, string file)
        {
            Version = version;
            Date = date;
            Path = new Uri(AllLogsFolderUri, file);
        }

        public string Version { get; }
        public string Date { get; }
        public Uri Path { get; }

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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}