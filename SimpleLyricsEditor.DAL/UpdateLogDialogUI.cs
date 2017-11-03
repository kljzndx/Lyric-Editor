namespace SimpleLyricsEditor.DAL
{
    public class UpdateLogDialogUI : DialogUserInterface
    {
        public UpdateLogDialogUI(string title, string closeText, string informationTag, string allVersionsTag) : base(title, closeText)
        {
            InformationTag = informationTag;
            AllVersionsTag = allVersionsTag;
        }

        public string InformationTag { get; set; }
        public string AllVersionsTag { get; set; }
    }
}