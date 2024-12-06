using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Nodes;

namespace clef_inspect.Model
{
    public class Filter
    {
        private string _key;
        private HashSet<string>? _enabledValues;
        private bool _holdBackUpdateFilterSet;
        public Filter(string key, IEnumerable<KeyValuePair<string, int>> filter)
        {
            _holdBackUpdateFilterSet = false;
            _key = key;
            List<FilterValue> values = new List<FilterValue>();
            foreach ((string value, int amount) in filter)
            {
                FilterValue fi = new FilterValue(value, amount, true);
                fi.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == nameof(fi.Enabled))
                    {
                        UpdateFilterSet();
                    }
                };
                values.Add(fi);
            }
            values.Sort();
            Values = new ObservableCollection<FilterValue>(values);
        }

        private void UpdateFilterSet()
        {
            if (_holdBackUpdateFilterSet)
            {
                return;
            }
            bool allEnabled = true;
            if(_enabledValues == null)
            {
                _enabledValues = new HashSet<string>();
            }
            foreach (FilterValue fi in Values)
            {
                if (fi.Enabled)
                {
                    _enabledValues.Add(fi.ValueMatcher);
                }
                else
                {
                    _enabledValues.Remove(fi.ValueMatcher);
                    allEnabled = false;
                }
            }
            if (allEnabled)
            {
                _enabledValues = null;
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
        public bool AllEnabled => (_enabledValues == null);

        public bool Accept(ClefLine line)
        {
            if(_enabledValues == null)
            {
                return true;
            }
            string val = line.JsonObject?[_key]?.ToString() ?? "";
            return _enabledValues.Contains(val);
        }

        public void CheckAll()
        {
            _holdBackUpdateFilterSet = true;
            try
            {
                foreach (var item in Values)
                {
                    item.Enabled = true;
                }
            }
            finally
            {
                _holdBackUpdateFilterSet = false;
                UpdateFilterSet();
            }
        }

        public void UncheckAll()
        {
            _holdBackUpdateFilterSet = true;
            try
            {
                foreach (var item in Values)
                {
                    item.Enabled = false;
                }
            }
            finally
            {
                _holdBackUpdateFilterSet = false;
                UpdateFilterSet();
            }
        }

        public ObservableCollection<FilterValue> Values { get; }

        public event Action? FilterChanged;
    }

}
