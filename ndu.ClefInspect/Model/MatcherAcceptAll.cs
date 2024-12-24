namespace ndu.ClefInspect.Model
{
    public class MatcherAcceptAll : IMatcher
    {
        public bool Accept(ClefLine line) => true;
    }
}
