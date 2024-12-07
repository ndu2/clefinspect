using clef_inspect.ViewModel.ClefView;
using System;
using System.ComponentModel;
using System.Globalization;

namespace clef_inspect.ViewModel
{
    public class Settings : INotifyPropertyChanged
    {
        private bool _localTime = true;
        private bool _devMode = false;

        private static readonly IFormatProvider local = CultureInfo.CurrentCulture.DateTimeFormat;
        private static readonly string utc = CultureInfo.InvariantCulture.DateTimeFormat.UniversalSortableDateTimePattern;

        public Settings()
        {
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        public bool LocalTime
        {
            get => _localTime;
            set
            {
                if (_localTime != value)
                {
                    _localTime = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LocalTime)));
                }
            }
        }
        public bool DevMode
        {
            get => _devMode;
            set
            {
                if (_devMode != value)
                {
                    _devMode = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DevMode)));
                }
            }
        }

        public static string LevelWidthText => "WARNINGW";

        public static string SourceContextWidthText => "WWWWWWWWWWWWWWWWWWWWWW";

        public static string DeltaWidthText => "WWWWWW";

        private static string ClefLlineFormatString { get; }

        static Settings()
        {
            DateWidthText = DateTime.Now.ToString(utc) + "W";
            ClefLlineFormatString = "{0,-" + DateWidthText.Length + "} {1,-" + LevelWidthText.Length + "} {2,-" + SourceContextWidthText.Length + "} {3}";
        }

        public static string PinWidthText = "NNN";
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
        public string Format(ClefLineView line)
        {
            return string.Format(ClefLlineFormatString, line.Time, line.Level, line.SourceContext, line.Message);
        }
    }
}
