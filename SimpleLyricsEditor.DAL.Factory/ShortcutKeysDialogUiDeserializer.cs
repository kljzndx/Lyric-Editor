using System.Xml.Linq;

namespace SimpleLyricsEditor.DAL.Factory
{
    public class ShortcutKeysDialogUiDeserializer
    {
        public ShortcutKeysDialogUI Deserialization(string xml)
        {
            XDocument doc = XDocument.Parse(xml);
            XElement dialogUiElement = doc.Element("DialogUi");
            return new ShortcutKeysDialogUI
                (
                    dialogUiElement.Element("Title").Value,
                    dialogUiElement.Element("CloseButtonText").Value,
                    dialogUiElement.Element("ConditionTag").Value,
                    dialogUiElement.Element("FunctionTag").Value
                );
        }
    }
}