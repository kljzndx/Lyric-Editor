using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleLyricsEditor.IDAL
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (field.Equals(value))
                return;

            OnPropertyChanged(propertyName);
        }
    }
}