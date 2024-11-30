using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;

namespace compact_log_browser.Model
{
    public class Clef : IDisposable, INotifyPropertyChanged
    {
        private bool _disposedValue;
        private FileStream? _fileStream;
        // we can load the whole file in mem, perf issue is about gui <=> model binding (max logfile size is typically 20mb)
        //private List<(int, int)> _lineStartEnd = new List<(int, int)>();
        private List<JsonObject?> _lines = new List<JsonObject?>();
        private Dictionary<string, (string, HashSet<string>)> _properties = new Dictionary<string, (string, HashSet<string>)>();
        private Dictionary<string, HashSet<string>> _data = new Dictionary<string, HashSet<string>>();


        private Dictionary<string, string> _indexableProperties = new Dictionary<string, string> {
            {"@l","Level"},
            {"@i","Event Id"}
        };

        public Clef(FileInfo file)
        {
            FileName = file.Name;
            if (file.Exists)
            {
                _fileStream = File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                _fileStream.Seek(0, SeekOrigin.Begin);
            }
            FilePath = file.FullName;
            Scan();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FilePath { get; }
        public string FileName { get; }

        public List<JsonObject?> Lines => _lines;



        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _fileStream?.Close();
                    _fileStream?.Dispose();
                }
                _disposedValue = true;
                //_lineStartEnd.Clear();
                _lines.Clear();
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

        public Dictionary<string, (string, HashSet<string>)> Properties { get => _properties; }
        public Dictionary<string, HashSet<string>> Data { get => _data; }


        private void Scan()
        {
            if (_fileStream == null)
            {
                return;
            }
            byte[] bytes = new byte[1024 * 1024];
            byte[] temp = new byte[1024 * 1024];
            int bytesAvail = 0;
            int bytesRead;
            int start = 0;
            while ((bytesRead = _fileStream.Read(bytes, bytesAvail, bytes.Length - bytesAvail)) > 0)
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
                        JsonObject? logline = Scan(bytes, start, endline);
                        _lines.Add(logline);
                        //_lineStartEnd.Add((start, endline));
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
                        dat = _fileStream.ReadByte();
                    } while (dat > 0 && dat != '\n');
                    if (dat == '\n')
                    {
                        // clear buffer and scan to the next line
                        bytesAvail = 0;
                        dat = _fileStream.ReadByte();
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
            }
            if (bytesAvail > 0)
            {
                // end of file. add rest in case there is no newline at the end
                _lines.Add(Scan(bytes, start, start + bytesAvail));
                //_lineStartEnd.Add((start, start + bytesAvail));
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
                                (string, HashSet<string>) values;
                                if (!_properties.TryGetValue(kv.Key, out values))
                                {
                                    values = (_indexableProperties[kv.Key], new HashSet<string>());
                                    _properties.Add(kv.Key, values);
                                    values.Item2.Add("");
                                }
                                values.Item2.Add(value.ToString());
                            }
                        }
                        {
                            if (!kv.Key.StartsWith("@") && kv.Value is JsonValue value)
                            {
                                HashSet<string>? values;
                                if (!_data.TryGetValue(kv.Key, out values))
                                {
                                    values = new HashSet<string>();
                                    _data.Add(kv.Key, values);
                                    values.Add("");
                                }
                                values.Add(value.ToString());
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
