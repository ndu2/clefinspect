using compact_log_browser.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Nodes;
using System.Windows.Input;

namespace compact_log_browser.ViewModel.ClefView
{
    public partial class ClefViewModel : INotifyPropertyChanged
    {
        private ClefViewSettings _settings;
        private List<Filter> _filters;
        private string? _textFilter;
        private int _selectedIndex;

        public ClefViewModel(Clef clef, Settings settings)
        {
            _settings = new ClefViewSettings(settings);
            settings.PropertyChanged += (s, e) => {
                if(e.PropertyName == nameof(settings.LocalTime))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DatePosition)));
            };

            _filters = new List<Filter>();
            Clef = clef;
            ClefLines = new FilteredClef(_settings);
            Filters = new ObservableCollection<ClefFilterViewModel>();
            ClearTextFilter = new ClearTextFilterCommand(this);
            ApplyTextFilter = new ApplyTextFilterCommand(this);
            foreach (var p in clef.Properties)
            {
                Filter filter = new Filter(p.Key, p.Value.Item2);
                _filters.Add(filter);
                Filters.Add(new ClefFilterViewModel(p.Value.Item1, filter));
            }
            foreach (var p in clef.Data)
            {
                Filter filter = new Filter(p.Key, p.Value);
                _filters.Add(filter);
                Filters.Add(new ClefFilterViewModel(p.Key, filter));
            }
            foreach (Filter filter in _filters)
            {
                filter.PropertyChanged += (sender, e) => { Reload(); };
            }
            Reload();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action? Reloaded;
        private void Reload()
        {
            // View uses UI virtualization, so we can add everything to ClefLines
            // Unfortunately ObservableCollection supports only adding one line at a
            // time (triggering a lot of layouting stuff)
            int selectedIndex = SelectedIndex;
            ClefLines.Reload(Clef, _filters, TextFilterOk, ref selectedIndex);

            SelectedIndex = selectedIndex;
            Reloaded?.Invoke();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClefLines)));
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
                return $"Log Entries: {ClefLines.Count}";
            }
        }
        public string DateInfo
        {
            get
            {
                
                return $"Range from {ClefLines.FirstOrDefault()?.Time} to {ClefLines.LastOrDefault()?.Time}";
            }
        }
        public FilteredClef ClefLines { get; set; }


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

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    _settings.SetRefTimeStamp(ClefLines.ElementAtOrDefault(_selectedIndex));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedIndex)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DatePosition)));
                }
            }
        }

        public string? DatePosition
        {
            get
            {
                return ClefLines.ElementAtOrDefault(SelectedIndex)?.Time;
            }
            set
            {
                return;
            }
        }

    }
}
