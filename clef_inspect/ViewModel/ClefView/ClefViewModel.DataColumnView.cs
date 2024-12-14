namespace clef_inspect.ViewModel.ClefView
{
    public partial class ClefViewModel
    {
        public class DataColumnView
        {
            private bool _enabled;
            private Action _enabledChanged;
            public DataColumnView(string header, bool enabled, Action enabledChanged)
            {
                Header = header;
                Enabled = enabled;
                _enabledChanged = enabledChanged;
            }

            public string Header { get; }
            public bool Enabled
            {
                get => _enabled;
                set
                {
                    if(_enabled != value)
                    {
                        _enabled = value;
                        _enabledChanged.Invoke();
                    }
                }
            }
        }
    }
}
