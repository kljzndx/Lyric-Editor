using System;
using System.Collections.Generic;
using Windows.Data.Json;

namespace SimpleLyricsEditor.DAL.Factory
{
    public class UpdateLogDeserializer
    {
        public IEnumerable<UpdateLog> Deserialization(string json)
        {
            List<UpdateLog> logs = new List<UpdateLog>();
            JsonArray datas = JsonArray.Parse(json);

            foreach (IJsonValue items in datas)
            {
                JsonObject data = items.GetObject();
                string version = data["Version"].GetString();
                string dateStr = data["Date"].GetString();
                DateTime date = DateTime.Parse(dateStr);
                string file = data["File"].GetString();

                logs.Add(new UpdateLog(version, date.ToString("D"), file));
            }

            return logs;
        }
    }
}