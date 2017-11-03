using System.Xml.Linq;

namespace SimpleLyricsEditor.DAL.Factory
{
    public class ShortcutKeysDialogUiDeserializer
    {
        public ShortcutKeysDialogUI Deserialization(string xml)
        {
            XDocument doc = XDocument.Parse(xml);
            XElement rootNode = doc.Root;
            XElement dialogUiElement = rootNode.Element("DialogUI");
            return new ShortcutKeysDialogUI
                (
                    dialogUiElement.Element("Title").Value.Trim(),
                    dialogUiElement.Element("CloseButtonText").Value.Trim(),
                    dialogUiElement.Element("ConditionTag").Value.Trim(),
                    dialogUiElement.Element("FunctionTag").Value.Trim()
                );
        }
    }
}