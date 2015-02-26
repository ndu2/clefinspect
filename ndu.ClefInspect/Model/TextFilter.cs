namespace ndu.ClefInspect.Model
{
    public class TextFilter(string? filterString) : IFilter
    {
        private readonly List<string> _textFilters = TextFilterParser.Parse(filterString);
        private readonly string? _filterString = filterString;

        public string? FilterString => _filterString;

        public class Matcher(List<string> textFilters) : IMatcher
        {
            private readonly List<string> _textFilters = textFilters;

            public bool Accept(ClefLine line)
            {
                if (_textFilters.Count == 0)
                {
                    return true;
                }
                if (line == null)
                {
                    return false;
                }
                if (line.JsonObject == null)
                {
                    return false;
                }
                foreach (string textFilter in _textFilters)
                {
                    if (line.Message?.Contains(textFilter, StringComparison.InvariantCultureIgnoreCase) ?? false)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public bool AcceptsAll => _textFilters == null || _textFilters.Count == 0 || _textFilters.All(f => { return f.Length == 0; });
        public bool AcceptsNone => false;

        public IMatcher Create()
        {
            if (AcceptsAll)
            {
                return new MatcherAcceptAll();
            }
            return new Matcher(_textFilters);
        }
    }

}
