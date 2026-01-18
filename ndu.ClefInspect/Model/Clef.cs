using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows;
using static ndu.ClefInspect.Model.LinesChangedEventArgs;

namespace ndu.ClefInspect.Model
{
    public partial class Clef : IDisposable, INotifyPropertyChanged
    {
        private const int FileRefreshDelayMs = 500;
        private const int BufferSize = 10 * 1024 * 1024;
        private const int UiRefreshDelayMs = 250;
        public const string LEVEL_KEY = "@l";
        public const string LEVEL_EMPTY = "Info";
        public const string EXCEPTION_KEY = "@x";
        public const string EXCEPTION_EMPTY = "";
        private bool _disposedValue;
        private readonly ClefLockedList _lines = new();
        private readonly ConcurrentDictionary<string, (string, ConcurrentDictionary<string, int>)> _properties = new();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _data = new();

        private int _filePos;
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
        public static readonly char Fsep = '|';

        public static Clef Create(string fd)
        {
            string[] fds = fd.Split(Fsep);
            List<FileInfo> fileInfos = [];
            foreach(string ifd in fds)
            {
                fileInfos.Add(new FileInfo(ifd));
            }
            return new Clef(fileInfos);
        }

        public Clef(List<FileInfo> files)
        {
            File = files;
            _seekPos = 0;
            _autoUpdate = true;
            _fileOk = true;
            _filePos = 0;
            _timer = new Timer(Scan, this, 0, Timeout.Infinite);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public delegate void LinesChangedEventHandler(object? sender, LinesChangedEventArgs e);
        public event LinesChangedEventHandler? LinesChanged;

        public List<FileInfo> File { get; }

        public string Title
        {
            get
            {
                if (File.Count == 1)
                {
                    return File[0].Name;
                }
                else
                {
                    return "multiple";

                }
            }
        }

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
                bytes = [];
                temp = [];
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
            // scan all files, refresh on the latest only
            for (; _filePos < File.Count; ++_filePos)
            {
                _seekPos = 0;
                _bytesAvail = 0;
                Scan(File[_filePos], (args) => { LinesChanged?.Invoke(this, args); });
            }
            (bool fileok, bool tracking) = Scan(File[^1], (args) => { LinesChanged?.Invoke(this, args); });

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

        public class TimedUiUpdate(int ms, Action<LinesChangedEventArgs> uiUpdate)
        {
            private readonly int _ms = ms;
            private readonly Action<LinesChangedEventArgs> _uiUpdate = uiUpdate;
            DateTime _time = DateTime.Now;
            private bool _updateRunning = false;
            private LinesChangedEventArgsAction _uiUpdateAction = LinesChangedEventArgsAction.None;

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

        private (bool, bool) Scan(FileInfo file, Action<LinesChangedEventArgs> uiUpdate)
        {
            TimedUiUpdate uiUpdateTimer = new(UiRefreshDelayMs, uiUpdate);
            try
            {
                file.Refresh();
                if (file.Exists)
                {
                    if (_seekPos < 0)
                    {
                        _seekPos = 0;
                        _lines.Clear();
                        uiUpdateTimer.Notify(LinesChangedEventArgsAction.Reset);
                    }
                    if (file.Length == _seekPos)
                    {
                        return (true, true);
                    }
                    if (file.Length < _seekPos)
                    {
                        _seekPos = -1;
                        _bytesAvail = 0;
                        return (true, false);
                    }
                    int bytesAvailPref = _bytesAvail;

                    using FileStream fileStream = System.IO.File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
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
                _lines.Add(new ClefLine(long.MaxValue, [new KeyValuePair<string, JsonNode?>("@m", JsonValue.Create(e.ToString()))]));
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
                int tot = filter.Value.Item2.Aggregate(0, (i, j) => i + j.Value, x => x);
                int missing = anz - tot;
                if (missing > 0)
                {
                    filter.Value.Item2.AddOrUpdate("", missing, (s, i) => { return i + missing; });
                }
            }
            foreach (var filter in Data)
            {
                int tot = filter.Value.Aggregate(0, (i, j) => i + j.Value, x => x);
                int missing = anz - tot;
                if (missing > 0)
                {
                    filter.Value.AddOrUpdate("", missing, (s, i) => { return i + missing; });
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
            catch (Exception ex)
            {
                return
                [
                    new KeyValuePair<string, JsonNode?>("@m", JsonValue.Create(ex.Message))
                ];
            }
        }
    }
}
