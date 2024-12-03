using System.Collections.ObjectModel;
using System.Windows.Input;
using clef_inspect.Model;

namespace clef_inspect.ViewModel.ClefView
{
    public partial class ClefFilterViewModel
    {
        private readonly Filter _filter;


        public ClefFilterViewModel(string name, Filter filter)
        {
            Name = name;
            _filter = filter;
            CheckAll = new CheckAllCommand(filter);
            CheckNone = new CheckNoneCommand(filter);
        }
        public ICommand CheckAll { get; }
        public ICommand CheckNone { get; }


        public string Name { get; set; }

        public ObservableCollection<FilterValue> FilterValues => _filter.Values;
    }
}
