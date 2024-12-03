using System.ComponentModel;

namespace compact_log_browser.Model
{
    public class FilterValue : INotifyPropertyChanged, IComparable<FilterValue>
    {
        private bool _enabled;
        public FilterValue(string value, bool enabled)
        {
            Value = value.Length ==0? "(empty)" : value;
            ValueMatcher = value;
            Enabled = enabled;
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Value { get; }
        public string ValueMatcher { get; }
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (value != _enabled)
                {
                    _enabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Enabled)));
                }
            }
        }

        public int CompareTo(FilterValue? other)
        {
            return Value.CompareTo(other?.Value);
        }
    }
}
