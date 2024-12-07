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

        public static LinesChangedEventArgsAction Union(LinesChangedEventArgsAction a, LinesChangedEventArgsAction b)
        {
            if(a == LinesChangedEventArgsAction.Reset || b == LinesChangedEventArgsAction.Reset)
            {
                return LinesChangedEventArgsAction.Reset;
            }
            if (a == LinesChangedEventArgsAction.Add|| b == LinesChangedEventArgsAction.Add)
            {
                return LinesChangedEventArgsAction.Add;
            }
            return LinesChangedEventArgsAction.None;
        }
    }
}
