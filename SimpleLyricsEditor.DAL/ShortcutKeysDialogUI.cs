namespace SimpleLyricsEditor.DAL
{
    public class ShortcutKeysDialogUI : DialogUserInterface
    {
        public ShortcutKeysDialogUI(string title, string closeText, string conditionTag, string functionTag) : base(title, closeText)
        {
            ConditionTag = conditionTag;
            FunctionTag = functionTag;
        }

        public string ConditionTag { get; }
        public string FunctionTag { get; }
    }
}