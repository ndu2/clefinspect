using clef_inspect.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace clef_inspect.ViewModel.ClefView
{
    public class ClefLineView : INotifyPropertyChanged
    {
        private ClefViewSettings _settings;
        public ClefLineView(int sourceIdx, ClefLine line, ClefViewSettings settings)
        {
            SourceIdx = sourceIdx;
            ClefLine = line;
            _settings = settings;
            settings.PropertyChanged += Settings_PropertyChanged;
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_settings.Settings)) { }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Time)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeltaTime)));
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

        public int SourceIdx { get; }
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
        public string? Message => ClefLine.Message;
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
        public override string ToString()
        {
            return _settings.Settings.Format(this);
        }
    }
}
