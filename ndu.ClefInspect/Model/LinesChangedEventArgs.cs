namespace ndu.ClefInspect.Model
{

    public class LinesChangedEventArgs(LinesChangedEventArgs.LinesChangedEventArgsAction change) : EventArgs
    {
        public enum LinesChangedEventArgsAction
        {
            None, Add, Reset
        }

        public LinesChangedEventArgsAction Action { get; } = change;

        public static LinesChangedEventArgsAction Union(LinesChangedEventArgsAction a, LinesChangedEventArgsAction b)
        {
            if (a == LinesChangedEventArgsAction.Reset || b == LinesChangedEventArgsAction.Reset)
            {
                return LinesChangedEventArgsAction.Reset;
            }
            if (a == LinesChangedEventArgsAction.Add || b == LinesChangedEventArgsAction.Add)
            {
                return LinesChangedEventArgsAction.Add;
            }
            return LinesChangedEventArgsAction.None;
        }
    }
}
