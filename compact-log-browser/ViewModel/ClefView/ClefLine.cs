using compact_log_browser.Model;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Nodes;

namespace compact_log_browser.ViewModel.ClefView
{
    public class ClefLine : INotifyPropertyChanged
    {
        private JsonObject? _line;
        private ClefViewSettings _settings;

        public ClefLine(JsonObject? line, ClefViewSettings settings)
        {
            _line = line;
            _settings = settings;
            settings.PropertyChanged += Settings_PropertyChanged;
            Message = Render(line);
        }

        private static string? Render(JsonObject? line)
        {
            return line?["@m"]?.ToString() ?? Render(line?["@mt"]?.ToString(), line);
        }

        private enum ParseState
        {
            Text,
            Token,
            TokenEaten,
        }

        private static string? Render(string? mt, JsonObject? line)
        {
            if (mt == null || line == null) return null;
            ParseState state = ParseState.Text;
            StringBuilder outString = new StringBuilder();
            int i1 = 0;
            for (int i = 0; i < mt.Length; ++i)
            {
                switch (state)
                {
                    case ParseState.Text:
                        if (mt[i] == '{')
                        {
                            outString.Append(mt.Substring(i1, i - i1));
                            state = ParseState.Token;
                            i1 = i;
                        }
                        break;
                    case ParseState.Token:
                        if (mt[i] == '}')
                        {
                            state = ParseState.TokenEaten;
                            string t1 = mt.Substring(i1 + 1, i - i1 - 1);
                            outString.Append(line[t1] ?? $"{{{t1}}}");

                        }
                        break;
                    case ParseState.TokenEaten:
                        state = ParseState.Text;
                        i1 = i;
                        break;
                }
            }
            outString.Append(mt.Substring(i1, mt.Length - i1));
            return outString.ToString();
        }

        private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_settings.Settings)) { }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Time)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeltaTime)));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public DateTime? GetTime()
        {
            string? l = _line?["@t"]?.ToString();
            if (l != null)
            {
                try
                {
                    DateTime dt = DateTime.Parse(l);
                    return dt;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return null;
                }
            }
            return null;
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

        public string? Level
        {
            get
            {
                return _line?["@l"]?.ToString();
            }
        }


        public string? SourceContext
        {
            get
            {
                return _line?["SourceContext"]?.ToString();
            }
        }
        public string? Message { get; set; }

        public JsonObject? JsonObject
        {
            get => _line;
        } 

        public string? Json
        {
            get
            {
                return _line?.ToString();
            }
        }

        public override string ToString()
        {
            return _settings.Settings.Format(this);
        }
    }
}
