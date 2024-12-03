using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows;

namespace clef_inspect.Model
{
    public class Clef : IDisposable, INotifyPropertyChanged
    {
        private const int FileRefreshDelayMs = 1000;
        private const int BufferSize = 10 * 1024 * 1024;
        private bool _disposedValue;
        private ConcurrentStack<JsonObject?> _lines = new ConcurrentStack<JsonObject?>();
        private ConcurrentDictionary<string, (string, ConcurrentDictionary<string, int>)> _properties = new ConcurrentDictionary<string, (string, ConcurrentDictionary<string, int>)>();
        private ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _data = new ConcurrentDictionary<string, ConcurrentDictionary<string, int>>();
        private long _seekPos;
        private Timer _timer;
        private bool _autoUpdate;
        private bool _fileOk;

        private byte[] bytes = new byte[BufferSize];
        private byte[] temp = new byte[BufferSize];
        private int _bytesAvail = 0;

        private Dictionary<string, string> _indexableProperties = new Dictionary<string, string> {
            {"@l","Level"},
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

        public FileInfo File { get; }

        public string FilePath { get => File.FullName; }

        public string FileName { get => File.Name; }

        public long SeekPos { get => _seekPos; }
        public ConcurrentStack<JsonObject?> Lines => _lines;



        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                }
                _lines.Clear();
                _properties.Clear();
                _data.Clear();
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

        private void Scan(object? state)
        {
            (bool fileok, bool tracking) = Scan( () => { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Lines))); });
            if (!tracking)
            {
                Application.Current.Dispatcher.Invoke(() => { AutoUpdate = false; });
            }
            if (fileok != FileOk)
            {
                Application.Current.Dispatcher.Invoke(() => { FileOk = fileok; });
            }
            _timer.Change(AutoUpdate? FileRefreshDelayMs:Timeout.Infinite, Timeout.Infinite);
        }

        public class TimedUiUpdate
        {
            private int _ms;
            private Action _uiUpdate;
            DateTime _time;
            private bool _updateRunning;
            public TimedUiUpdate(int ms, Action uiUpdate)
            {
                _ms = ms;
                _uiUpdate = uiUpdate;
                _time = DateTime.Now;
                _updateRunning = false;
            }
            public void ForceUpdate(Action preAction)
            {
                preAction();
                Application.Current.Dispatcher.Invoke(_uiUpdate);
            }
            public void CheckAndUpdate(Action preAction)
            {
                DateTime now = DateTime.Now;
                if (!_updateRunning && (now - _time).TotalMilliseconds > _ms)
                {
                    _time = now;
                    preAction();
                    _updateRunning = true;
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        _uiUpdate();
                        _updateRunning = false;
                    });
                }
            }
        }

        private (bool, bool) Scan(Action uiUpdate)
        {
            bool changed = false;
            TimedUiUpdate uiUpdateTimer = new TimedUiUpdate(250, uiUpdate);
            try
            {
                File.Refresh();
                if (File.Exists)
                {
                    if (_seekPos == 0)
                    {
                        _lines.Clear();
                        changed = true;
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

                    using (FileStream fileStream = System.IO.File.Open(File.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        fileStream.Seek(_seekPos, SeekOrigin.Begin);
                        int bytesRead;
                        int start = 0;
                        while ((bytesRead = fileStream.Read(bytes, _bytesAvail, bytes.Length - _bytesAvail)) > 0)
                        {
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
                                            // replace last entry
                                            _lines.TryPop(out _);
                                            JsonObject? logline = Scan(bytes, start, endline);
                                            _lines.Push(logline);
                                            changed = true;
                                        }
                                        bytesAvailPref = 0;
                                    }
                                    else
                                    {
                                        JsonObject? logline = Scan(bytes, start, endline);
                                        _lines.Push(logline);
                                        changed = true;
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
                                _bytesAvail = _bytesAvail - start;
                                Array.Copy(bytes, start, temp, 0, _bytesAvail);
                                Array.Copy(temp, bytes, _bytesAvail);
                            }
                            _seekPos = fileStream.Position;
                            uiUpdateTimer.CheckAndUpdate(fillDefaultFilterStats);
                        }
                        if (_bytesAvail > 0)
                        {

                            if (bytesAvailPref > 0)
                            {
                                // replace last entry
                                _lines.TryPop(out _);
                                bytesAvailPref = 0;
                            }
                            // end of file. add rest in case there is no newline at the end
                            _lines.Push(Scan(bytes, start, start + _bytesAvail));
                            changed = true;
                            Array.Copy(bytes, start, temp, 0, _bytesAvail);
                            Array.Copy(temp, bytes, _bytesAvail);
                        }
                    }
                }
                else
                {
                    return (false, false);
                }
            }
            catch (Exception e)
            {
                _lines.Push(new JsonObject
                    {
                        new KeyValuePair<string, JsonNode?>("@m", JsonValue.Create(e.ToString()))
                    });
                changed = true;
            }
            finally
            {
                if (changed)
                {
                    uiUpdateTimer.ForceUpdate(fillDefaultFilterStats);
                }
            }
            return (true, true);
        }

        private void fillDefaultFilterStats()
        {
            int anz = Lines.Count;
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
                if (!(line.StartsWith("{") && line.EndsWith("}")))
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
                                    ConcurrentDictionary<string, int> newSet = new ConcurrentDictionary<string, int>();
                                    return (_indexableProperties[k], newSet);


                                });
                                values.Item2.AddOrUpdate(value.ToString(), 1, (s, c) => c + 1);
//                                values.Item2.TryAdd(value.ToString(), (byte)0);
                            }
                        }
                        {
                            if (!kv.Key.StartsWith("@") && kv.Value is JsonValue value)
                            {
                                ConcurrentDictionary<string, int> values = _data.GetOrAdd(kv.Key, (k) =>
                                {
                                    ConcurrentDictionary<string, int> newSet = new ConcurrentDictionary<string, int>();
                                    return newSet;
                                });
                                values.AddOrUpdate(value.ToString(), 1, (s, c) => c + 1);
                                //values.TryAdd(value.ToString(), (byte)0);
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
