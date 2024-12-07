using System;

namespace clef_inspect.Model
{
    public class TextFilter : IFilter
    {
        private readonly string? _textFilter;
        public TextFilter(string? textFilter)
        {
            _textFilter = textFilter;
        }

        public class Matcher : IMatcher
        {
            private readonly string[] _textFilters;

            public Matcher(string textFilter)
            {
                _textFilters = textFilter.Split(",");
            }


            public bool Accept(ClefLine line)
            {
                if (_textFilters.Length == 0)
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
        public bool AccceptsAll => (_textFilter == null || _textFilter.Length == 0);

        public IMatcher Create()
        {
            if (_textFilter == null || _textFilter.Length == 0)
            {
                return new MatcherAcceptAll();
            }
            return new Matcher(_textFilter);
        }
    }

}
