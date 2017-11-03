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
                rootNode.Element("Title").Value.Trim(),
                rootNode.Element("CloseButtonText").Value.Trim(),
                rootNode.Element("InformationTag").Value.Trim(),
                rootNode.Element("AllVersionsTag").Value.Trim()
            );
        }
    }
}