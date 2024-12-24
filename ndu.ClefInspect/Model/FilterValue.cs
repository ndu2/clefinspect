using System.ComponentModel;

namespace ndu.ClefInspect.Model
{
    public class FilterValue : INotifyPropertyChanged, IComparable<FilterValue>
    {
        private bool _enabled;
        private int _amount;
        public FilterValue(string value, int amount, bool enabled, string whenEmpty)
        {
            Value = value.Length == 0 ? whenEmpty : value;
            ValueMatcher = value;
            _amount = amount;
            Enabled = enabled;
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        public string Value { get; }
        public string ValueMatcher { get; }
        public int Amount
        {
            get => _amount;
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Amount)));
                }
            }
        }
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
            return ValueMatcher.CompareTo(other?.ValueMatcher);
        }
    }
}
