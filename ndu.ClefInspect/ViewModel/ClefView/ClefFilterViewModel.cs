using ndu.ClefInspect.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace ndu.ClefInspect.ViewModel.ClefView
{
    public partial class ClefFilterViewModel : INotifyPropertyChanged
    {
        private readonly Filter _filter;
        private bool _visible;

        public ClefFilterViewModel(string name, Filter filter, bool visible)
        {
            Name = name;
            _filter = filter;
            _visible = visible;
            CheckAll = new CheckAllCommand(filter);
            CheckNone = new CheckNoneCommand(filter);
            ChangeVisibility = new ChangeVisibilityCommand(this, filter);
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand CheckAll { get; }
        public ICommand CheckNone { get; }
        public ICommand ChangeVisibility { get; }

        public string Name { get; set; }

        public ObservableCollection<FilterValue> FilterValues => _filter.Values;
        public bool Visible
        {
            get => _visible;
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Visible)));
                }
            }
        }
    }
}
