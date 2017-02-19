using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsEditor.Model
{
    class Error
    {
        public string Code { get; set; }
        public string Source { get; set; }
        public string Content { get; set; }
        public string StackTrace { get; set; }

        public override string ToString()
        {
            return $"错误码： {Code} ； 崩溃源： {Source} ； 错误信息： {Content} ； 堆栈信息： {StackTrace}";
        }
    }
}
