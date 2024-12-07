using clef_inspect.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;

namespace clef_inspect.ViewModel.ClefView
{
    public partial class ClefViewModel : INotifyPropertyChanged
    {
        private ClefViewSettings _settings;
        private Dictionary<string, Filter> _filters;
        private FilterTaskManager _filterTaskManager;
        private string? _textFilter;
        private int _selectedIndex;
        private bool _calculationRunning;

        public ClefViewModel(string fileName, Settings settings)
        {
            _settings = new ClefViewSettings(settings);
            _filters = new Dictionary<string, Filter>();
            _filterTaskManager = new FilterTaskManager(this, _settings);
            _calculationRunning = false;
            settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(settings.LocalTime))
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DatePosition)));
            };
            Clef = new Clef(new FileInfo(fileName));
            ClefLines = new FilteredClef(_settings);
            Filters = new ObservableCollection<ClefFilterViewModel>();
            ClearTextFilter = new ClearTextFilterCommand(this);
            ApplyTextFilter = new ApplyTextFilterCommand(this);
            Clef.LinesChanged += Reload;
        }


        public bool CalculationRunning
        {
            get => _calculationRunning;
            set
            {
                if (value != _calculationRunning)
                {
                    _calculationRunning = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CalculationRunning)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action? Reloaded;

        public enum UserAction { Copy, CopyClef, Pin, Unpin };
        public delegate void UserActionEvent(UserAction userAction);
        public event UserActionEvent? UserActionHandler;
        public void DoUserAction(UserAction userAction)
        {
            UserActionHandler?.Invoke(userAction);
        }

        public void DoClose()
        {
            Clef.Dispose();
            Clef.LinesChanged -= Reload;
        }

        private void Reload(object? sender, LinesChangedEventArgs e)
        {
            if(e.Action != LinesChangedEventArgs.LinesChangedEventArgsAction.None)
            {
                Reload(e);
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DateInfo)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileInfo)));
        }


        private void Reload()
        {
            Reload(new LinesChangedEventArgs(LinesChangedEventArgs.LinesChangedEventArgsAction.Reset));
        }

        private void Reload(LinesChangedEventArgs e)
        {
            foreach (var p in Clef.Properties)
            {
                if (!_filters.ContainsKey(p.Key))
                {
                    Filter filter = new Filter(p.Key, p.Value.Item2);
                    filter.FilterChanged += Reload;
                    _filters.Add(p.Key, filter);
                    Filters.Add(new ClefFilterViewModel(p.Value.Item1, filter));
                }
                else
                {
                    _filters[p.Key].Update(p.Value.Item2);
                }
            }
            foreach (var p in Clef.Data)
            {
                if (!_filters.ContainsKey(p.Key))
                {
                    Filter filter = new Filter(p.Key, p.Value);
                    filter.FilterChanged += Reload;
                    _filters.Add(p.Key, filter);
                    Filters.Add(new ClefFilterViewModel(p.Key, filter));
                }
                else
                {
                    _filters[p.Key].Update(p.Value);
                }
            }

            List<IMatcher> matchers = CreateMatchers();
            CalculationRunning = true;

            _filterTaskManager.Filter(matchers, e.Action,
                 (selectedIndex) =>
                 {
                     SelectedIndex = selectedIndex;
                     Reloaded?.Invoke();
                     CalculationRunning = false;
                 });
        }

        private List<IMatcher> CreateMatchers()
        {
            TextFilter tf = new TextFilter(_textFilter);
            List<IMatcher> matchers = new List<IMatcher>();
            foreach (IFilter v in _filters.Values)
            {
                if (!v.AccceptsAll)
                {
                    matchers.Add(v.Create());
                }
            }
            if (!tf.AccceptsAll)
            {
                matchers.Add(tf.Create());
            }

            return matchers;
        }

        public Clef Clef { get; }
        public string FilePath => Clef.FilePath;
        public string FileInfo
        {
            get
            {
                return $"Log Entries displayed: {ClefLines.Count} (Total {Clef.Count}, Size {_settings.FormatFileSize(Clef.SeekPos)})";
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

        public ClefLineView? SelectedItem
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
