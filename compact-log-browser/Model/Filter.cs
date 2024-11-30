using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Nodes;

namespace compact_log_browser.Model
{
    public class Filter : INotifyPropertyChanged
    {
        private string _key;

        public Filter(string key, HashSet<string> filter)
        {
            _key = key;
            List<FilterValue> values = new List<FilterValue>();
            foreach (string value in filter)
            {
                FilterValue fi = new FilterValue(value, true);
                fi.PropertyChanged += (sender, e) => { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Values))); };
                values.Add(fi);
            }
            Values = new ObservableCollection<FilterValue>(values);
        }

        public bool Accept(JsonObject? line)
        {
            JsonNode? val = line?[_key];
            bool ok = Values.Any(f => f.Accepts(val));
            return ok;
        }

        public ObservableCollection<FilterValue> Values { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

}
