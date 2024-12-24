using System.ComponentModel;

namespace ndu.ClefInspect.ViewModel.ClefView
{
    public partial class ClefViewModel
    {
        public class DataColumnView : INotifyPropertyChanged
        {
            private bool _enabled;

            public event PropertyChangedEventHandler? PropertyChanged;

            public DataColumnView(string header, bool enabled)
            {
                Header = header;
                Enabled = enabled;
            }

            public string Header { get; }
            public bool Enabled
            {
                get => _enabled;
                set
                {
                    if (_enabled != value)
                    {
                        _enabled = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Enabled)));
                    }
                }
            }
        }
    }
}
