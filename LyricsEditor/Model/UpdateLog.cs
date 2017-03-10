using LyricsEditor.Auxiliary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsEditor.Model
{
    public class UpdateLog : BindableBase
    {
        private int id;
        public int ID { get => id; set => SetProperty(ref id, value); }

        private string version;
        public string Version { get => version; set => SetProperty(ref version, value); }

        private string content;
        public string Content { get => content; set => SetProperty(ref content, value); }

        private string date;
        public string Date { get => date; set => SetProperty(ref date, value); }
    }

}
