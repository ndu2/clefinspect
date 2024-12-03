using clef_inspect.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json.Nodes;
using System.Windows.Input;

namespace clef_inspect.ViewModel.ClefView
{
    public partial class ClefViewModel : INotifyPropertyChanged
    {
        private ClefViewSettings _settings;
        private Dictionary<string, Filter> _filters;
        private string? _textFilter;
        private int _selectedIndex;

        public ClefViewModel(string fileName, Settings settings)
        {
            
            _settings = new ClefViewSettings(settings);
            settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(settings.LocalTime))
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DatePosition)));
            };

            _filters = new Dictionary<string, Filter>();
            Clef = new Clef(new FileInfo(fileName));
            ClefLines = new FilteredClef(_settings);
            Filters = new ObservableCollection<ClefFilterViewModel>();
            ClearTextFilter = new ClearTextFilterCommand(this);
            ApplyTextFilter = new ApplyTextFilterCommand(this);
            Clef.PropertyChanged += Reload;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action? Reloaded;

        public void DoClose()
        {
            Clef.Dispose();
            Clef.PropertyChanged -= Reload;
        }

        private void Reload(object? sender, PropertyChangedEventArgs e)
        {
            Reload();
        }


        private void Reload()
        {
            foreach (var p in Clef.Properties)
            {
                if (!_filters.ContainsKey(p.Key))
                {
                    Filter filter = new Filter(p.Key, p.Value.Item2.Keys);
                    filter.PropertyChanged += (sender, e) => { Reload(); };
                    _filters.Add(p.Key, filter);
                    Filters.Add(new ClefFilterViewModel(p.Value.Item1, filter));
                }
                else
                {
                    _filters[p.Key].Update(p.Value.Item2.Keys);
                }
            }
            foreach (var p in Clef.Data)
            {
                if (!_filters.ContainsKey(p.Key))
                {
                    Filter filter = new Filter(p.Key, p.Value.Keys);
                    filter.PropertyChanged += (sender, e) => { Reload(); };
                    _filters.Add(p.Key, filter);
                    Filters.Add(new ClefFilterViewModel(p.Key, filter));
                }
                else
                {
                    _filters[p.Key].Update(p.Value.Keys);
                }
            }

            // View uses UI virtualization, so we can add everything to ClefLines
            // Unfortunately ObservableCollection supports only adding one line at a
            // time (triggering a lot of layouting stuff)
            int selectedIndex = SelectedIndex;
            ClefLines.Reload(Clef, _filters.Values, TextFilterOk, ref selectedIndex);
            SelectedIndex = selectedIndex;
            Reloaded?.Invoke();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClefLines)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DateInfo)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileInfo)));
        }

        private bool TextFilterOk(JsonObject? line)
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
            string[] textFilters = _textFilter.Split(",");
            foreach (var dat in line)
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

        public Clef Clef { get; }
        public string FilePath => Clef.FilePath;
        public string FileInfo
        {
            get
            {
                return $"Log Entries displayed: {ClefLines.Count} (Total {Clef.Lines.Count}, Size {_settings.FormatFileSize(Clef.SeekPos)})";
            }
        }
        public string DateInfo
        {
            get
            {
                string? from = ClefLines.FirstOrDefault()?.Time;
                string? to = ClefLines.LastOrDefault()?.Time;
                if (from != null && to != null)
                {
                    return $"Range from {from} to {to}";
                }
                else
                {
                    return "";
                }
            }
        }
        public FilteredClef ClefLines { get; set; }

        public bool AutoUpdate { get; set; }
        public ObservableCollection<ClefFilterViewModel> Filters { get; set; }

        public string? TextFilter
        {
            get => _textFilter;
            set
            {
                if (value != _textFilter)
                {
                    _textFilter = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextFilter)));
                }
            }
        }

        public ICommand ClearTextFilter { get; set; }

        public ICommand ApplyTextFilter { get; set; }

        public ClefLine? SelectedItem
        {
            get
            {
                if(SelectedIndex < 0 || SelectedIndex >= ClefLines.Count)
                {
                    return null;
                }
                return ClefLines[SelectedIndex];
            }
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    _settings.RefTimeStamp = ClefLines.ElementAtOrDefault(_selectedIndex)?.GetTime();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedIndex)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DatePosition)));
                }
            }
        }

        public string? DatePosition
        {
            get
            {
                return _settings.Settings.Format(_settings.RefTimeStamp);
            }
            set
            {
                if (value == null)
                {
                    _settings.RefTimeStamp = null;
                }
                else
                {
                    DateTime dt = DateTime.Parse(value);
                    _settings.RefTimeStamp = dt;

                    int i = 0;
                    for (; i < ClefLines.Count; i++)
                    {
                        if (ClefLines[i].GetTime() > dt)
                        {
                            break; ;
                        }
                    }
                    _selectedIndex = i - 1;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedIndex)));
                    Reloaded?.Invoke();
                }
            }
        }

    }
}
