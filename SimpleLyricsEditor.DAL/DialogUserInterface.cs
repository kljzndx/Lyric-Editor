namespace SimpleLyricsEditor.DAL
{
    public abstract class DialogUserInterface
    {
        protected DialogUserInterface(string title, string closeText)
        {
            Title = title;
            CloseText = closeText;
        }

        public string Title { get; }
        public string CloseText { get; }
    }
}