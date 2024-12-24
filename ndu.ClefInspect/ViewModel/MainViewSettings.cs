using ndu.ClefInspect.Model;
using ndu.ClefInspect.ViewModel.ClefView;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace ndu.ClefInspect.ViewModel
{
    public class MainViewSettings : INotifyPropertyChanged
    {
        private readonly Configuration _configuration;

        private static readonly IFormatProvider local = CultureInfo.CurrentCulture.DateTimeFormat;
        private static readonly string utc = CultureInfo.InvariantCulture.DateTimeFormat.UniversalSortableDateTimePattern;

        public MainViewSettings()
        {
            // load defaults
            _configuration = new Configuration();
            _configuration.Session.Files.CollectionChanged += (sender, e) => { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasSessionData))); };
        }

        public Configuration UserSettings => _configuration;
        public bool CanPersist => _configuration.ClefFeatures.WriteableConfig;
        public bool HasSessionData => _configuration.Session.Files.Count > 0;

        public void Persist()
        {
            _configuration.Write();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool LocalTime
        {
            get => _configuration.ViewSettings.LocalTime;
            set
            {
                if (_configuration.ViewSettings.LocalTime != value)
                {
                    _configuration.ViewSettings.LocalTime = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LocalTime)));
                }
            }
        }
        public bool OneLineOnly
        {
            get => _configuration.ViewSettings.OneLineOnly;
            set
            {
                if (_configuration.ViewSettings.OneLineOnly != value)
                {
                    _configuration.ViewSettings.OneLineOnly = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OneLineOnly)));
                }
            }
        }

        public static string LevelWidthText => "WARNINGW";
        public static string DefaultColWidthText => "WWWWWWWWWWWWWWWWWWWWWW";
        public static string DeltaWidthText => "WWWWWW";
        private static string ClefLineFormatString { get; }
        private static string ClefLineNoLogFormatString { get; }
        private static string ClefColFormatString { get; }

        static MainViewSettings()
        {
            DateWidthText = DateTime.Now.ToString(utc) + "W";
            ClefLineFormatString = "{0,-" + DateWidthText.Length + "} {1,-" + LevelWidthText.Length + "} {2}";
            ClefLineNoLogFormatString = "{0,-" + DateWidthText.Length + "} {1,-" + LevelWidthText.Length + "}";
            ClefColFormatString = "{0,-" + DefaultColWidthText.Length + "}";
        }

        public static readonly string PinWidthText = "NNN";
        public static string DateWidthText { get; }

        public string Format(DateTime? dt)
        {
            if (dt == null)
            {
                return "unknown";
            }
            if (LocalTime)
            {
                return dt.Value.ToString(local);
            }
            else
            {
                return dt.Value.ToUniversalTime().ToString(utc);
            }
        }
        public static string Format(ClefLineView line)
        {
            return string.Format(ClefLineFormatString, line.Time, line.Level, line.Message);
        }
        public static string FormatNoLog(ClefLineView line)
        {
            return string.Format(ClefLineNoLogFormatString, line.Time, line.Level);
        }
        public static string FormatCol(string? data)
        {
            return string.Format(ClefColFormatString, data);
        }
        public bool IsVisibleFilterByDefault(string filter)
        {
            return _configuration.ViewSettings.DefaultFilterVisibility.Contains(filter);
        }

        public void SetVisibleFilterDefaults(List<string> filters)
        {
            _configuration.ViewSettings.DefaultFilterVisibility.Clear();
            foreach (string s in filters)
            {
                _configuration.ViewSettings.DefaultFilterVisibility.Add(s);
            }
        }

        public bool IsVisibleColumnByDefault(string column)
        {
            return _configuration.ViewSettings.DefaultColumnVisibility.Contains(column);
        }
        public void SetVisibleColumnDefaults(List<string> columns)
        {
            _configuration.ViewSettings.DefaultColumnVisibility.Clear();
            foreach (string s in columns)
            {
                _configuration.ViewSettings.DefaultColumnVisibility.Add(s);
            }
        }

        public void SetSessionFiles(List<string> openFiles)
        {
            _configuration.Session.Files.Clear();
            foreach (string file in openFiles)
            {
                _configuration.Session.Files.Add(file);
            }
        }

        public ObservableCollection<string> GetSessionFiles()
        {
            return _configuration.Session.Files;
        }
    }
}
