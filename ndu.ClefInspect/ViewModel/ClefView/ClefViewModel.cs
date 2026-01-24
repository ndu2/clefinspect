using ndu.ClefInspect.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace ndu.ClefInspect.ViewModel.ClefView
{
    public partial class ClefViewModel : INotifyPropertyChanged
    {
        private readonly ClefViewSettings _settings;

        public ReadOnlyCollection<PinPreset> PinPresets => Clef.PinPresets;

        private readonly Dictionary<string, Filter> _filters;
        private readonly ObservableCollection<DataColumnView> _dataColumns;
        private readonly FilterTaskManager _filterTaskManager;
        private TextFilter? _textFilter;
        private int _selectedIndex;
        private bool _calculationRunning;
        private GridLength _colWidthEvtList;
        private GridLength _colWidthDetails;

        public ClefViewModel(string fileName, MainViewSettings settings)
        {
            _settings = new ClefViewSettings(settings);

            List<PinPreset> pinPresets = [];
            foreach(Configuration.PinPresetOptions p in _settings.SessionSettings.UserSettings.PinPresets)
            {
                PinPreset pp = new(p);
                PropertyChangedEventManager.AddHandler(pp, (s, e) => Reload(new LinesChangedEventArgs(LinesChangedEventArgs.LinesChangedEventArgsAction.Reset), true), nameof(pp.Enabled));
                pinPresets.Add(pp);
            }
            _filters = [];
            _dataColumns = [];
            _filterTaskManager = new FilterTaskManager(this, _settings);
            _calculationRunning = false;
            DetailViewInit();
            PropertyChangedEventManager.AddHandler(_settings.SessionSettings, OnSessionSettingsLocalTimeChanged, nameof(_settings.SessionSettings.LocalTime));
            PropertyChangedEventManager.AddHandler(_settings.SessionSettings, OnSessionSettingsOneLineOnlyChanged, nameof(_settings.SessionSettings.OneLineOnly));
            PropertyChangedEventManager.AddHandler(_settings.SessionSettings, OnSessionSettingsDetailViewChanged, nameof(_settings.SessionSettings.DetailView));
            PropertyChangedEventManager.AddHandler(_settings.SessionSettings, OnSessionSettingsDetailViewChanged, nameof(_settings.SessionSettings.DetailViewFraction));
            PropertyChangedEventManager.AddHandler(_settings, OnViewSettingsRefTimeStampChanged, nameof(_settings.RefTimeStamp));
            Clef = Clef.Create(fileName, new ReadOnlyCollection<PinPreset>(pinPresets));
            ClefLines = new FilteredClef(_settings);
            Filters = [];
            ClearTextFilter = new ClearTextFilterCommand(this);
            ApplyTextFilter = new ApplyTextFilterCommand(this);
            FiltersMenu = new FiltersMenuCommand(this);
            Clef.LinesChanged += Reload;
        }

        public ClefViewSettings Settings => _settings;

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

        private void OnSessionSettingsLocalTimeChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DatePosition)));
            foreach (ClefLineViewModel lvm in ClefLines)
            {
                lvm.NotifySettingsLocalTimeChanged();
            }
        }

        private void OnSessionSettingsOneLineOnlyChanged(object? sender, PropertyChangedEventArgs e)
        {
            foreach (ClefLineViewModel lvm in ClefLines)
            {
                lvm.NotifySettingsOneLineOnlyChanged();
            }
        }

        private void OnViewSettingsRefTimeStampChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DatePosition)));
            foreach (ClefLineViewModel lvm in ClefLines)
            {
                lvm.NotifySettingsRefTimeStampChanged();
            }
        }
        private void OnSessionSettingsDetailViewChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnSessionSettingsDetailViewChanged();
        }

        private void OnSessionSettingsDetailViewChanged()
        {
            DetailViewInit();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DetailsVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColWidthEvtList)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColWidthDetails)));
        }

        private void DetailViewInit()
        {
            double eventSize = _settings.SessionSettings.DetailView ? (1.0 - _settings.SessionSettings.DetailViewFraction) : 1.0;
            _colWidthEvtList = new GridLength(eventSize, GridUnitType.Star);
            _colWidthDetails = new GridLength(1.0 - eventSize, GridUnitType.Star);
        }
        public void DetailViewUpdate(double value)
        {
            if (value < 0.1)
            {
                if (_settings.SessionSettings.DetailView)
                {
                    _settings.SessionSettings.DetailView = false;
                }
                else if(value > 0.0)
                {
                    // completely close details view
                    OnSessionSettingsDetailViewChanged();
                }
            }
            else
            {
                if (value > 1.0) value = 1.0;
                _settings.SessionSettings.DetailView = true;
                _settings.SessionSettings.DetailViewFraction = value;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action? Reloaded;
        public event Action? DataColumnEnabledChanged;
        public void NotifyDataColumnEnabledChanged()
        {
            DataColumnEnabledChanged?.Invoke();
        }
        public enum UserAction { Copy, CopyClef, Pin, Unpin };
        public delegate void UserActionEvent(UserAction userAction, object? parameter);
        public event UserActionEvent? UserActionHandler;
        public void DoUserAction(UserAction userAction, object? parameter)
        {
            UserActionHandler?.Invoke(userAction, parameter);
        }

        public void DoClose()
        {
            Clef.LinesChanged -= Reload;
            Clef.Dispose();
            ClefLines.Clear();
        }

        private void Reload(object? sender, LinesChangedEventArgs e)
        {
            if (e.Action != LinesChangedEventArgs.LinesChangedEventArgsAction.None)
            {
                Reload(e, false);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DateInfo)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileInfo)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataColumns)));
            }
        }


        private void Reload()
        {
            Reload(new LinesChangedEventArgs(LinesChangedEventArgs.LinesChangedEventArgsAction.Reset), false);
        }

        private void Reload(LinesChangedEventArgs e, bool pinPresetChanged)
        {
            bool newFilters = false;
            foreach (var p in Clef.Properties)
            {
                if (!_filters.TryGetValue(p.Key, out Filter? value))
                {
                    Filter filter = new(p.Key, p.Value.Item2);
                    PropertyChangedEventManager.AddHandler(filter, (s, e) => { Reload(); }, nameof(filter.Values));
                    _filters.Add(p.Key, filter);
                    ClefFilterViewModel clefFilterViewModel = new(p.Value.Item1, filter, true);
                    PropertyChangedEventManager.AddHandler(clefFilterViewModel, (s, e) => { NotifyVisibleFiltersChanged(); }, nameof(clefFilterViewModel.Visible));
                    Filters.Add(clefFilterViewModel);
                    newFilters = true;
                }
                else
                {
                    value.Update(p.Value.Item2);
                }
            }
            foreach (var p in Clef.Data)
            {
                if (!_filters.TryGetValue(p.Key, out Filter? value))
                {
                    Filter filter = new(p.Key, p.Value);
                    PropertyChangedEventManager.AddHandler(filter, (s, e) => { Reload(); }, nameof(filter.Values));
                    _filters.Add(p.Key, filter);
                    ClefFilterViewModel clefFilterViewModel = new(p.Key, filter, Settings.SessionSettings.IsVisibleFilterByDefault(p.Key));
                    PropertyChangedEventManager.AddHandler(clefFilterViewModel, (s, e) => { NotifyVisibleFiltersChanged(); }, nameof(clefFilterViewModel.Visible));
                    Filters.Add(clefFilterViewModel);
                    var dataColumnView = new DataColumnView(p.Key, Settings.SessionSettings.IsVisibleColumnByDefault(p.Key));
                    PropertyChangedEventManager.AddHandler(dataColumnView, (s, e) => { NotifyDataColumnEnabledChanged(); }, nameof(dataColumnView.Enabled));
                    _dataColumns.Add(dataColumnView);
                    newFilters = true;
                }
                else
                {
                    value.Update(p.Value);
                }
            }
            if (newFilters)
            {
                NotifyVisibleFiltersChanged();
                NotifyDataColumnEnabledChanged();
            }

            List<IMatcher> matchers = CreateMatchers();
            CalculationRunning = true;
            _filterTaskManager.Filter(matchers, e.Action, pinPresetChanged,
                 (selectedIndex) =>
                 {
                     SelectedIndex = selectedIndex;
                     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DateInfo)));
                     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileInfo)));
                     Reloaded?.Invoke();
                     CalculationRunning = false;
                 });
        }
        public ObservableCollection<DataColumnView> DataColumns => _dataColumns;

        private List<IMatcher> CreateMatchers()
        {
            List<IMatcher> matchers = [];
            foreach (IFilter v in _filters.Values)
            {
                if (!v.AcceptsAll)
                {
                    matchers.Add(v.Create());
                }
            }
            if (null != _textFilter && !_textFilter.AcceptsAll)
            {
                matchers.Add(_textFilter.Create());
            }

            return matchers;
        }

        public Clef Clef { get; }
        public string FilePath
        {
            get
            {
                StringBuilder sb = new();
                foreach (FileInfo fi in Clef.File)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("; ");
                    }
                    sb.Append(fi.FullName);
                }
                return sb.ToString();
            }
        }
        public string FileInfo
        {
            get
            {
                return $"Log Entries displayed: {ClefLines.Count} (Total {Clef.Count}, Files {Clef.File.Count}, Size last file {ClefViewSettings.FormatFileSize(Clef.SeekPos)})";
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
        public IEnumerable<ClefFilterViewModel> VisibleFilters
        {
            get
            {
                return Filters.Where(f => f.Visible);
            }
        }
        private void NotifyVisibleFiltersChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VisibleFilters)));
        }

        public string? TextFilter
        {
            get => _textFilter?.FilterString;
            set
            {
                if (value != _textFilter?.FilterString)
                {
                    _textFilter = new(value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextFilter)));
                }
            }
        }
        public GridLength ColWidthEvtList => _colWidthEvtList;
        public GridLength ColWidthDetails => _colWidthDetails;
        public Visibility DetailsVisibility => _settings.SessionSettings.DetailView ? Visibility.Visible: Visibility.Collapsed;

        public ICommand ClearTextFilter { get; }

        public ICommand ApplyTextFilter { get; }
        public ICommand FiltersMenu { get; }

        public ClefLineViewModel? SelectedItem
        {
            get
            {
                if (SelectedIndex < 0 || SelectedIndex >= ClefLines.Count)
                {
                    return null;
                }
                return ClefLines[SelectedIndex];
            }
        }
        public IEnumerable<ClefLineViewDetailModel> SelectedItemDetails
        {
            get
            {
                return SelectedItem?.Details(DataColumns) ?? [];
            }
        }

        public double VerticalOffset { get; set; }
        public double HorizontalOffset { get; set; }

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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItem)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItemDetails)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DatePosition)));
                }
            }
        }

        public string? DatePosition
        {
            get
            {
                return _settings.SessionSettings.Format(_settings.RefTimeStamp);
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
                            break;
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
