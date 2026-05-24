using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ndu.ClefInspect.ViewModel.ClefView
{
    public class ClefViewSettings : INotifyPropertyChanged
    {
        private readonly MainViewSettings _settings;
        private DateTime? _refTimeStamp;

        public ClefViewSettings(MainViewSettings settings)
        {
            _settings = settings;
            IgnoredEventId = new ObservableCollection<string>(_settings.HideEventIds);
            LoadDefaults();
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        public ReadOnlyCollection<Model.Configuration.PinPresetOptions> PinPresets => _settings.PinPresets;

        public string FormatDelta(DateTime? dt)
        {
            if (_refTimeStamp == null || dt == null)
            {
                return "";
            }
            return (dt - _refTimeStamp).Value.TotalMilliseconds.ToString("0.0");
        }
        public static string FormatFileSize(long seekPos)
        {
            if (seekPos < 1024)
            {
                return $"{seekPos} B";
            }
            if (seekPos < 1024 * 1024)
            {
                return $"{seekPos / 1024} KiB";
            }
            return $"{seekPos / (1024 * 1024)} MiB";
        }

        public void LoadDefaults()
        {
            LocalTime = _settings.ViewSettings.LocalTime;
            OneLineOnly = _settings.ViewSettings.OneLineOnly;
            DetailView = _settings.ViewSettings.DetailView;
            DetailViewFraction = _settings.ViewSettings.DetailViewFraction;
            TextSearchMsgOnly = _settings.ViewSettings.TextSearchMsgOnly;
        }
        public void StoreAsDefaults()
        {
            _settings.ViewSettings.LocalTime = LocalTime;
            _settings.ViewSettings.OneLineOnly = OneLineOnly;
            _settings.ViewSettings.DetailView = DetailView;
            _settings.ViewSettings.DetailViewFraction = DetailViewFraction;
            _settings.ViewSettings.TextSearchMsgOnly = TextSearchMsgOnly;
        }
        public bool IsVisibleFilterByDefault(string filter)
        {
            return _settings.ViewSettings.DefaultFilterVisibility.Contains(filter);
        }
        public void SetVisibleFilterDefaults(List<string> filters)
        {
            _settings.ViewSettings.DefaultFilterVisibility.Clear();
            foreach (string s in filters)
            {
                _settings.ViewSettings.DefaultFilterVisibility.Add(s);
            }
        }
        public bool IsVisibleColumnByDefault(string column)
        {
            return _settings.ViewSettings.DefaultColumnVisibility.Contains(column);
        }
        public void SetVisibleColumnDefaults(List<string> columns)
        {
            _settings.ViewSettings.DefaultColumnVisibility.Clear();
            foreach (string s in columns)
            {
                _settings.ViewSettings.DefaultColumnVisibility.Add(s);
            }
        }
        public string? Format(DateTime? dt) => MainViewSettings.Format(dt, LocalTime);

        public bool LocalTime
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LocalTime)));
                }
            }
        }
        public bool OneLineOnly
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OneLineOnly)));
                }
            }
        }
        public bool DetailView
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DetailView)));
                }
            }
        }
        public double DetailViewFraction
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DetailViewFraction)));
                }
            }
        }
        public bool TextSearchMsgOnly
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextSearchMsgOnly)));
                }
            }
        }

        public void ResetView()
        {
            ShowFiltered = false;
            ShowHiddenEvents = false;
            ShowPinned = true;
            FilterAll = false;
        }

        public DateTime? RefTimeStamp
        {
            get
            {
                return _refTimeStamp;
            }
            set
            {
                _refTimeStamp = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RefTimeStamp)));
            }
        }

        public int DefaultCapacity { get; } = 100000;
        public ObservableCollection<string> IgnoredEventId { get; set; }
        public bool ShowHiddenEvents
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowHiddenEvents)));
                }
            }
        } = false;
        public bool ShowFiltered
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowFiltered)));
                }
            }
        } = false;
        public bool ShowPinned
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowPinned)));
                }
            }
        } = true;
        public bool FilterAll
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterAll)));
                }
            }
        } = false;
    }
}
