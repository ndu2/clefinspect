﻿using System.Collections.ObjectModel;

namespace clef_inspect.Model
{
    public class Filter : IFilter
    {
        private string _key;
        private bool _disableNotifyFilterChanged;
        public Filter(string key, IEnumerable<KeyValuePair<string, int>> filter)
        {
            _disableNotifyFilterChanged = false;
            _key = key;
            List<FilterValue> values = new List<FilterValue>();
            foreach ((string value, int amount) in filter)
            {
                FilterValue fi = new FilterValue(value, amount, true);
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
        public class MatcherAcceptAll : IMatcher
        {
            public bool Accept(ClefLine line) => true;
        }
        public bool AccceptsAll => Values.All((f) => f.Enabled);

        public IMatcher Create()
        {
            bool allEnabled = true;
            HashSet<string> enabledValues = new HashSet<string>();
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
            FilterChanged?.Invoke();
        }
        public void Update(IEnumerable<KeyValuePair<string, int>> values)
        {
            foreach ((string value, int amount) in values)
            {
                FilterValue? filterValue = Values.FirstOrDefault((f) => (f.ValueMatcher == value));
                if (filterValue == null)
                {
                    FilterValue newFilterValue = new FilterValue(value, amount, true);
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

        public event Action? FilterChanged;
    }

}
