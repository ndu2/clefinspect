using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ndu.ClefInspect.Model
{
    public class Filter : IFilter, INotifyPropertyChanged
    {
        private readonly string _key;
        private bool _disableNotifyFilterChanged;
        // Values and _values store the same objects, the former allows VM to bind to, the latter is used for fast retrieval by the filter value
        private Dictionary<string, FilterValue> _values = [];

        public event PropertyChangedEventHandler? PropertyChanged;

        public Filter(string key, IEnumerable<KeyValuePair<string, int>> filter)
        {
            _disableNotifyFilterChanged = false;
            _key = key;
            List<FilterValue> values = [];
            foreach ((string value, int amount) in filter)
            {
                Add(values, value, amount);
            }
            values.Sort();
            Values = new ObservableCollection<FilterValue>(values);
        }

        private void Add(IList<FilterValue> values, string value, int amount)
        {
            if (_values.TryGetValue(value, out var filter))
            {
                filter.Amount = amount;
            }
            else
            {
                FilterValue fi = new(value, amount, true, UiWhenEmpty());
                fi.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == nameof(fi.Enabled))
                    {
                        NotifyFilterChanged();
                    }
                };
                values.Add(fi);
                _values.Add(value, fi);
            }
        }

        private string UiWhenEmpty()
        {
            if (_key.Equals(ClefSchema.LEVEL_KEY))
            {
                return ClefSchema.LEVEL_EMPTY;
            }
            else
            {
                return "(empty)";
            }
        }

        public class Matcher(string key, HashSet<string> enabledValues) : IMatcher
        {
            private readonly string _key = key;
            private readonly HashSet<string> _enabledValues = enabledValues;

            public bool Accept(ClefLine line)
            {
                string val = line.JsonObject?[_key]?.ToString() ?? "";
                return _enabledValues.Contains(val);
            }
        }
        public bool AcceptsAll => Values.All((f) => f.Enabled);
        public bool AcceptsNone => Values.All((f) => !f.Enabled);

        public IMatcher Create()
        {
            bool allEnabled = true;
            HashSet<string> enabledValues = [];
            foreach (FilterValue fi in Values)
            {
                if (fi.Enabled)
                {
                    enabledValues.Add(fi.ValueMatcher);
                }
                else
                {
                    enabledValues.Remove(fi.ValueMatcher);
                    allEnabled = false;
                }
            }
            if (allEnabled)
            {
                return new MatcherAcceptAll();
            }
            else
            {
                return new Matcher(_key, enabledValues);
            }
        }

        private void NotifyFilterChanged()
        {
            if (_disableNotifyFilterChanged)
            {
                return;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AcceptsAll)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AcceptsNone)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Values)));
        }
        public void Update(IEnumerable<KeyValuePair<string, int>> values)
        {
            foreach ((string value, int amount) in values)
            {
                Add(Values, value, amount);
            }
        }

        public void CheckAll()
        {
            _disableNotifyFilterChanged = true;
            try
            {
                foreach (var item in Values)
                {
                    item.Enabled = true;
                }
            }
            finally
            {
                _disableNotifyFilterChanged = false;
                NotifyFilterChanged();
            }
        }

        public void UncheckAll()
        {
            _disableNotifyFilterChanged = true;
            try
            {
                foreach (var item in Values)
                {
                    item.Enabled = false;
                }
            }
            finally
            {
                _disableNotifyFilterChanged = false;
                NotifyFilterChanged();
            }
        }

        public ObservableCollection<FilterValue> Values { get; }
    }

}
