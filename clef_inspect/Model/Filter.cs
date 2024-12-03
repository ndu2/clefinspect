using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Nodes;

namespace clef_inspect.Model
{
    public class Filter : INotifyPropertyChanged
    {
        private string _key;
        private HashSet<string>? _enabledValues;
        public Filter(string key, IEnumerable<string> filter)
        {
            _key = key;
            List<FilterValue> values = new List<FilterValue>();
            foreach (string value in filter)
            {
                FilterValue fi = new FilterValue(value, true);
                fi.PropertyChanged += (sender, e) => UpdateFilterSet();
                fi.PropertyChanged += (sender, e) => { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Values))); };
                values.Add(fi);
            }
            values.Sort();
            Values = new ObservableCollection<FilterValue>(values);
        }

        private void UpdateFilterSet()
        {
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
        }
        public void Update(ICollection<string> values)
        {
            foreach(string value in values)
            {
                if(!Values.Any(e=> e.ValueMatcher == value))
                {
                    Values.Add(new FilterValue(value, true));
                }
            }
        }

        public bool Accept(JsonObject? line)
        {
            if(_enabledValues == null)
            {
                return true;
            }
            string val = line?[_key]?.ToString() ?? "";
            return _enabledValues.Contains(val);
        }


        public ObservableCollection<FilterValue> Values { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

}
