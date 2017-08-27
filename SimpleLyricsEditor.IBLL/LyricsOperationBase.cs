namespace SimpleLyricsEditor.IBLL
{
    public abstract class LyricsOperationBase
    {
        public string Message { get; set; }

        public abstract void Do();
        public abstract void Undo();
    }
}