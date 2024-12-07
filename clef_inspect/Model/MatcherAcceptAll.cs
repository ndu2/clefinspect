namespace clef_inspect.Model
{
        public class MatcherAcceptAll : IMatcher
        {
            public bool Accept(ClefLine line) => true;
        }
}
