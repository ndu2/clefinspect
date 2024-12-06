namespace clef_inspect.Model
{
    public class LinesChangedEventArgs : EventArgs
    {
        public enum LinesChangedEventArgsAction
        {
            None, Add, Reset
        }
        public LinesChangedEventArgs(LinesChangedEventArgsAction change)
        {
            Action = change;
        }
        public LinesChangedEventArgsAction Action { get; }
    }
}
