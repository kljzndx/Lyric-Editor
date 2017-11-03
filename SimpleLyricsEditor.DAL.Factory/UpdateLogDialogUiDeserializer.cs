using System.Xml.Linq;

namespace SimpleLyricsEditor.DAL.Factory
{
    public class UpdateLogDialogUiDeserializer
    {
        public UpdateLogDialogUI Deserialization(string xml)
        {
            XDocument doc = XDocument.Parse(xml);
            XElement rootNode = doc.Root;
            return new UpdateLogDialogUI
            (
                rootNode.Element("Title").Value,
                rootNode.Element("CloseButtonText").Value,
                rootNode.Element("InformationTag").Value,
                rootNode.Element("AllVersionsTag").Value
            );
        }
    }
}