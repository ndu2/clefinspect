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
            private readonly string? _textFilter;
            public Matcher(string? textFilter)
            {
                _textFilter = textFilter;
            }


            public bool Accept(ClefLine line)
            {
                if (_textFilter == null)
                {
                    return true;
                }
                if (_textFilter.Length == 0)
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
                string[] textFilters = _textFilter.Split(",");
                foreach (var dat in line.JsonObject)
                {
                    foreach (string textFilter in textFilters)
                    {
                        if (dat.Value?.ToString().Contains(textFilter, StringComparison.InvariantCultureIgnoreCase) ?? false)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        public bool AccceptsAll => (_textFilter == null || _textFilter.Length == 0);

        public IMatcher Create()
        {
            return new Matcher(_textFilter);
        }
    }

}
