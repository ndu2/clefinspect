﻿using ndu.ClefInspect.Model;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Media;

namespace ndu.ClefInspect.ViewModel.ClefView
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
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        public void NotifySettingsRefTimeStampChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeltaTime)));
        }
        public void NotifySettingsLocalTimeChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Time)));
        }
        public void NotifySettingsOneLineOnlyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
        }

        public string? Time
        {
            get
            {
                return _settings.SessionSettings.Format(GetTime());
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
                    if (l.StartsWith('e') || l.StartsWith('E') || l.StartsWith('f') || l.StartsWith('F'))
                        return Brushes.Red;
                    if (l.StartsWith('w') || l.StartsWith('W'))
                        return Brushes.Yellow;
                    if (l.StartsWith('i') || l.StartsWith('I') || l.Length == 0)
                        return Brushes.LightSkyBlue;
                }
                return SystemColors.WindowBrush;
            }
        }
        public string? Message => _settings.SessionSettings.OneLineOnly ? _messageOneLine : ClefLine.Message;
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
                if (ClefLine?.JsonObject?.TryGetPropertyValue(key, out JsonNode? jsonNode) ?? false)
                {
                    return _settings.SessionSettings.OneLineOnly ? jsonNode?.ToJsonString() : jsonNode?.ToString();
                }
                else
                {
                    return null;
                }
            }
        }
        public override string ToString()
        {
            return MainViewSettings.Format(this);
        }

        public string? ToString(IList<string> columns)
        {
            StringBuilder sb = new(MainViewSettings.FormatNoLog(this));
            foreach (string c in columns)
            {
                sb.Append(MainViewSettings.FormatCol(this[c]));
            }
            sb.Append(Message);
            return sb.ToString();
        }
    }
}
