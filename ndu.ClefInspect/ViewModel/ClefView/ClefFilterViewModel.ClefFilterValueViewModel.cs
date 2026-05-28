using ndu.ClefInspect.Model;
using System.ComponentModel;

namespace ndu.ClefInspect.ViewModel.ClefView
{
    public partial class ClefFilterViewModel
    {
        public class ClefFilterValueViewModel : INotifyPropertyChanged
        {
            private bool _visible;
            private readonly ClefFilterViewModel _vm;

            public ClefFilterValueViewModel(ClefFilterViewModel vm, FilterValue filterValue)
            {
                _vm = vm;
                FilterValue = filterValue;
            }

            public event PropertyChangedEventHandler? PropertyChanged;


            public FilterValue FilterValue { get; }
            public bool Visible
            {
                get => FilterValue.Value.Contains(_vm.SearchFilter, StringComparison.InvariantCultureIgnoreCase);
            }

        }

    }
}
