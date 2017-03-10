using LyricsEditor.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;

namespace LyricsEditor.Services
{
    public static class UpdateLogXmlService
    {
        public static async Task<(UpdateLog theVersionUpdateLog, ObservableCollection<UpdateLog> allUpdateLog)> GetUpdateLog()
        {
            ObservableCollection<UpdateLog> logs = new ObservableCollection<UpdateLog>();
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Datas/UpdateLog.xml"));

            XDocument dom = XDocument.Parse(await FileIO.ReadTextAsync(file));
            var x_Logs = dom.Descendants("UpdateLog");

            int id = 0;
            foreach (var x_log in x_Logs)
            {
                var log = new UpdateLog
                {
                    ID = id++,
                    Version = x_log.Element("Version").Value,
                    Content = x_log.Element("Content").Value.Trim(),
                    Date = x_log.Element("Date").Value
                };
                logs.Add(log);
            }

            return (logs[0], logs);
        }
    }
}
