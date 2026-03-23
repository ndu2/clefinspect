using ndu.ClefInspect.Model;
using System.ComponentModel;

namespace ndu.ClefInspect.ViewModel.ClefView
{
    public partial class ClefFilterViewModel
    {
        public class ClefFilterView : INotifyPropertyChanged
        {
            private bool _visible;
            private readonly ClefFilterViewModel _vm;

            public ClefFilterView(ClefFilterViewModel vm, FilterValue filterValue)
            {
                _vm = vm;
                FilterValue = filterValue;
                Eval();
                PropertyChangedEventManager.AddHandler(_vm, Eval, nameof(_vm.SearchFilter));
            }

            private void Eval(object? sender, PropertyChangedEventArgs e)
            {
                Eval();
            }

            private void Eval()
            {
                Visible = FilterValue.Value.Contains(_vm.SearchFilter, StringComparison.InvariantCultureIgnoreCase);
            }

            public event PropertyChangedEventHandler? PropertyChanged;


            public FilterValue FilterValue { get; }
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
}
