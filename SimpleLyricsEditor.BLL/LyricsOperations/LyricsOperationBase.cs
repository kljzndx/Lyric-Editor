namespace SimpleLyricsEditor.BLL.LyricsOperations
{
    public abstract class LyricsOperationBase
    {
        public string Message { get; set; }

        public abstract void Do();
        public abstract void Undo();
    }
}