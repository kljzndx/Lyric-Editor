namespace SimpleLyricsEditor.DAL
{
    public class ShortcutKey
    {
        public ShortcutKey(string condition, string function)
        {
            Condition = condition;
            Function = function;
        }

        public string Condition { get; set; }
        public string Function { get; set; }
    }
}