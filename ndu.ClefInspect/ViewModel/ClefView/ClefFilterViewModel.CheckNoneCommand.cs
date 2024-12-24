using ndu.ClefInspect.Model;
using System.ComponentModel;
using System.Windows.Input;

namespace ndu.ClefInspect.ViewModel.ClefView
{
    public partial class ClefFilterViewModel
    {
        private class CheckNoneCommand : ICommand
        {
            private readonly Filter _filter;

            public CheckNoneCommand(Filter filter)
            {
                _filter = filter;
                PropertyChangedEventManager.AddHandler(_filter, (s, e) => { CanExecuteChanged?.Invoke(this, EventArgs.Empty); }, nameof(_filter.AcceptsNone));
            }
            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return !_filter.AcceptsNone;
            }

            public void Execute(object? parameter)
            {
                _filter.UncheckAll();
            }
        }
    }
}
