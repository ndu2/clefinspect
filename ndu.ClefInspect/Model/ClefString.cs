using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json.Nodes;
using System.Windows;
using static ndu.ClefInspect.Model.LinesChangedEventArgs;

namespace ndu.ClefInspect.Model
{
    public class ClefString : IClef
    {
        private bool _disposedValue;
        private readonly string? _clefFromString;
        private readonly ClefLockedList _lines = new();
        private readonly ConcurrentDictionary<string, (string, ConcurrentDictionary<string, int>)> _properties = new();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _data = new();


        private readonly Dictionary<string, string> _indexableProperties = new()
        {
            {ClefSchema.LEVEL_KEY,"Level"},
            //{"@i","Event Id"}
        };

        public ClefString(string? text, ReadOnlyCollection<PinPreset> pinPresets)
        {
            _clefFromString = text;
            PinPresets = pinPresets;
            Task.Run(Scan);
        }

        private void Scan()
        {
            // scan text
            if (_clefFromString != null)
            {
                int n = 1;
                using StringReader sr = new StringReader(_clefFromString);
                string? line = null;

                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Count() > 0)
                        _lines.Add(CreateClefLine(n++, Scan(line)));
                }

                Application.Current?.Dispatcher?.Invoke(() => { LinesChanged?.Invoke(this, new LinesChangedEventArgs(LinesChangedEventArgsAction.Reset)); });

            }
            SeekPos = _clefFromString?.Length ?? 0;

        }
        private JsonObject? Scan(string line)
        {
            try
            {
                return ScanLine(line);
            }
            catch (Exception ex)
            {
                return
                [
                    new KeyValuePair<string, JsonNode?>("@m", JsonValue.Create(ex.Message))
                ];
            }
        }

        private JsonObject? ScanLine(string line)
        {
            if (!(line[0] == '{') && line[^1] == '}')
            {
                return
                [
                    new KeyValuePair<string, JsonNode?>("@m", JsonValue.Create(line))
                ];
            }
            JsonObject? logline = JsonNode.Parse(line) as JsonObject;

            if (logline != null)
            {
                foreach (KeyValuePair<string, JsonNode?> kv in logline)
                {
                    {
                        if (_indexableProperties.ContainsKey(kv.Key) && kv.Value is JsonValue value)
                        {
                            (string, ConcurrentDictionary<string, int>) values = _properties.GetOrAdd(kv.Key, (k) =>
                            {
                                ConcurrentDictionary<string, int> newSet = new();
                                return (_indexableProperties[k], newSet);


                            });
                            values.Item2.AddOrUpdate(value.ToString(), 1, (s, c) => c + 1);
                        }
                    }
                    {
                        if (((kv.Key.Length >= 1 && kv.Key[0] != '@') || (kv.Key.Length >= 2 && kv.Key[1] == '@')) && kv.Value != null)
                        {
                            ConcurrentDictionary<string, int> values = _data.GetOrAdd(kv.Key, (k) =>
                            {
                                ConcurrentDictionary<string, int> newSet = new();
                                return newSet;
                            });
                            values.AddOrUpdate(kv.Value.ToString(), 1, (s, c) => c + 1);
                        }
                    }
                }
            }
            return logline;
        }

        private ClefLine CreateClefLine(long pos, JsonObject? logline)
        {
            ClefLine clefLine = new(pos, logline);

            foreach (PinPreset pinPreset in PinPresets)
            {
                foreach (string s in pinPreset.SearchText)
                {
                    if (clefLine.Message?.Contains(s, StringComparison.InvariantCultureIgnoreCase) ?? false)
                    {
                        clefLine.PinPreset.Add(pinPreset);
                        break;
                    }
                }
            }
            return clefLine;
        }

        public string Title => "pasted text";

        public bool AutoUpdate
        {
            get => true;
            set
            {
                if (!value)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoUpdate)));
                }
            }
        }
        public bool FileOk
        {
            get => true;
            set
            {
                if (!value)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileOk)));
                }
            }
        }

        public List<FileInfo> File => new List<FileInfo>();

        public long SeekPos { get; private set; }

        public int Count => _lines.Count;

        public ConcurrentDictionary<string, (string, ConcurrentDictionary<string, int>)> Properties { get => _properties; }
        public ConcurrentDictionary<string, ConcurrentDictionary<string, int>> Data { get => _data; }

        public ReadOnlyCollection<PinPreset> PinPresets { get; internal set; }

        public event LinesChangedEventHandler? LinesChanged;
        public event PropertyChangedEventHandler? PropertyChanged;


        public IList<ClefLine> ViewFrom(int first) => _lines.ViewFrom(first);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _lines.Clear();
                _properties.Clear();
                _data.Clear();
                _disposedValue = true;
            }
        }

        ~ClefString()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
