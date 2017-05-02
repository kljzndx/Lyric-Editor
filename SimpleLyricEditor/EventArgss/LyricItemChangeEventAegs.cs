using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLyricEditor.EventArgss
{
    public enum LyricItemOperationType { Add, Del, ChangeTime, ChangeContent, Refresh }
    public class LyricItemChangeEventAegs : EventArgs
    {
        public LyricItemOperationType ChangeType { get; private set; }

        public LyricItemChangeEventAegs(LyricItemOperationType changeType)
        {
            ChangeType = changeType;
        }
    }
}
