using System.ComponentModel;
using System.Text.Json.Nodes;
using System.Windows.Navigation;

namespace compact_log_browser.Model
{
    public class FilterValue : INotifyPropertyChanged
    {
        private bool _enabled;
        private bool _defaultFilter;
        public FilterValue(string value, bool enabled)
        {
            _defaultFilter = value == "";
            Value = _defaultFilter ? "<N/A>" : value;
            Enabled = enabled;
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Value { get; }
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

        public bool Accepts(JsonNode? val)
        {
            if (!_enabled)
                return false;

            string? value = val?.ToString();
            if (_defaultFilter && (value == null || value.Length == 0))
                return true;
            if (value == null) return false;
            return value == Value;
        }
    }
}
