using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows;
using static clef_inspect.Model.LinesChangedEventArgs;

namespace clef_inspect.Model
{
    public partial class Clef : IDisposable, INotifyPropertyChanged
    {
        private const int FileRefreshDelayMs = 500;
        private const int BufferSize = 10 * 1024 * 1024;
        private const int UiRefreshDelayMs = 250;
        public const string LEVEL_KEY = "@l";
        public const string LEVEL_EMPTY = "Info";
        private bool _disposedValue;
        private readonly ClefLockedList _lines = new();
        private readonly ConcurrentDictionary<string, (string, ConcurrentDictionary<string, int>)> _properties = new();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _data = new();
        private long _seekPos;
        private readonly Timer _timer;
        private bool _autoUpdate;
        private bool _fileOk;

        private byte[] bytes = new byte[BufferSize];
        private byte[] temp = new byte[BufferSize];
        private int _bytesAvail = 0;

        private readonly Dictionary<string, string> _indexableProperties = new()
        {
            {LEVEL_KEY,"Level"},
            //{"@i","Event Id"}
        };

        public Clef(FileInfo file)
        {
            this.File = file;
            _seekPos = 0;
            _autoUpdate = true;
            _fileOk = true;
            _timer = new Timer(Scan, this, 0, Timeout.Infinite);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public delegate void LinesChangedEventHandler(object? sender, LinesChangedEventArgs e);
        public event LinesChangedEventHandler? LinesChanged;

        public FileInfo File { get; }

        public string FilePath { get => File.FullName; }

        public string FileName { get => File.Name; }

        public long SeekPos { get => _seekPos; }

        public IList<ClefLine> ViewFrom(int first) => _lines.ViewFrom(first);

        public int Count => _lines.Count;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    AutoUpdate = false;
                    _timer.Dispose();
                }
                _lines.Clear();
                _properties.Clear();
                _data.Clear();
                bytes = Array.Empty<byte>();
                temp = Array.Empty<byte>();
                _disposedValue = true;
            }
        }

        ~Clef()
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

        public ConcurrentDictionary<string, (string, ConcurrentDictionary<string, int>)> Properties { get => _properties; }
        public ConcurrentDictionary<string, ConcurrentDictionary<string, int>> Data { get => _data; }
        public bool AutoUpdate
        {
            get => _autoUpdate;
            set
            {
                if (_autoUpdate != value)
                {
                    _autoUpdate = value;
                    if (AutoUpdate)
                    {
                        _timer.Change(FileRefreshDelayMs, Timeout.Infinite);
                    }
                    else
                    {
                        _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoUpdate)));
                }
            }
        }
        public bool FileOk
        {
            get => _fileOk;
            set
            {
                if (_fileOk != value)
                {
                    _fileOk = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileOk)));
                }
            }
        }


        private void Scan(object? _)
        {
            (bool fileok, bool tracking) = Scan( (args) => { LinesChanged?.Invoke(this, args); });
            if (!tracking)
            {
                Application.Current?.Dispatcher?.Invoke(() => { AutoUpdate = false; });
            }
            if (fileok != FileOk)
            {
                Application.Current?.Dispatcher?.Invoke(() => { FileOk = fileok; });
            }
            try
            {
                _timer.Change(AutoUpdate ? FileRefreshDelayMs : Timeout.Infinite, Timeout.Infinite);
            }
            catch (ObjectDisposedException)
            {
                // timer can be disposed during Scan(), in this case _timer.Change will throw ObjectDisposedException
            }
        }

        public class TimedUiUpdate
        {
            private readonly int _ms;
            private readonly Action<LinesChangedEventArgs> _uiUpdate;
            DateTime _time;
            private bool _updateRunning;
            private LinesChangedEventArgsAction _uiUpdateAction;
            public TimedUiUpdate(int ms, Action<LinesChangedEventArgs> uiUpdate)
            {
                _uiUpdateAction = LinesChangedEventArgsAction.None;
                _ms = ms;
                _uiUpdate = uiUpdate;
                _time = DateTime.Now;
                _updateRunning = false;
            }
            public void ForceUpdate(Action preAction)
            {
                preAction();
                Application.Current?.Dispatcher?.Invoke(() => { _uiUpdate(new LinesChangedEventArgs(_uiUpdateAction)); });
                _uiUpdateAction = LinesChangedEventArgsAction.None;
            }
            public void CheckAndUpdate(Action preAction)
            {
                DateTime now = DateTime.Now;
                if (!_updateRunning && (now - _time).TotalMilliseconds > _ms)
                {
                    _time = now;
                    preAction();
                    _updateRunning = true;
                    LinesChangedEventArgsAction uiUpdateAction = _uiUpdateAction;
                    Application.Current?.Dispatcher?.BeginInvoke(() =>
                    {
                        _uiUpdate(new LinesChangedEventArgs(uiUpdateAction));
                        _updateRunning = false;
                    });
                    _uiUpdateAction = LinesChangedEventArgsAction.None;
                }
            }
            public void Notify(LinesChangedEventArgsAction uiUpdateAction)
            {
                _uiUpdateAction = Union(_uiUpdateAction, uiUpdateAction);
            }
        }

        private (bool, bool) Scan(Action<LinesChangedEventArgs> uiUpdate)
        {
            TimedUiUpdate uiUpdateTimer = new(UiRefreshDelayMs, uiUpdate);
            try
            {
                File.Refresh();
                if (File.Exists)
                {
                    if (_seekPos == 0)
                    {
                        _lines.Clear();
                        uiUpdateTimer.Notify(LinesChangedEventArgsAction.Reset);
                    }
                    if (File.Length == _seekPos)
                    {
                        return (true, true);
                    }
                    if (File.Length < _seekPos)
                    {
                        _seekPos = 0;
                        _bytesAvail = 0;
                        return (true, false);
                    }
                    int bytesAvailPref = _bytesAvail;

                    using FileStream fileStream = System.IO.File.Open(File.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    fileStream.Seek(_seekPos, SeekOrigin.Begin);
                    int bytesRead;
                    int start = 0;
                    while ((bytesRead = fileStream.Read(bytes, _bytesAvail, bytes.Length - _bytesAvail)) > 0)
                    {
                        long pos = _seekPos - _bytesAvail;
                        start = 0;
                        _bytesAvail += bytesRead;
                        for (int i = 0; i < _bytesAvail; ++i)
                        {
                            if (bytes[i] == '\n')
                            {
                                int endline = i;
                                if (i > 0 && bytes[i - 1] == '\r')
                                {
                                    endline--;
                                }
                                if (bytesAvailPref > 0)
                                {
                                    if (bytesAvailPref != endline)
                                    {
                                        JsonObject? logline = Scan(bytes, start, endline);
                                        _lines.ReplaceLast(new ClefLine(pos + i, logline));
                                        uiUpdateTimer.Notify(LinesChangedEventArgsAction.Reset);
                                    }
                                    bytesAvailPref = 0;
                                }
                                else
                                {
                                    JsonObject? logline = Scan(bytes, start, endline);
                                    _lines.Add(new ClefLine(pos + i, logline));
                                    uiUpdateTimer.Notify(LinesChangedEventArgsAction.Add);
                                }
                                start = i + 1;
                            }
                        }
                        if (start == 0)
                        {
                            // EOF or buffer full (log line too long).
                            int dat;
                            do
                            {
                                // eat the rest of a possibly too long line
                                dat = fileStream.ReadByte();
                            } while (dat > 0 && dat != '\n');
                            if (dat == '\n')
                            {
                                // clear buffer and scan to the next line
                                _bytesAvail = 0;
                                dat = fileStream.ReadByte();
                                if (dat != '\r')
                                {
                                    bytes[0] = (byte)dat;
                                    _bytesAvail = 1;
                                }
                            }
                        }
                        else
                        {
                            _bytesAvail -= start;
                            Array.Copy(bytes, start, temp, 0, _bytesAvail);
                            Array.Copy(temp, bytes, _bytesAvail);
                        }
                        _seekPos = fileStream.Position;
                        uiUpdateTimer.CheckAndUpdate(FillDefaultFilterStats);
                    }
                    if (_bytesAvail > 0)
                    {
                        bool replaceLast = bytesAvailPref > 0;
                        if (replaceLast)
                        {
                            // replace last entry
                            bytesAvailPref = 0;
                        }
                        // end of file. add rest in case there is no newline at the end
                        JsonObject? logline = Scan(bytes, start, start + _bytesAvail);
                        ClefLine line = new(_seekPos - _bytesAvail, logline);
                        if (replaceLast)
                        {
                            _lines.ReplaceLast(line);
                            uiUpdateTimer.Notify(LinesChangedEventArgsAction.Reset);
                        }
                        else
                        {
                            _lines.Add(line);
                            uiUpdateTimer.Notify(LinesChangedEventArgsAction.Add);
                        }
                        Array.Copy(bytes, start, temp, 0, _bytesAvail);
                        Array.Copy(temp, bytes, _bytesAvail);
                    }
                }
                else
                {
                    return (false, false);
                }
            }
            catch (Exception e)
            {
                _lines.Add(new ClefLine(long.MaxValue, new JsonObject { new KeyValuePair<string, JsonNode?>("@m", JsonValue.Create(e.ToString())) }));
                uiUpdateTimer.Notify(LinesChangedEventArgsAction.Reset);
            }
            finally
            {
                uiUpdateTimer.ForceUpdate(FillDefaultFilterStats);
            }
            return (true, true);
        }

        private void FillDefaultFilterStats()
        {
            int anz = _lines.Count;
            foreach (var filter in Properties)
            {
                int tot = filter.Value.Item2.Aggregate(0, (i, j) => (i + j.Value), x => x);
                int missing = anz - tot;
                if (missing > 0)
                {
                    filter.Value.Item2.AddOrUpdate("", missing, (s, i) => { return (i + missing); });
                }
            }
            foreach (var filter in Data)
            {
                int tot = filter.Value.Aggregate(0, (i, j) => (i + j.Value), x => x);
                int missing = anz - tot;
                if (missing > 0)
                {
                    filter.Value.AddOrUpdate("", missing, (s, i) => { return (i + missing); });
                }
            }
        }

        private JsonObject? Scan(byte[] bytes, int start, int endline)
        {
            try
            {
                string line = Encoding.UTF8.GetString(bytes, start, endline - start);
                if (!(line[0] == '{') && line[^1] == '}')
                {
                    return new JsonObject
                    {
                        new KeyValuePair<string, JsonNode?>("@m", JsonValue.Create(line))
                    };
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
                            if (!(kv.Key[0] == '@') && kv.Value != null)
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
            catch (Exception ex)
            {
                return new JsonObject
                {
                    new KeyValuePair<string, JsonNode?>("@m", JsonValue.Create(ex.Message))
                };
            }
        }
    }
}
