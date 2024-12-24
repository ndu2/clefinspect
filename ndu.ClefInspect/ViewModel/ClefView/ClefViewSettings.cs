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
            PropertyChangedEventManager.AddHandler(_settings, (s, e) => { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SessionSettings))); }, string.Empty);
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SessionSettings)));
            }
        }

        public int DefaultCapacity { get; } = 100000;
    }
}
