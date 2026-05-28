using ndu.ClefInspect.Model;
using ndu.ClefInspect.ViewModel.ClefView;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;

namespace ndu.ClefInspect.ViewModel
{
    public class MainViewSettings
    {
        private readonly Configuration _configuration;
        private readonly HashSet<string> _sessionFilesSet;
        private static readonly IFormatProvider local = CultureInfo.CurrentCulture.DateTimeFormat;
        private static readonly string utc = CultureInfo.InvariantCulture.DateTimeFormat.UniversalSortableDateTimePattern;
        public MainViewSettings()
        {
            // load defaults
            _configuration = new Configuration();
            _sessionFilesSet = new(_configuration.Session.Files);
            RecentFiles = _configuration.Session.Files;
            PinPresets = _configuration.PinPresets.AsReadOnly();
            HideEventIds = _configuration.EventSettings.HideEventIds.AsReadOnly();
        }
        public ReadOnlyCollection<Configuration.PinPresetOptions> PinPresets { get; }
        public ReadOnlyCollection<string> HideEventIds { get; }
        public Configuration.ViewSettingsOptions ViewSettings => _configuration.ViewSettings;
        public bool CanPersist => _configuration.ClefFeatures.WriteableConfig;

        public void AddRecentFile(List<FileInfo> fileNames)
        {
            List<string> names = fileNames.Select(fn => fn.FullName).ToList();
            string recentEntry = String.Join(Fsep, names);
            if (_sessionFilesSet.Add(recentEntry))
            {
                _configuration.Session.Files.Add(recentEntry);
            }
            while (_configuration.Session.Files.Count >= _configuration.Session.MaxFiles)
            {
                _configuration.Session.Files.RemoveAt(0);
            }
            Persist();
        }
        public void ClearRecentFiles()
        {
            _configuration.Session.Files.Clear();
            _sessionFilesSet.Clear();
            Persist();
        }
        public ObservableCollection<string> RecentFiles { get; }
        public void Persist()
        {
            if (CanPersist)
            {
                _configuration.Write();
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

        public static readonly string PinWidthText = "NNNNNNN";
        public static readonly char Fsep = '|';

        public static string DateWidthText { get; }

        public static string? Format(DateTime? dt, bool localTime)
        {
            if (dt == null)
            {
                return null;
            }
            if (localTime)
            {
                return dt.Value.ToString(local);
            }
            else
            {
                return dt.Value.ToUniversalTime().ToString(utc);
            }
        }
        public static string Format(ClefLineViewModel line)
        {
            return string.Format(ClefLineFormatString, line.Time, line.Level, line.Message);
        }
        public static string FormatNoLog(ClefLineViewModel line)
        {
            return string.Format(ClefLineNoLogFormatString, line.Time, line.Level);
        }
        public static string FormatCol(string? data)
        {
            return string.Format(ClefColFormatString, data);
        }
    }
}
