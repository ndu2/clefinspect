using clef_inspect.Model;
using System;
using System.ComponentModel;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Media;

namespace clef_inspect.ViewModel.ClefView
{
    public class ClefLineView : INotifyPropertyChanged
    {
        private readonly ClefViewSettings _settings;
        private readonly string? _messageOneLine;
        public ClefLineView(ClefLine line, ClefViewSettings settings)
        {
            ClefLine = line;
            int nl = line.Message?.IndexOf('\n') ?? -1;
            if (nl > 0)
            {
                _messageOneLine = line?.Message?[..(nl - 1)] + " ...";
            }
            else
            {
                _messageOneLine = line.Message;
            }
            _settings = settings;
            settings.PropertyChanged += Settings_PropertyChanged;
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_settings.Settings))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Time)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeltaTime)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
            }
        }
        public string? Time
        {
            get
            {
                return _settings.Settings.Format(GetTime());
            }
        }
        public string? DeltaTime
        {
            get
            {
                return _settings.FormatDelta(GetTime());
            }
        }

        public ClefLine ClefLine { get; }
        public long Sort => ClefLine.Sort;
        public DateTime? GetTime() => ClefLine.Time;
        public string? Level => ClefLine.Level;


        public Brush LevelBackground
        {
            get
            {
                string? l = Level;
                if (l != null)
                {
                    if (l.StartsWith('e') || l.StartsWith('E'))
                        return Brushes.Red;
                    if (l.StartsWith('w') || l.StartsWith('W'))
                        return Brushes.Yellow;
                    if (l.StartsWith('i') || l.StartsWith('I') || l.Length == 0)
                        return Brushes.LightSkyBlue;
                }
                return SystemColors.WindowBrush;
            }
        }
        public string? SourceContext => ClefLine.SourceContext;
        public string? Message => _settings.Settings.OneLineOnly ? _messageOneLine: ClefLine.Message;
        public JsonObject? JsonObject => ClefLine.JsonObject;
        public string? Json => ClefLine.Json;

        public bool Pin
        {
            get => ClefLine.Pin;
            set
            {
                if (ClefLine.Pin != value)
                {
                    ClefLine.Pin = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Pin)));
                }
            }
        }

        public string? this[string key]
        {
            get
            {
                JsonNode? jsonNode;
                if(ClefLine?.JsonObject?.TryGetPropertyValue(key, out jsonNode) ?? false)
                {
                    return _settings.Settings.OneLineOnly ? jsonNode?.ToJsonString() : jsonNode?.ToString();
                }
                else
                {
                    return null;
                }
            }
        }
        public override string ToString()
        {
            return _settings.Settings.Format(this);
        }
    }
}
