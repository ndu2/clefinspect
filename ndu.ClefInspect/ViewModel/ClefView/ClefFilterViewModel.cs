using ndu.ClefInspect.Model;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;

namespace ndu.ClefInspect.ViewModel.ClefView
{
    public partial class ClefFilterViewModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<ClefFilterView> _filterValues;
        private readonly ObservableCollection<FilterValue> _filter;
        private string _searchFilter;
        private bool _visible;


        public class ClearSearchFilterCommand : ICommand
        {
            private readonly ClefFilterViewModel _vm;

            public ClearSearchFilterCommand(ClefFilterViewModel vm)
            {
                _vm = vm;
                PropertyChangedEventManager.AddHandler(_vm, (s, e) => { CanExecuteChanged?.Invoke(this, EventArgs.Empty); }, nameof(vm.SearchFilter));
            }

            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return _vm.SearchFilter != string.Empty;
            }

            public void Execute(object? parameter)
            {
                _vm.SearchFilter = string.Empty;
            }
        }

        public ClefFilterViewModel(string name, Filter filter, bool visible)
        {
            Name = name;
            _visible = visible;
            _filter = filter.Values;
            _filterValues = [];

            CheckAll = new CheckAllCommand(filter);
            CheckNone = new CheckNoneCommand(filter);
            CheckVisible = new CheckVisibleCommand(this, filter, true, false);
            CheckAddVisible = new CheckVisibleCommand(this, filter, false, false);
            CheckRemoveVisible = new CheckVisibleCommand(this, filter, false, true);
            ChangeVisibility = new ChangeVisibilityCommand(this, filter);
            ClearSearchFilter = new ClearSearchFilterCommand(this);
            CollectionChangedEventManager.AddHandler(filter.Values, FilterCollectionChanged);
            _searchFilter = string.Empty;
            DisplayAll();
        }

        private void DisplayAll()
        {
            _filterValues.Clear();
            foreach (FilterValue filterValue in _filter)
            {
                _filterValues.Add(new ClefFilterView(this, filterValue));
            }
        }

        private void FilterCollectionChanged(object? s, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Add)
            {
                for(int i = _filterValues.Count; i < _filter.Count; ++i)
                {
                    _filterValues.Add(new ClefFilterView(this, _filter[i]));
                }
            }
            else
            {
                DisplayAll();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand CheckAll { get; }
        public ICommand CheckNone { get; }
        public ICommand CheckVisible { get; }
        public ICommand CheckAddVisible { get; }
        public ICommand ChangeVisibility { get; }
        public ICommand CheckRemoveVisible { get; }

        public string Name { get; set; }

        public ObservableCollection<ClefFilterView> FilterValues => _filterValues;
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

        public ICommand ClearSearchFilter { get; }
        public string SearchFilter
        {
            get => _searchFilter;
            set
            {
                if (_searchFilter != value)
                {
                    _searchFilter = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchFilter)));
                }
            }
        }

    }
}
