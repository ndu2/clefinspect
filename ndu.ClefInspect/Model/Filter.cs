using ndu.ClefInspect.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ndu.ClefInspect.Model
{
    public class Filter : IFilter, INotifyPropertyChanged
    {
        private readonly string _key;
        private bool _disableNotifyFilterChanged;

        public event PropertyChangedEventHandler? PropertyChanged;


        public Filter(string key, IEnumerable<KeyValuePair<string, int>> filter)
        {
            _disableNotifyFilterChanged = false;
            _key = key;
            List<FilterValue> values = new();
            foreach ((string value, int amount) in filter)
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
            }
            values.Sort();
            Values = new ObservableCollection<FilterValue>(values);
        }


        private string UiWhenEmpty()
        {
            if (_key.Equals(Clef.LEVEL_KEY))
            {
                return Clef.LEVEL_EMPTY;
            }
            else
            {
                return "(empty)";
            }
        }

        public class Matcher : IMatcher
        {
            private readonly string _key;
            private readonly HashSet<string> _enabledValues;

            public Matcher(string key, HashSet<string> enabledValues)
            {
                _key = key;
                _enabledValues = enabledValues;
            }

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
            HashSet<string> enabledValues = new();
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
                FilterValue? filterValue = Values.FirstOrDefault((f) => f.ValueMatcher == value);
                if (filterValue == null)
                {
                    FilterValue newFilterValue = new(value, amount, true, UiWhenEmpty());
                    int pos = 0;
                    foreach (FilterValue fi in Values)
                    {
                        if (fi.CompareTo(newFilterValue) > 0)
                        {
                            break;
                        }
                        pos++;
                    }
                    Values.Insert(pos, newFilterValue);
                }
                else
                {
                    filterValue.Amount = amount;
                }
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
