using System.ComponentModel;

namespace clef_inspect.ViewModel.ClefView
{
    public class ClefViewSettings : INotifyPropertyChanged
    {
        private readonly Settings _settings;
        private DateTime? _refTimeStamp;

        public ClefViewSettings(Settings settings)
        {
            _settings = settings;
            _settings.PropertyChanged += (s, e) => { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Settings))); };
        }

        public Settings Settings => _settings;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FormatDelta(DateTime? dt)
        {
            if (_refTimeStamp == null || dt == null)
            {
                return "";
            }
            return (dt - _refTimeStamp).Value.TotalMilliseconds.ToString("0.0");
        }

        public string FormatFileSize(long seekPos)
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Settings)));
            }
        }

        public int DefaultCapacity { get; } = 100000;
    }
}
