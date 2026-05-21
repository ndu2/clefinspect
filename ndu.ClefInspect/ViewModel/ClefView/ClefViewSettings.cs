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
            IgnoredEventId = new ObservableCollection<string>(_settings.UserSettings.EventSettings.HideEventIds);
        }

        public MainViewSettings SessionSettings => _settings;

        public event PropertyChangedEventHandler? PropertyChanged;

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
