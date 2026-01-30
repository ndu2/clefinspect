using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace ndu.ClefInspect.Model
{
    public delegate void LinesChangedEventHandler(object? sender, LinesChangedEventArgs e);
    public interface IClef : IDisposable, INotifyPropertyChanged
    {
        event LinesChangedEventHandler? LinesChanged;
        string Title { get; }
        bool AutoUpdate { get; set; }
        List<FileInfo> File { get; }
        bool FileOk { get; }
        long SeekPos { get; }
        int Count { get; }
        ConcurrentDictionary<string, (string, ConcurrentDictionary<string, int>)> Properties { get; }
        ConcurrentDictionary<string, ConcurrentDictionary<string, int>> Data { get; }
        ReadOnlyCollection<PinPreset> PinPresets { get; }
        IList<ClefLine> ViewFrom(int first);
    }
}