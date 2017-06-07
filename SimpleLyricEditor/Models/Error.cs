using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLyricEditor.Models
{
    class Error
    {
        public int Code { get; set; }
        public string Source { get; set; }
        public string Content { get; set; }
        public string StackTrace { get; set; }

        public Error(Exception ex)
        {
            Code = ex.HResult;
            Source = ex.Source;
            Content = ex.Message;
            StackTrace = ex.StackTrace;
        }

        public override string ToString()
        {
            return $"错误码： 0x{Code.ToString("X")}\n崩溃源： {Source}\n错误信息： \"{Content}\"\n堆栈信息\n{StackTrace}";
        }
    }
}
