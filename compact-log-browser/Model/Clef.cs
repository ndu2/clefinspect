using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows;

namespace compact_log_browser.Model
{
    public class Clef : IDisposable, INotifyPropertyChanged
    {
        private const int FileRefreshDelayMs = 1000;
        private const int BufferSize = 10 * 1024 * 1024;
        private bool _disposedValue;
        private ConcurrentStack<JsonObject?> _lines = new ConcurrentStack<JsonObject?>();
        private ConcurrentDictionary<string, (string, ConcurrentDictionary<string, byte>)> _properties = new ConcurrentDictionary<string, (string, ConcurrentDictionary<string, byte>)>();
        private ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _data = new ConcurrentDictionary<string, ConcurrentDictionary<string, byte>>();
        private long _seekPos;
        private Timer _timer;
        private bool _autoUpdate;

        private byte[] bytes = new byte[BufferSize];
        private byte[] temp = new byte[BufferSize];
        private int bytesAvail = 0;

        private Dictionary<string, string> _indexableProperties = new Dictionary<string, string> {
            {"@l","Level"},
            {"@i","Event Id"}
        };

        public Clef(FileInfo file)
        {
            this.File = file;
            _seekPos = 0;
            _autoUpdate = true;
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

        public ConcurrentDictionary<string, (string, ConcurrentDictionary<string, byte>)> Properties { get => _properties; }
        public ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> Data { get => _data; }
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

        private void Scan(object? state)
        {
            bool tracking = Scan(
                () => {
                    Application.Current.Dispatcher.Invoke(() => { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Lines))); });
                });
            if (!tracking)
            {
                Application.Current.Dispatcher.Invoke(() => { AutoUpdate = false; });
            }
            _timer.Change(AutoUpdate? FileRefreshDelayMs:Timeout.Infinite, Timeout.Infinite);
        }

        private bool Scan(Action uiUpdate)
        {
            File.Refresh();
            if (File.Exists)
            {
                if(_seekPos == 0)
                {
                    _lines.Clear();
                }
                if (File.Length == _seekPos)
                {
                    return true;
                }
                if(File.Length < _seekPos)
                {
                    _seekPos = 0;
                    return false;
                }
                int bytesAvailPref = bytesAvail;
                try
                {
                    using (FileStream fileStream = System.IO.File.Open(File.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        fileStream.Seek(_seekPos, SeekOrigin.Begin);
                        int bytesRead;
                        int start = 0;
                        while ((bytesRead = fileStream.Read(bytes, bytesAvail, bytes.Length - bytesAvail)) > 0)
                        {
                            start = 0;
                            bytesAvail += bytesRead;
                            for (int i = 0; i < bytesAvail; ++i)
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
                                        }
                                        bytesAvailPref = 0;
                                    }
                                    else
                                    {
                                        JsonObject? logline = Scan(bytes, start, endline);
                                        _lines.Push(logline);
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
                                    bytesAvail = 0;
                                    dat = fileStream.ReadByte();
                                    if (dat != '\r')
                                    {
                                        bytes[0] = (byte)dat;
                                        bytesAvail = 1;
                                    }
                                }
                            }
                            else
                            {
                                bytesAvail = bytesAvail - start;
                                Array.Copy(bytes, start, temp, 0, bytesAvail);
                                Array.Copy(temp, bytes, bytesAvail);
                            }
                            _seekPos = fileStream.Position;
                            uiUpdate();
                        }
                        _seekPos = fileStream.Position;
                        if (bytesAvail > 0)
                        {

                            if (bytesAvailPref > 0)
                            {
                                // replace last entry
                                _lines.TryPop(out _);
                                bytesAvailPref = 0;
                            }
                            // end of file. add rest in case there is no newline at the end
                            _lines.Push(Scan(bytes, start, start + bytesAvail));
                            Array.Copy(bytes, start, temp, 0, bytesAvail);
                            Array.Copy(temp, bytes, bytesAvail);
                        }
                    }
                }
                catch (Exception e)
                {
                    _lines.Push(new JsonObject
                    {
                        new KeyValuePair<string, JsonNode?>("@m", JsonValue.Create(e.ToString()))
                    });
                }
            }
            uiUpdate();
            return true;
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
                                (string, ConcurrentDictionary<string, byte>) values = _properties.GetOrAdd(kv.Key, (k) =>
                                {
                                    ConcurrentDictionary<string, byte> newSet = new ConcurrentDictionary<string, byte>();
                                    newSet.TryAdd("", (byte)0);
                                    return (_indexableProperties[k], newSet);


                                });
                                values.Item2.TryAdd(value.ToString(), (byte)0);
                            }
                        }
                        {
                            if (!kv.Key.StartsWith("@") && kv.Value is JsonValue value)
                            {
                                ConcurrentDictionary<string, byte> values = _data.GetOrAdd(kv.Key, (k) =>
                                {
                                    ConcurrentDictionary<string, byte> newSet = new ConcurrentDictionary<string, byte>();
                                    newSet.TryAdd("", (byte)0);
                                    return newSet;
                                });
                                values.TryAdd(value.ToString(), (byte)0);
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
