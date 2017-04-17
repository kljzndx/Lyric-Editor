using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLyricEditor.EventArgss
{
    public class PositionChangeEventArgs : EventArgs
    {
        public bool IsUserChange { get; set; } = false;
        public TimeSpan ThisPosition { get; set; } = TimeSpan.Zero;

        public PositionChangeEventArgs(bool isUserChange, TimeSpan thisPosition)
        {
            IsUserChange = isUserChange;
            ThisPosition = thisPosition;
        }
    }
}
