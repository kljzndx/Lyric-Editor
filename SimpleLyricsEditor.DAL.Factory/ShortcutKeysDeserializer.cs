using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Windows.Data.Xml.Dom;

namespace SimpleLyricsEditor.DAL.Factory
{
    public class ShortcutKeysDeserializer
    {
        public IDictionary<string, IEnumerable<ShortcutKey>> Deserialization(string xml)
        {
            var result = new Dictionary<string, IEnumerable<ShortcutKey>>();
            List<ShortcutKey> currentClassList = new List<ShortcutKey>();
            XDocument xd = XDocument.Parse(xml);
            var allKeys = xd.Element("ShortcutKeys");
            var keys = allKeys.Elements("ShortcutKey");
            string currentClassName = String.Empty;

            foreach (XElement key in keys)
            {
                string className = key.Attribute("Class").Value.Trim();
                string condition = key.Element("Condition").Value.Trim();
                string function = key.Element("Function").Value.Trim();
                
                if (!className.Equals(currentClassName))
                {
                    if (!String.IsNullOrEmpty(currentClassName))
                        result[currentClassName] = currentClassList;
                    currentClassName = className;
                    currentClassList = new List<ShortcutKey>();

                    result.Add(currentClassName, null);
                }
                
                currentClassList.Add(new ShortcutKey(condition, function));
            }

            result[currentClassName] = currentClassList;
            return result;
        }
    }
}