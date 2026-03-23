using ndu.ClefInspect.Model;
using System.ComponentModel;
using System.Windows.Input;

namespace ndu.ClefInspect.ViewModel.ClefView
{
    public partial class ClefFilterViewModel
    {
        public class CheckVisibleCommand : ICommand
        {
            private readonly ClefFilterViewModel _vm;
            private readonly bool _only;
            private readonly bool _inv;
            private readonly Filter _filter;

            public CheckVisibleCommand(ClefFilterViewModel vm, Filter filter, bool only, bool inv)
            {
                _vm = vm;
                _only = only;
                _inv = inv;
                _filter = filter;
                PropertyChangedEventManager.AddHandler(_filter, (s, e) => { CanExecuteChanged?.Invoke(this, EventArgs.Empty); }, nameof(filter.AcceptsNone));
                PropertyChangedEventManager.AddHandler(_filter, (s, e) => { CanExecuteChanged?.Invoke(this, EventArgs.Empty); }, nameof(filter.AcceptsAll));
            }

            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                if (_only)
                {
                    return true;
                }
                else if (_inv)
                {
                    return !_filter.AcceptsNone;
                }
                else
                {
                    return !_filter.AcceptsAll;
                }
            }

            public void Execute(object? parameter)
            {
                foreach (ClefFilterView fv in _vm.FilterValues)
                {
                    if (_only || fv.Visible)
                    {
                        fv.FilterValue.Enabled = fv.Visible ^ _inv;
                    }
                }
            }
        }

    }
}
