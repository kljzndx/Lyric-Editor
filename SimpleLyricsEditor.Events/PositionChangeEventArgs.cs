using System;

namespace SimpleLyricsEditor.Events
{
    public class PositionChangeEventArgs : EventArgs
    {
        public PositionChangeEventArgs(bool isUserChange, TimeSpan position)
        {
            IsUserChange = isUserChange;
            Position = position;
        }

        public bool IsUserChange { get; set; }
        public TimeSpan Position { get; set; }
    }
}