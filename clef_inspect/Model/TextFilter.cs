using System;
using System.Text;

namespace clef_inspect.Model
{
    public class TextFilter : IFilter
    {
        private readonly List<string> _textFilters;
        private readonly string? _filterString;

        public TextFilter(string? filterString)
        {
            _textFilters = TextFilterParser.Parse(filterString);
            _filterString = filterString;
        }
        public string? FilterString => _filterString;

        public class Matcher : IMatcher
        {
            private readonly List<string> _textFilters;

            public Matcher(List<string> textFilters)
            {
                _textFilters = textFilters;
            }


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
        public bool AccceptsAll => _textFilters == null || _textFilters.Count == 0 || _textFilters.All(f => { return f.Length == 0; });

        public IMatcher Create()
        {
            if (AccceptsAll)
            {
                return new MatcherAcceptAll();
            }
            return new Matcher(_textFilters);
        }
    }

}
